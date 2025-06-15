using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ComputeShaderTutorials.gitamend_CS_Tutorial
{
    public class CustomRenderFeature : ScriptableRendererFeature
    {
        class FogMapPass : ScriptableRenderPass
        {
            ComputeShader _computeShader;
            int _kernel;
            
            GraphicsBuffer _lightSourceBuffer;
            Vector2[] _lightSourcePositions;
            int _lightSourceCount = 64;
            
            RTHandle _fogMapHandle;
            int _width = 256, _height = 256;
            
            public RTHandle FogMapHandle => _fogMapHandle;

            public void Setup(ComputeShader computeShader)
            {
                _computeShader = computeShader;
                _kernel = computeShader.FindKernel("CSMain");

                if (_fogMapHandle == null || _fogMapHandle.rt.width != _width || _fogMapHandle.rt.height != _height)
                {
                    _fogMapHandle?.Release();

                    var descriptor = new RenderTextureDescriptor(_width, _height, GraphicsFormat.R32_SFloat, 0)
                    {
                        enableRandomWrite = true,
                        msaaSamples = 1,
                        sRGB = false,
                        useMipMap = false
                    };
                    
                    _fogMapHandle = RTHandles.Alloc(descriptor, name: "_fogMapRT");
                }

                if (_lightSourceBuffer == null || _lightSourceBuffer.count != _lightSourceCount)
                {
                    _lightSourceBuffer?.Release();
                    _lightSourceBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _lightSourceCount, sizeof(float) * 2);
                    _lightSourcePositions = new Vector2[_lightSourceCount];
                }
            }

            class PassData
            {
                public ComputeShader ComputeShader;
                public int Kernel;
                public TextureHandle FogMapHandle; //output fogMap texture 
                public GraphicsBufferHandle LightSourceHandle; //GPU-side handle
                public int LightSourceCount;
            }

            public void RecordRenderGraph(RenderGraph renderGraph, ref RenderingData renderingData)
            {
                
            }
            
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                 
            }
        }
        
        //Called when the renderer feature is created by Unity
        public override void Create()
        {
            
        }

        //Called once per frame per camera, this method injects 'ScriptableRenderPass' into the renderer 
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            
        }
    }
}


