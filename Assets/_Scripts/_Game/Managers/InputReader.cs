using System;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

using Zenject;

using static PlayerInputActions;

namespace _Scripts._Game.Managers
{
    [CreateAssetMenu(menuName = "InputReader", fileName = "InputReader", order = 0)]
    public class InputReader : ScriptableObject, IPlayerActions
    {
        private PlayerInputActions _inputActions;
        public event UnityAction MouseClicked = delegate {};
        public event UnityAction CancelPressed = delegate {};

        public Vector3 CameraMoveDir => _inputActions.Player.MoveCamera.ReadValue<Vector2>();
        public Vector2 CameraRotationDir => _inputActions.Player.CameraRotation.ReadValue<Vector2>();
        public Vector2 CameraZoomDir => _inputActions.Player.ZoomCamera.ReadValue<Vector2>();
        public Vector2 PointerPosition => _inputActions.Player.MovePointer.ReadValue<Vector2>();
        public bool WasCancelPressed => _inputActions.Player.Cancel.ReadValue<float>() > 0.5f;
        public bool WasMouseClicked => _inputActions.Player.Fire.ReadValue<float>() > 0.5f;
        
        private void OnEnable()
        {
            if (_inputActions != null)
            {
                return;
            }

            _inputActions = new PlayerInputActions();
            _inputActions.Player.SetCallbacks(this);
        }

        public void EnablePlayerActions()
        {
            _inputActions.Enable();
        }

        public void DisablePlayerActions()
        {
            _inputActions.Disable();
        }

        public void OnFire(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Started)
            {
                return;
            }
            
            MouseClicked.Invoke();
        }

        public void OnMoveCamera(InputAction.CallbackContext context)
        {
            //MoveCamera.Invoke(context.ReadValue<Vector2>());
        }

        public void OnCameraRotation(InputAction.CallbackContext context)
        {
            
        }

        public void OnOpenStructures(InputAction.CallbackContext context)
        {
            throw new NotImplementedException();
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Started)
            {
                return;
            }
            
            CancelPressed.Invoke();
        }

        public void OnZoomCamera(InputAction.CallbackContext context)
        {
            //ZoomCamera.Invoke(context.ReadValue<Vector2>());
        }
        
        public void OnEnableCameraRotation(InputAction.CallbackContext context)
        {
        }

        public void OnMovePointer(InputAction.CallbackContext context)
        {
            
        }
    }
}