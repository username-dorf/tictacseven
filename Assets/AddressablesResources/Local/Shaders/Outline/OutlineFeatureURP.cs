using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineFeatureURP : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public LayerMask outlineLayer = 0;

        public Shader maskShader;       // Hidden/Outline/MaskURP
        public Shader compositeShader;  // Hidden/Outline/CompositeURP

        [ColorUsage(false,true)] public Color color = new(0, 0.42f, 1f, 1f);
        [Range(1, 12)] public int   thicknessPx = 4;
        [Range(0, 2)]  public float softness    = 0.6f;
        [Range(0, 1)]  public float opacity     = 1.0f;

        public RenderPassEvent evt = RenderPassEvent.AfterRenderingOpaques;
        public bool debugMask = false;
    }
    public Settings settings = new();

    Material maskMat, compMat;
    RTHandle maskRT, copyRT;

    class MaskPass : ScriptableRenderPass
    {
        readonly Material mat;
        FilteringSettings filtering;
        static readonly ShaderTagId tagFwd   = new("UniversalForward");
        static readonly ShaderTagId tagUnlit = new("SRPDefaultUnlit");
        RTHandle colorTarget;
        RTHandle depthTarget;

        public MaskPass(LayerMask layer, Material m, RenderPassEvent e)
        {
            mat = m;
            filtering = new FilteringSettings(RenderQueueRange.opaque, layer);
            renderPassEvent = e;
        }

        public void SetTargets(RTHandle t,RTHandle depth)
        {
            colorTarget = t;
            depthTarget = depth;

        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor d)
        {
            ConfigureTarget(colorTarget, depthTarget);
            ConfigureClear(ClearFlag.Color, Color.black);
        }

        public override void Execute(ScriptableRenderContext ctx, ref RenderingData data)
        {
            if (!mat) return;

            var draw = CreateDrawingSettings(tagFwd, ref data, SortingCriteria.CommonOpaque);
            draw.SetShaderPassName(1, tagUnlit);
            draw.overrideMaterial = mat;

            var cmd = CommandBufferPool.Get("Outline Mask");
            ctx.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            ctx.DrawRenderers(data.cullResults, ref draw, ref filtering);
        }
    }

    class CompositePass : ScriptableRenderPass
    {
        readonly Material mat;
        readonly bool debug;

        RTHandle cameraColor, mask, copy;

        public CompositePass(Material m, RenderPassEvent e, bool debug)
        {
            mat = m;
            this.debug = debug;
            renderPassEvent = e + 1;
        }

        public void SetTargets(RTHandle color, RTHandle maskRT, RTHandle copyRT)
        {
            cameraColor = color; mask = maskRT; copy = copyRT;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor d)
        {
            ConfigureInput(ScriptableRenderPassInput.Depth);

            ConfigureTarget(cameraColor);
            ConfigureClear(ClearFlag.None, Color.black);
        }

        public override void Execute(ScriptableRenderContext ctx, ref RenderingData data)
        {
            if (!mat) return;

            var cmd = CommandBufferPool.Get("Outline Composite");

            Blitter.BlitCameraTexture(cmd, cameraColor, copy);

            CoreUtils.SetRenderTarget(cmd, cameraColor);
            cmd.SetGlobalTexture("_SourceTex", copy.nameID);
            cmd.SetGlobalTexture("_MaskTex",   mask.nameID);

            if (debug)
                Blitter.BlitCameraTexture(cmd, mask, cameraColor);
            else
                CoreUtils.DrawFullScreen(cmd, mat, null, 0);

            ctx.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    MaskPass     maskPass;
    CompositePass compPass;

    public override void Create()
    {
        if (settings.maskShader)      maskMat = CoreUtils.CreateEngineMaterial(settings.maskShader);
        if (settings.compositeShader) compMat = CoreUtils.CreateEngineMaterial(settings.compositeShader);

        maskPass = new MaskPass(settings.outlineLayer, maskMat, settings.evt);
        compPass = new CompositePass(compMat, settings.evt, settings.debugMask);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData data)
    {
        var maskDesc = data.cameraData.cameraTargetDescriptor;
        maskDesc.depthBufferBits = 0;
        maskDesc.msaaSamples     = 1;
        maskDesc.colorFormat     =
            SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.R8)
            ? RenderTextureFormat.R8
            : RenderTextureFormat.Default;
        RenderingUtils.ReAllocateIfNeeded(ref maskRT, maskDesc, name: "_OutlineMaskRT");

        var copyDesc = data.cameraData.cameraTargetDescriptor;
        copyDesc.depthBufferBits = 0;
        copyDesc.msaaSamples     = 1;
        copyDesc.colorFormat     = RenderTextureFormat.Default;
        RenderingUtils.ReAllocateIfNeeded(ref copyRT, copyDesc, name: "_OutlineCopyRT");

        maskPass.SetTargets(maskRT, renderer.cameraDepthTargetHandle);
        compPass.SetTargets(renderer.cameraColorTargetHandle, maskRT, copyRT);

        if (compMat)
        {
            compMat.SetColor ("_Color",      settings.color);
            compMat.SetFloat ("_ThicknessPx", settings.thicknessPx);
            compMat.SetFloat ("_Softness",    settings.softness);
            compMat.SetFloat ("_Opacity",     settings.opacity);
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData data)
    {
        if (!maskMat || !compMat) return;
        renderer.EnqueuePass(maskPass);
        renderer.EnqueuePass(compPass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(maskMat);
        CoreUtils.Destroy(compMat);
        maskRT?.Release();
        copyRT?.Release();
    }
}
