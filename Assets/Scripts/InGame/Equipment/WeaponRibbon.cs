using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Game
{
    /// <summary>
    /// 칼날 양 끝의 월드 궤적을 연결하고 시간에 따라 소멸하는 검기를 표시한다.
    /// </summary>
    [DefaultExecutionOrder(32000)]
    [DisallowMultipleComponent]
    public sealed class WeaponRibbon : MonoBehaviour
    {
        [Header("칼날 위치")]
        [Tooltip("기본 검의 메시 Transform. 지정한 두 끝점이 없으면 메시의 가장 긴 축을 사용한다.")]
        public Transform bladeSource;
        public Transform bladeStart;
        public Transform bladeEnd;
        [Tooltip("손잡이 쪽에서 칼끝까지의 비율. 기본 검에서는 손잡이 부분을 제외한다.")]
        [Range(0f, 1f)] public float bladeStartFraction = 0.25f;

        [Header("검기 모습")]
        public Material ribbonMaterial;
        [ColorUsage(true, true)] public Color ribbonColor = new Color(0.3f, 1.3f, 2f, 0.8f);
        [Tooltip("잔상이 사라지는 시간(게임 시간, 초).")]
        [Min(0.01f)] public float lifetime = 0.16f;
        [Tooltip("궤적을 추가하는 최소 이동 거리(월드 단위).")]
        [Min(0.001f)] public float minimumDistance = 0.008f;
        [Tooltip("순간이동 시 끊을 거리(월드 단위).")]
        [Min(0.01f)] public float breakDistance = 2f;
        [Range(4, 256)] public int maxSamples = 96;

        private readonly List<Vector3> _starts = new List<Vector3>(256);
        private readonly List<Vector3> _ends = new List<Vector3>(256);
        private readonly List<float> _times = new List<float>(256);
        private readonly List<Vector3> _vertices = new List<Vector3>(512);
        private readonly List<Color> _colors = new List<Color>(512);
        private readonly List<Vector2> _uvs = new List<Vector2>(512);
        private readonly List<int> _triangles = new List<int>(1530);
        private GameObject _ribbonObject;
        private Mesh _mesh;
        private MeshRenderer _renderer;
        private Vector3 _localStart;
        private Vector3 _localEnd;
        private bool _isEmitting;
        private bool _hasBlade;

        private void Awake()
        {
            ResolveBlade();
            if (!ribbonMaterial || !_hasBlade)
            {
                Debug.LogWarning("검기에는 머티리얼과 칼날 메시 또는 두 끝점이 필요하다.", this);
                return;
            }

            // 월드 좌표를 저장하므로 무기나 캐릭터의 이동을 과거 궤적에 다시 적용하지 않는다.
            _ribbonObject = new GameObject("Weapon Ribbon");
            _ribbonObject.layer = gameObject.layer;
            _ribbonObject.transform.SetParent(transform, false);
            _mesh = new Mesh { name = "Weapon Ribbon Mesh" };
            _mesh.MarkDynamic();
            _ribbonObject.AddComponent<MeshFilter>().sharedMesh = _mesh;
            _renderer = _ribbonObject.AddComponent<MeshRenderer>();
            _renderer.sharedMaterial = ribbonMaterial;
            _renderer.shadowCastingMode = ShadowCastingMode.Off;
            _renderer.receiveShadows = false;
            _renderer.enabled = false;
        }

        /// <summary>
        /// 명시한 끝점을 우선하며 기본 메시에서는 원점에 가까운 쪽을 손잡이로 정한다.
        /// </summary>
        private void ResolveBlade()
        {
            _hasBlade = bladeStart && bladeEnd;
            if (_hasBlade || !bladeSource)
            {
                return;
            }
            MeshFilter filter = bladeSource.GetComponent<MeshFilter>();
            if (!filter || !filter.sharedMesh)
            {
                return;
            }
            Bounds bounds = filter.sharedMesh.bounds;
            int axis = bounds.size.y > bounds.size.x ? 1 : 0;
            if (bounds.size.z > bounds.size[axis])
            {
                axis = 2;
            }
            Vector3 near = bounds.center;
            Vector3 far = bounds.center;
            near[axis] = bounds.min[axis];
            far[axis] = bounds.max[axis];
            if (near.sqrMagnitude > far.sqrMagnitude)
            {
                Vector3 temporary = near;
                near = far;
                far = temporary;
            }
            _localStart = Vector3.Lerp(near, far, bladeStartFraction);
            _localEnd = far;
            _hasBlade = bounds.size[axis] > 0.001f;
        }

        /// <summary>
        /// 이전 잔상을 지우고 현재 칼날 위치에서 생성을 시작한다.
        /// </summary>
        public void BeginTrail()
        {
            ClearTrail();
            if (!_mesh || !isActiveAndEnabled)
            {
                return;
            }
            _isEmitting = true;
            AddSample();
        }

        /// <summary>
        /// 새 정점 생성을 멈추고 기존 잔상은 수명이 끝날 때까지 유지한다.
        /// </summary>
        public void EndTrail()
        {
            _isEmitting = false;
        }

        /// <summary>
        /// 잔상과 생성 상태를 지워 장비 해제나 재사용 시 궤적 연결을 방지한다.
        /// </summary>
        public void ClearTrail()
        {
            _isEmitting = false;
            _starts.Clear();
            _ends.Clear();
            _times.Clear();
            if (_mesh)
            {
                _mesh.Clear();
                _renderer.enabled = false;
            }
        }

        private void LateUpdate()
        {
            if (!_mesh)
            {
                return;
            }
            float duration = Mathf.Max(0.01f, lifetime);
            while (_times.Count > 0 && Time.time - _times[0] >= duration)
            {
                RemoveOldest();
            }
            if (_isEmitting)
            {
                AddSample();
            }
            RebuildMesh(duration);
        }

        private void AddSample()
        {
            Vector3 start = bladeStart && bladeEnd ? bladeStart.position : bladeSource.TransformPoint(_localStart);
            Vector3 end = bladeStart && bladeEnd ? bladeEnd.position : bladeSource.TransformPoint(_localEnd);
            int last = _times.Count - 1;
            if (last >= 0)
            {
                float distance = Mathf.Max(Vector3.Distance(start, _starts[last]), Vector3.Distance(end, _ends[last]));
                if (distance > Mathf.Max(0.01f, breakDistance))
                {
                    _starts.Clear();
                    _ends.Clear();
                    _times.Clear();
                }
                else if (distance < Mathf.Max(0.001f, minimumDistance))
                {
                    return;
                }
            }
            while (_times.Count >= Mathf.Clamp(maxSamples, 4, 256))
            {
                RemoveOldest();
            }
            _starts.Add(start);
            _ends.Add(end);
            _times.Add(Time.time);
        }

        private void RemoveOldest()
        {
            _starts.RemoveAt(0);
            _ends.RemoveAt(0);
            _times.RemoveAt(0);
        }

        private void RebuildMesh(float duration)
        {
            _mesh.Clear();
            _renderer.enabled = _times.Count >= 2;
            if (!_renderer.enabled)
            {
                return;
            }
            _vertices.Clear();
            _colors.Clear();
            _uvs.Clear();
            _triangles.Clear();
            for (int i = 0; i < _times.Count; i++)
            {
                float fade = Mathf.Clamp01(1f - (Time.time - _times[i]) / duration);
                Color color = ribbonColor;
                color.a *= fade * fade;
                _vertices.Add(_ribbonObject.transform.InverseTransformPoint(_starts[i]));
                _vertices.Add(_ribbonObject.transform.InverseTransformPoint(_ends[i]));
                _colors.Add(color);
                _colors.Add(color);
                _uvs.Add(new Vector2(fade, 0f));
                _uvs.Add(new Vector2(fade, 1f));
                if (i > 0)
                {
                    int index = i * 2;
                    _triangles.Add(index - 2);
                    _triangles.Add(index - 1);
                    _triangles.Add(index);
                    _triangles.Add(index);
                    _triangles.Add(index - 1);
                    _triangles.Add(index + 1);
                }
            }
            _mesh.SetVertices(_vertices);
            _mesh.SetColors(_colors);
            _mesh.SetUVs(0, _uvs);
            _mesh.SetTriangles(_triangles, 0);
            _mesh.RecalculateBounds();
        }

        private void OnDisable()
        {
            ClearTrail();
        }

        private void OnDestroy()
        {
            if (_mesh)
            {
                Destroy(_mesh);
            }
        }
    }
}
