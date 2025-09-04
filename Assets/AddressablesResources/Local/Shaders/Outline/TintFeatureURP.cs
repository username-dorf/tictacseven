using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


//test render graph disabled. don't forget to add feature to URP asset
public class TintFeatureURP : ScriptableRendererFeature
{
    [System.Serializable] public class Settings
    {
        public Color color = new(1,0,0,0.35f);
        public RenderPassEvent evt = RenderPassEvent.AfterRendering;
    }
    public Settings settings = new();

    class TintPass : ScriptableRenderPass
    {
        Material mat; RTHandle colorTarget;
        static readonly string kTag = "TINT TEST";
        public TintPass(Material m, RenderPassEvent e){ mat = m; renderPassEvent = e; }
        public void SetTarget(RTHandle t){ colorTarget = t; }
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor d)
        {
            ConfigureTarget(colorTarget);
            ConfigureClear(ClearFlag.None, Color.black);
        }
        public override void Execute(ScriptableRenderContext ctx, ref RenderingData data)
        {
            if (!mat) return;
            var cmd = CommandBufferPool.Get(kTag);
            Blitter.BlitCameraTexture(cmd, colorTarget, colorTarget, mat, 0);
            ctx.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    Material _mat;
    TintPass _pass;

    public override void Create()
    {
        var shader = Shader.Find("Hidden/TintURP");
        _mat = CoreUtils.CreateEngineMaterial(shader);
        _pass = new TintPass(_mat, settings.evt);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData data)
    {
        _mat.SetColor("_Color", settings.color);
        _pass.SetTarget(renderer.cameraColorTargetHandle);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData data)
    {
        renderer.EnqueuePass(_pass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(_mat);
    }
}