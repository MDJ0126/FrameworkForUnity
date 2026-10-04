using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 자식 메시의 기존 머티리얼을 유지하며 스텐실 외곽선의 표시 상태를 관리한다.
/// </summary>
[ExecuteAlways, DisallowMultipleComponent]
[AddComponentMenu("Rendering/Stencil Outline")]
public sealed class StencilOutline : MonoBehaviour
{
    [SerializeField] private bool _isOutlineEnabled = true;
    [SerializeField] private Color _outlineColor = Color.black;
    [SerializeField, Range(0f, 12f)] private float _outlineWidth = 3f;
    private Renderer[] _renderers;
    private Material _material;
    private readonly List<Material> _sourceMaterials = new List<Material>();
    internal static readonly HashSet<StencilOutline> ActiveOutlines = new HashSet<StencilOutline>();
    private static readonly int COLOR_ID = Shader.PropertyToID("_StencilOutlineColor");
    private static readonly int WIDTH_ID = Shader.PropertyToID("_StencilOutlineWidth");

    public bool IsOutlineEnabled => _isOutlineEnabled;

    /// <summary>
    /// 컴포넌트 활성 상태와 별개로 외곽선 표시 여부를 설정한다.
    /// </summary>
    public void SetOutlineEnabled(bool isEnabled)
    {
        _isOutlineEnabled = isEnabled;
    }

    /// <summary>
    /// 외곽선 색상과 화면 픽셀 기준 두께를 설정한다.
    /// </summary>
    public void SetOutlineStyle(Color color, float width)
    {
        _outlineColor = color;
        _outlineWidth = Mathf.Clamp(width, 0f, 12f);
    }

    /// <summary>
    /// 장비 교체 등으로 변경된 자식 렌더러 목록을 다시 수집한다.
    /// </summary>
    [ContextMenu("Refresh Renderers")]
    public void RefreshRenderers()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
    }

    private void OnEnable()
    {
        RefreshRenderers();
        ActiveOutlines.Add(this);
    }

    private void OnDisable()
    {
        ActiveOutlines.Remove(this);
        CoreUtils.Destroy(_material);
        _material = null;
    }

    private void OnTransformChildrenChanged()
    {
        RefreshRenderers();
    }

    internal void CollectDraws(Shader shader, Camera camera, Plane[] planes, List<StencilOutlineRendererFeature.DrawItem> draws)
    {
        if (!_isOutlineEnabled || _outlineWidth <= 0f || _outlineColor.a <= 0f) return;
        if (_material == null || _material.shader != shader)
        {
            CoreUtils.Destroy(_material);
            _material = CoreUtils.CreateEngineMaterial(shader);
        }
        _material.SetColor(COLOR_ID, _outlineColor);
        _material.SetFloat(WIDTH_ID, _outlineWidth);
        foreach (Renderer renderer in _renderers)
        {
            if (renderer == null || !renderer.enabled || renderer.forceRenderingOff || !renderer.gameObject.activeInHierarchy) continue;
            if ((camera.cullingMask & (1 << renderer.gameObject.layer)) == 0 || !GeometryUtility.TestPlanesAABB(planes, renderer.bounds)) continue;
            // 중첩된 컴포넌트가 같은 메시를 두 번 그리지 않도록 가장 가까운 소유자만 처리한다.
            if (renderer.GetComponentInParent<StencilOutline>() != this) continue;
            Mesh mesh = null;
            if (renderer is SkinnedMeshRenderer skinned) mesh = skinned.sharedMesh;
            else if (renderer is MeshRenderer && renderer.TryGetComponent(out MeshFilter filter)) mesh = filter.sharedMesh;
            if (mesh == null) continue;
            renderer.GetSharedMaterials(_sourceMaterials);
            for (int index = 0; index < mesh.subMeshCount && index < _sourceMaterials.Count; index++)
            {
                Material source = _sourceMaterials[index];
                // 투명 표면과 알파 컷아웃은 원본 실루엣을 재현할 수 없어 제외한다.
                if (source == null || source.renderQueue > 2500 || source.IsKeywordEnabled("_ALPHATEST_ON")) continue;
                draws.Add(new StencilOutlineRendererFeature.DrawItem(renderer, _material, index));
            }
        }
    }
}
