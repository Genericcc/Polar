using _Scripts._Game.Managers;

using Cinemachine;
using UnityEngine;

using Zenject;

namespace _Scripts
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] 
        private float minFollowYOffset = 2f;
        
        [SerializeField] 
        private float maxFollowYOffset = 12f;
        
        [SerializeField] 
        private float moveSpeed = 10f;
        
        [SerializeField] 
        private float rotationSpeed = 100f;
        
        [SerializeField] 
        private float zoomAmount = 1f;
        
        [SerializeField] 
        private float zoomSpeed = 5f;

        [SerializeField] 
        private CinemachineVirtualCamera cinemachineVirtualCamera;
        
        private InputReader _inputReader;

        private CinemachineTransposer _cinemachineTransposer;
        private Vector3 _targetFollowOffset;

        [Inject]
        public void Construct(InputReader inputReader)
        {
            _inputReader = inputReader;
        }

        private void Start()
        {
            _cinemachineTransposer = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
            _targetFollowOffset = _cinemachineTransposer.m_FollowOffset;
            
            _inputReader.EnablePlayerActions();
        }

        private void Update()
        {
            HandleMovement();
            HandleRotation();
            HandleZoom();
        }

        private void HandleMovement()
        {
            if (_inputReader.CameraMoveDir == Vector3.zero)
            {
                return;
            }
            
            var cameraTransform = transform;
            var moveDir = cameraTransform.forward * _inputReader.CameraMoveDir.y + cameraTransform.right * _inputReader.CameraMoveDir.x;
            cameraTransform.position += moveDir * (moveSpeed * Time.deltaTime);
        }

        private void HandleRotation()
        {
            if (_inputReader.CameraRotationDir == Vector2.zero)
            {
                return;
            }
            
            var rotationVector = new Vector3(0, -_inputReader.CameraRotationDir.x, 0);
            transform.eulerAngles += rotationVector * (rotationSpeed * Time.deltaTime);
        }

        private void HandleZoom()
        {
            if (_inputReader.CameraZoomDir.y > 0)
            {
                _targetFollowOffset.y -= zoomAmount;
            }
            
            if (_inputReader.CameraZoomDir.y < 0)
            {
                _targetFollowOffset.y += zoomAmount;
            }

            _targetFollowOffset.y = Mathf.Clamp(_targetFollowOffset.y, minFollowYOffset, maxFollowYOffset);

            _cinemachineTransposer.m_FollowOffset =
                Vector3.Lerp(_cinemachineTransposer.m_FollowOffset, _targetFollowOffset, Time.deltaTime * zoomSpeed);
        }

    }
}