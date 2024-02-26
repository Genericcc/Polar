using System;

using _Scripts._Game.Managers;

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


        [SerializeField]
        private InputReader input;

        private void OnEnable()
        {
            //inputReader.MoveCamera += MoveCamera();
        }

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
            var cameraTransform = transform;
            var moveDir = cameraTransform.forward * input.CameraMoveDir.y + cameraTransform.right * input.CameraMoveDir.x;
            cameraTransform.position += moveDir * (moveSpeed * Time.deltaTime);
        }

        private void HandleRotation()
        {
            var rotationVector = new Vector3(0, -input.CameraRotationDir.x, 0);
            transform.eulerAngles += rotationVector * (rotationSpeed * Time.deltaTime);
        }

        private void HandleZoom()
        {
            if (input.CameraZoomDir.y > 0)
            {
                _targetFollowOffset.y -= zoomAmount;
            }
            if (input.CameraZoomDir.y < 0)
            {
                _targetFollowOffset.y += zoomAmount;
            }

            _targetFollowOffset.y = Mathf.Clamp(_targetFollowOffset.y, minFollowYOffset, maxFollowYOffset);

            _cinemachineTransposer.m_FollowOffset =
                Vector3.Lerp(_cinemachineTransposer.m_FollowOffset, _targetFollowOffset, Time.deltaTime * zoomSpeed);
        }

    }
}