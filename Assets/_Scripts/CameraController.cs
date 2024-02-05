using Cinemachine;
using UnityEngine;

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

        private CinemachineTransposer _cinemachineTransposer;
        private Vector3 _targetFollowOffset;

        private void Start()
        {
            _cinemachineTransposer = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
            _targetFollowOffset = _cinemachineTransposer.m_FollowOffset;
        }

        private void Update()
        {
            HandleMovement();
            HandleRotation();
            HandleZoom();
        }

        private void HandleMovement()
        {
            var inputMoveDir = new Vector3(0, 0, 0);
            if (Input.GetKey(KeyCode.W))
            {
                inputMoveDir.z = +1f;
            }
            if (Input.GetKey(KeyCode.S))
            {
                inputMoveDir.z = -1f;
            }
            if (Input.GetKey(KeyCode.A))
            {
                inputMoveDir.x = -1f;
            }
            if (Input.GetKey(KeyCode.D))
            {
                inputMoveDir.x = +1f;
            }

            var transform1 = transform;
            var moveVector = transform1.forward * inputMoveDir.z + transform1.right * inputMoveDir.x;
            transform1.position += moveVector * (moveSpeed * Time.deltaTime);
        }

        private void HandleRotation()
        {
            var rotationVector = new Vector3(0, 0, 0);

            if (Input.GetKey(KeyCode.Q))
            {
                rotationVector.y = +1f;
            }
            if (Input.GetKey(KeyCode.E))
            {
                rotationVector.y = -1f;
            }

            transform.eulerAngles += rotationVector * (rotationSpeed * Time.deltaTime);
        }

        private void HandleZoom()
        {
            if (Input.mouseScrollDelta.y > 0)
            {
                _targetFollowOffset.y -= zoomAmount;
            }
            if (Input.mouseScrollDelta.y < 0)
            {
                _targetFollowOffset.y += zoomAmount;
            }

            _targetFollowOffset.y = Mathf.Clamp(_targetFollowOffset.y, minFollowYOffset, maxFollowYOffset);

            _cinemachineTransposer.m_FollowOffset =
                Vector3.Lerp(_cinemachineTransposer.m_FollowOffset, _targetFollowOffset, Time.deltaTime * zoomSpeed);
        }

    }
}