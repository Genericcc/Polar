using UnityEngine;
using UnityEngine.Experimental.Rendering;

using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace ComputeShaderTutorials.gitamend_CS_Tutorial
{
    public class CustomRenderFeature : ScriptableRendererFeature
    {
        public static CustomRenderFeature Instance { get; private set; }
        
        [SerializeField] 
        private ComputeShader _computeShader;

        private FogMapPass _fogMapPass;
        
        public override void Create()
        {
            _fogMapPass = new FogMapPass
            {
                renderPassEvent = RenderPassEvent.BeforeRendering,
            };
            Instance = this;
        }

        //Called once per frame per camera, this method injects 'ScriptableRenderPass' into the renderer 
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!SystemInfo.supportsComputeShaders || _computeShader == null)
            {
                return;
            }
            
            _fogMapPass.Setup(_computeShader);
            renderer.EnqueuePass(_fogMapPass);
        }
        
        protected override void Dispose(bool disposing)
        {
            _fogMapPass?.CleanUp();
        }
        
        public RTHandle GetFogMapHandle() => _fogMapPass?.FogMapHandle;
        
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
                public TextureHandle FogMapHandle;          //output fogMap texture 
                public BufferHandle LightSourceHandle;      //GPU-side handle
                public int LightSourceCount;
            }

            public void CleanUp()
            {
                _fogMapHandle?.Release();
                _fogMapHandle = null;

                _lightSourceBuffer?.Release();
                _lightSourceBuffer = null;
            }

            public override void RecordRenderGraph(RenderGraph graph, ContextContainer frameData)
            {
                for (var i = 0; i <= _lightSourceCount; i++)
                {
                    var t = Time.time * 0.5f + i * 0.1f;
                    var x = Mathf.PerlinNoise(t, i * 1.31f) * _width;
                    var y = Mathf.PerlinNoise(i * 0.91f, t) * _height;
                    _lightSourcePositions[i] = new Vector2(x, y);
                }
                
                _lightSourceBuffer.SetData(_lightSourcePositions);

                var fogMapTextureHandler = graph.ImportTexture(_fogMapHandle);
                var lightSourceHandler = graph.ImportBuffer(_lightSourceBuffer);
                
                using IComputeRenderGraphBuilder builder = graph.AddComputePass("FogMapPass", out PassData data);
                data.ComputeShader = _computeShader;
                data.Kernel = _kernel;
                data.FogMapHandle = fogMapTextureHandler;
                data.LightSourceHandle = lightSourceHandler;
                data.LightSourceCount = _lightSourceCount;
                
                builder.UseTexture(fogMapTextureHandler, AccessFlags.Write);
                builder.UseBuffer(lightSourceHandler, AccessFlags.Read);

                builder.SetRenderFunc
                ((PassData d, ComputeGraphContext ctx) =>
                    {
                        ctx.cmd.SetComputeIntParam(d.ComputeShader, "LightSourceCount", d.LightSourceCount);
                        ctx.cmd.SetComputeBufferParam(d.ComputeShader, d.Kernel, "LightSourcePositions", d.LightSourceHandle);
                        ctx.cmd.SetComputeTextureParam(d.ComputeShader, d.Kernel, "FogMapTexture", d.FogMapHandle);

                        ctx.cmd.DispatchCompute(
                            d.ComputeShader,
                            d.Kernel,
                            Mathf.CeilToInt(_width / 8f),
                            Mathf.CeilToInt(_height / 8f),
                            1
                        );
                    }
                );
            }
        }
    }
}


