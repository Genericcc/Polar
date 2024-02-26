using System;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

using static PlayerInputActions;

namespace _Scripts._Game.Managers
{
    [CreateAssetMenu(menuName = "InputReader", fileName = "InputReader", order = 0)]
    public class InputReader : ScriptableObject, IPlayerActions
    {
        private PlayerInputActions _inputActions;

        public Vector3 CameraMoveDir => _inputActions.Player.MoveCamera.ReadValue<Vector2>();
        public Vector2 CameraRotationDir => _inputActions.Player.CameraRotation.ReadValue<Vector2>();
        public Vector2 CameraZoomDir => _inputActions.Player.ZoomCamera.ReadValue<Vector2>();

        private void OnEnable()
        {
            if (_inputActions == null)
            {
                _inputActions = new PlayerInputActions();
                _inputActions.Player.SetCallbacks(this);
            }
            
            _inputActions.Enable();
        }
        
        public void OnMoveCamera(InputAction.CallbackContext context)
        {
            
        }

        public void OnCameraRotation(InputAction.CallbackContext context)
        {
            
        }

        public void OnZoomCamera(InputAction.CallbackContext context)
        {
            
        }
        
        public void OnEnableCameraRotation(InputAction.CallbackContext context)
        {
            
        }

        public void OnMovePointer(InputAction.CallbackContext context)
        {
            
        }

        public void OnFire(InputAction.CallbackContext context)
        {
            
        }
    }
}