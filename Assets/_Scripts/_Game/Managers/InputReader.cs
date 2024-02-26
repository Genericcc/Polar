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
        public event UnityAction<Vector2> MoveCamera = delegate { };
        public event UnityAction<Vector2> RotateCamera = delegate { };
        public event UnityAction<Vector2> ZoomCamera = delegate { };

        public PlayerInputActions inputActions;

        public Vector3 Direction => inputActions.Player.MoveCamera.ReadValue<Vector2>();
        public Vector2 RotationDirection => inputActions.Player.CameraRotation.ReadValue<Vector2>();
        public Vector2 ZoomDir => inputActions.Player.ZoomCamera.ReadValue<Vector2>();

        private void OnEnable()
        {
            if (inputActions == null)
            {
                inputActions = new PlayerInputActions();
                inputActions.Player.SetCallbacks(this);
            }
            
            inputActions.Enable();
        }
        
        public void OnMoveCamera(InputAction.CallbackContext context)
        {
            MoveCamera.Invoke(context.ReadValue<Vector2>());
        }

        public void OnCameraRotation(InputAction.CallbackContext context)
        {
            RotateCamera.Invoke(context.ReadValue<Vector2>());
        }

        public void OnZoomCamera(InputAction.CallbackContext context)
        {
            ZoomCamera.Invoke(context.ReadValue<Vector2>());
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