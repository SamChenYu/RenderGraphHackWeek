using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class RenderObjectsWithRenderListsRenderFeature : ScriptableRendererFeature
{
    class RenderObjectsWithRenderListCustomPass : ScriptableRenderPass
    {
        private List<ShaderTagId> shaderTagIDList = new List<ShaderTagId>();
        
        //Configure the properties from our pass here
        public void Setup(LayerMask layerMask, Material material)
        {
            
        }
        
        //We'll store the data that our RenderGraph pass needs here 
        private class MyPassData
        {
        }

        //We'll configure our Render List here: set up the shaders, filtering&drawing settings, etc. 
        private void InitRendererList(ContextContainer frameData, ref MyPassData passData, RenderGraph renderGraph)
        {
            // UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            // UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            // UniversalLightData lightData = frameData.Get<UniversalLightData>();
            //
            // var sortFlags = cameraData.defaultOpaqueSortFlags;
            // RenderQueueRange renderQueueRange = RenderQueueRange.opaque;
            //
            // int ignoreMask = ~layerMask;
            // FilteringSettings filteringSettings = new FilteringSettings(renderQueueRange, ignoreMask); //gathers all objects that can be rendered as normal
            //
            // ShaderTagId[] forwardOnlyShaderTagIds = new[] //LightMode ShaderLab pass
            // {
            //     new ShaderTagId("UniversalForwardOnly"),
            //     new ShaderTagId("UniversalForward")
            // };
            //
            // shaderTagIDList.Clear();
            //
            // foreach (var sid in forwardOnlyShaderTagIds)
            // {
            //     shaderTagIDList.Add(sid);
            // }
        }
        
        //Actually execute our draw commands. Will call it directly from RecordRenderGraph.
        static void ExecutePass(MyPassData data, RasterGraphContext context)
        {
        }
        
        //Here we tell RenderGraph our inputs our outputs & register our passes. We don't execute command buffers.
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            const string passName = "My Render Custom Pass";
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            
            // This adds a raster render pass to the graph, specifying the name and the data type that will be passed to the ExecutePass function.
            using (var builder = renderGraph.AddRasterRenderPass<MyPassData>(passName, out var passData))
            {
                
                
                // Set up our function that will actually run our command buffers. This is what RG runs when executing the pass. 
                builder.SetRenderFunc((MyPassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }
    }

    RenderObjectsWithRenderListCustomPass m_ScriptablePass;
    

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new RenderObjectsWithRenderListCustomPass();

        // Set up injection point
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }
}
