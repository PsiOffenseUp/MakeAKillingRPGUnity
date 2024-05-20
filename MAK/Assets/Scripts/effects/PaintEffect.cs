using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

internal class PaintEffectFeature : ScriptableRendererFeature
{
    internal class PaintEffectPass : ScriptableRenderPass
    {
        ProfilingSampler m_ProfilingSampler = new ProfilingSampler("PaintEffect");
        Material m_Material;
        RTHandle m_CameraColorTarget;
        float m_KernelSize;

        public PaintEffectPass(Material material)
        {
            m_Material = material;
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }

        public void SetTarget(RTHandle colorHandle, float kernelSize)
        {
            m_CameraColorTarget = colorHandle;
            m_KernelSize = kernelSize;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureTarget(m_CameraColorTarget);
            //Blitter.Initialize();
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CameraData cameraData = renderingData.cameraData;

            if (cameraData.camera.cameraType != CameraType.Game)
                return;

            if (m_Material == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, m_ProfilingSampler))
            {
                m_Material.SetFloat("_KernelSize", m_KernelSize);
                //m_Material.SetTexture("_MainTex", m_CameraColorTarget);
                //Blitter.BlitCameraTexture(cmd, m_CameraColorTarget, m_CameraColorTarget, m_Material, 0);
                Blit(cmd, m_CameraColorTarget, m_CameraColorTarget, m_Material, 0);
            }
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }
    }

    //[SerializeField] Shader m_Shader;
    [SerializeField] Material m_Material;
    [SerializeField] float m_KernelSize;

    PaintEffectPass m_RenderPass = null;

    public override void AddRenderPasses(ScriptableRenderer renderer,
                                    ref RenderingData renderingData)
    {
        //if (renderingData.cameraData.cameraType == CameraType.Game)
            renderer.EnqueuePass(m_RenderPass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer,
                                        in RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            // Calling ConfigureInput with the ScriptableRenderPassInput.Color argument
            // ensures that the opaque texture is available to the Render Pass.
            m_RenderPass.ConfigureInput(ScriptableRenderPassInput.Color);
            m_RenderPass.SetTarget(renderer.cameraColorTargetHandle, m_KernelSize);
        }
    }

    public override void Create()
    {
        //m_Material = CoreUtils.CreateEngineMaterial(m_Shader);
        m_RenderPass = new PaintEffectPass(m_Material);
    }

    protected override void Dispose(bool disposing)
    {
        //CoreUtils.Destroy(m_Material);
    }
}