using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 등록된 메시의 실루엣과 확장 메시를 URP 카메라에 순서대로 그린다.
/// </summary>
public sealed class StencilOutlineRendererFeature : ScriptableRendererFeature
{
    [SerializeField] private Shader _shader;
    private Material _clearMaterial;
    private OutlinePass _pass;

    internal sealed class DrawItem
    {
        internal readonly Renderer renderer;
        internal readonly Material material;
        internal readonly int submesh;

        internal DrawItem(Renderer renderer, Material material, int submesh)
        {
            this.renderer = renderer;
            this.material = material;
            this.submesh = submesh;
        }
    }

    public override void Create()
    {
        CoreUtils.Destroy(_clearMaterial);
        if (_shader == null) _shader = Shader.Find("Hidden/Framework/StencilOutline");
        _clearMaterial = _shader != null ? CoreUtils.CreateEngineMaterial(_shader) : null;
        _pass = new OutlinePass { renderPassEvent = RenderPassEvent.AfterRenderingOpaques };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        CameraType type = renderingData.cameraData.cameraType;
        if (_clearMaterial == null || StencilOutline.ActiveOutlines.Count == 0 || (type != CameraType.Game && type != CameraType.SceneView)) return;
        _pass.shader = _shader;
        _pass.clearMaterial = _clearMaterial;
        renderer.EnqueuePass(_pass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(_clearMaterial);
        _clearMaterial = null;
    }

#pragma warning disable 618, 672
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        // Compatibility Mode에서도 카메라의 색상과 스텐실을 포함한 깊이 타깃을 함께 사용한다.
        _pass.ConfigureTarget(renderer.cameraColorTargetHandle, renderer.cameraDepthTargetHandle);
    }
#pragma warning restore 618, 672

    private sealed class OutlinePass : ScriptableRenderPass
    {
        internal Shader shader;
        internal Material clearMaterial;
        private readonly Plane[] _planes = new Plane[6];

        private sealed class PassData
        {
            internal List<DrawItem> draws;
            internal Material clearMaterial;
        }

        private List<DrawItem> CollectDraws(Camera camera)
        {
            var draws = new List<DrawItem>();
            GeometryUtility.CalculateFrustumPlanes(camera, _planes);
            foreach (StencilOutline outline in StencilOutline.ActiveOutlines)
            {
                if (outline != null) outline.CollectDraws(shader, camera, _planes, draws);
            }
            return draws;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var draws = CollectDraws(frameData.Get<UniversalCameraData>().camera);
            if (draws.Count == 0) return;
            var resources = frameData.Get<UniversalResourceData>();
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Stencil Outline", out var data))
            {
                data.draws = draws;
                data.clearMaterial = clearMaterial;
                builder.SetRenderAttachment(resources.activeColorTexture, 0, AccessFlags.ReadWrite);
                builder.SetRenderAttachmentDepth(resources.activeDepthTexture, AccessFlags.ReadWrite);
                builder.AllowPassCulling(false);
                builder.SetRenderFunc((PassData passData, RasterGraphContext context) =>
                {
                    context.cmd.DrawProcedural(Matrix4x4.identity, passData.clearMaterial, 2, MeshTopology.Triangles, 3);
                    foreach (DrawItem item in passData.draws) context.cmd.DrawRenderer(item.renderer, item.material, item.submesh, 0);
                    foreach (DrawItem item in passData.draws) context.cmd.DrawRenderer(item.renderer, item.material, item.submesh, 1);
                    context.cmd.DrawProcedural(Matrix4x4.identity, passData.clearMaterial, 2, MeshTopology.Triangles, 3);
                });
            }
        }

#pragma warning disable 618, 672
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var draws = CollectDraws(renderingData.cameraData.camera);
            if (draws.Count == 0) return;
            CommandBuffer cmd = CommandBufferPool.Get("Stencil Outline");
            cmd.DrawProcedural(Matrix4x4.identity, clearMaterial, 2, MeshTopology.Triangles, 3);
            foreach (DrawItem item in draws) cmd.DrawRenderer(item.renderer, item.material, item.submesh, 0);
            foreach (DrawItem item in draws) cmd.DrawRenderer(item.renderer, item.material, item.submesh, 1);
            cmd.DrawProcedural(Matrix4x4.identity, clearMaterial, 2, MeshTopology.Triangles, 3);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
#pragma warning restore 618, 672
    }
}
