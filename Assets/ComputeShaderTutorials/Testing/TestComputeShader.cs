using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace ComputeShaders
{
    [ExecuteInEditMode]
    public class NodeRenderer : MonoBehaviour
    {
        private const int _textureSize = 256;
        private const int _nodeCount = 3;
        
        [SerializeField]
        private ComputeShader _computeShader;
        
        [SerializeField]
        private RenderTexture _outputTexture;
        
        [SerializeField]
        private Material _material;
        
        [SerializeField]
        private float _radius;

        private ComputeBuffer _nodeBuffer;

        [Button]
        private void DoIt()
        {
            // Create and initialize the output texture
            if (_outputTexture == null)
            {
                _outputTexture = new RenderTexture(_textureSize, _textureSize, 0);
                _outputTexture.enableRandomWrite = true;
                _outputTexture.Create();
            }

            // Assign the render texture to the material
            if (_material != null)
            {
                _material.SetTexture("BaseMap", _outputTexture);
            }

            // Create three random nodes
            var nodes = new Node[_nodeCount];
            for (var i = 0; i < _nodeCount; i++)
            {
                var randomPosition = new Vector3(
                    Random.Range(0, _textureSize),
                    Random.Range(0, _textureSize),
                    0
                );
                nodes[i] = new Node(randomPosition, Color.white, _radius);
            }

            // Create and set up the compute buffer (3 for position, 4 for color, 1 for radius)
            _nodeBuffer = new ComputeBuffer(nodes.Length, sizeof(float) * 8);
            _nodeBuffer.SetData(nodes);

            // Clear the texture first (set to black)
            _computeShader.SetTexture(0, "Result", _outputTexture);
            _computeShader.SetBuffer(0, "nodeBuffer", _nodeBuffer);
            _computeShader.SetInt("width", _textureSize);
            _computeShader.SetInt("height", _textureSize);

            // Dispatch the shader for the entire texture
            _computeShader.Dispatch(0, _textureSize / 8, _textureSize / 8, 1);

            if (_nodeBuffer != null)
            {
                _nodeBuffer.Release();
            }
        }

        private void OnDestroy()
        {
            if (_nodeBuffer != null)
            {
                _nodeBuffer.Release();
            }

            if (_outputTexture != null)
            {
                _outputTexture.Release();
            }
        }
    }

    public struct Node
    {
        public Vector3 Position;
        public Color Color;
        public float Radius;

        public Node(Vector3 pos, Color col, float radius)
        {
            Position = pos;
            Color = col;
            Radius = radius;
        }
    }
}