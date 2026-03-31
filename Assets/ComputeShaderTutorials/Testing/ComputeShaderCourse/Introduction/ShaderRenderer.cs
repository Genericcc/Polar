using Sirenix.OdinInspector;
using UnityEngine;

namespace ComputeShaderTutorials.Testing.ComputeShaderCourse.Introduction
{
    [ExecuteInEditMode]
    public class ShaderRenderer : MonoBehaviour
    {
        private const int _textureSize = 256;
        
        [SerializeField]
        private ComputeShader _computeShader;
        
        [SerializeField]
        private RenderTexture _outputTexture;
        
        [SerializeField]
        private Material _material;
        
        [SerializeField]
        private int _radius;

        [Button]
        private void Paint(string kernelName)
        {
            Setup();

            var kernelHandle = _computeShader.FindKernel(kernelName);
            _computeShader.SetInt("radius", _radius);
            
            DispatchShader(kernelHandle);
        }

        private void DispatchShader(int kernelHandle)
        {
            _computeShader.SetTexture(kernelHandle, "result", _outputTexture);
            _computeShader.SetInt("width", _textureSize);
            _computeShader.SetInt("height", _textureSize);

            _computeShader.Dispatch(kernelHandle, _textureSize / 8, _textureSize / 8, 1);
        }

        private void Setup()
        {
            if (_outputTexture == null)
            {
                _outputTexture = new RenderTexture(_textureSize, _textureSize, 0);
                _outputTexture.enableRandomWrite = true;
                _outputTexture.Create();
            }

            if (_material != null)
            {
                _material.SetTexture("BaseMap", _outputTexture);
            }
        }

        private void OnDestroy()
        {
            if (_outputTexture != null)
            {
                _outputTexture.Release();
            }
        }
    }
}