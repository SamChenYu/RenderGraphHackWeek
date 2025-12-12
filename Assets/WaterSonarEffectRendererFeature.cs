using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

// Manages the life cycle and configuration of the passes
// Manager class
public class WaterSonarRendererFeature : ScriptableRendererFeature
{
    [SerializeField] WaterSonarRendererFeatureSettings settings;
    WaterSonarRendererFeaturePass m_ScriptablePass;
    
    public RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingPostProcessing;
    public Material material;

    
    public override void Create()
    {
        // Creates an instance of the pass and injection point
        m_ScriptablePass = new WaterSonarRendererFeaturePass(settings);
        m_ScriptablePass.renderPassEvent = injectionPoint;
    }

    
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (material == null)
        {
            Debug.LogWarning("WaterSonarRendererFeature material is null and will be skipped.");
            return;
        }
        
        // Queue into the rendering pipeline
        m_ScriptablePass.Setup(material);
        renderer.EnqueuePass(m_ScriptablePass);
    }

    [Serializable]
    public class WaterSonarRendererFeatureSettings
    {
        
    }

    
    
    
    
    
    
    
    
    
    // Defines the rendering logic what the render pass does - drawing / rendering actions
    // Worker class
    class WaterSonarRendererFeaturePass : ScriptableRenderPass
    {
        private const string m_PassName = "WaterSonarPass"; // Useful for debugging
        private Material m_BlitMaterial; // Contains the shader you apply to the current state of the rendered image
        
        readonly WaterSonarRendererFeatureSettings settings;
        
        public WaterSonarRendererFeaturePass(WaterSonarRendererFeatureSettings settings)
        {   
            this.settings = settings;
        }


        //private class PassData
        //{
            // Don't actually need this
            // Defines the data used when we declare a pass the rendering pass we can access
            // This pass doesn't need any additional data
        //}


        public void Setup(Material mat)
        {
            // Allows the renderer feature to set the material for the dither effect
            m_BlitMaterial = mat;
            // It needs to read the current color texture of the scene so far
            // Ideally we want to apply the effect to the final output - we can't directly use the output (back buffer) as an input
            // Instead we'll need to work with an intermediate texture
            requiresIntermediateTexture = false;
        }
        
        
        // static void ExecutePass(PassData data, RasterGraphContext context)
        // {
        //     Just deleted
        // }

        
        // 
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            
            var stack = VolumeManager.instance.stack;
            var customEffect = stack.GetComponent<SphereVolumeComponent>(); // Created this type

            if (!customEffect.IsActive()) return;
            // Continue to inject the pass    
            
            // Future passes will reference our modified version without needing additional blit operations
            // Access point to the renderers texture handles
            var resourceData = frameData.Get<UniversalResourceData>();

            if (resourceData.isActiveTargetBackBuffer)
            {   
                // Ensure that resource data target backbuffer is false - otherwise we can't use the intermediate texture and log error
                Debug.LogError($"Skipping render pass. WaterSonarRendererFeature requires an intermediate ColorTexture, we can't use the BackBuffer as a texture input. ");
                return;
            }
            
            // The activeColorTexture - we'll use this texture in the blit operation and we also use this to create destination texture
            var source = resourceData.activeColorTexture;

            var destinationDesc = renderGraph.GetTextureDesc(source); // Match the source, e.g. dimensions
            destinationDesc.name = $"CameraColor-{m_PassName}"; // Name of the destination to fit the pass
            destinationDesc.clearBuffer = false; // Clear buffer to be false - we don't want a blank slate, we want to modify

            TextureHandle destination = renderGraph.CreateTexture(destinationDesc); // Creating the destination handle

            RenderGraphUtils.BlitMaterialParameters para = new(source, destination, m_BlitMaterial, 0);

            using (var builder = renderGraph.AddBlitPass(para, m_PassName, returnBuilder: true))
            {
                builder.UseTexture(resourceData.cameraDepthTexture, AccessFlags.Read);
            }
            
            // renderGraph.AddBlitPass(para, passName: m_PassName); // Execute the blit using the material
            
            // Dither effect blits the full screen draw to color buffer to a temporary texture
            // This swaps the camera color buffer with our modified texture
            resourceData.cameraColor = destination;


        }
    }
}
