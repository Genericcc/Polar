using System;

using _Scripts.Zenject.Installers;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

using Zenject;

using static PlayerInputActions;

namespace _Scripts._Game.Managers
{

    public class InputReader : MonoBehaviour, IPlayerActions
    {
        private PlayerInputActions _inputActions;
        private SignalBus _signalBus;

        public event UnityAction MouseClicked = delegate
        {
        };

        public event UnityAction StructuresMenu = delegate
        {
        };

        public Vector3 CameraMoveDir => _inputActions.Player.MoveCamera.ReadValue<Vector2>();
        public Vector2 CameraRotationDir => _inputActions.Player.CameraRotation.ReadValue<Vector2>();
        public Vector2 CameraZoomDir => _inputActions.Player.ZoomCamera.ReadValue<Vector2>();
        public Vector2 PointerPosition => _inputActions.Player.MovePointer.ReadValue<Vector2>();
        public bool WasCancelPressed => _inputActions.Player.Cancel.ReadValue<float>() > 0.5f;
        public bool WasMouseClicked => _inputActions.Player.Fire.ReadValue<float>() > 0.5f;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void EnablePlayerActions()
        {
            if (_inputActions == null)
            {
                _inputActions = new PlayerInputActions();
                _inputActions.Player.SetCallbacks(this);
            }

            _inputActions.Enable();
        }

        public void DisablePlayerActions()
        {
            _inputActions.Disable();
        }

        public void OnFire(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                MouseClicked.Invoke();
            }
        }

        public void OnMoveCamera(InputAction.CallbackContext context)
        {
        }

        public void OnCameraRotation(InputAction.CallbackContext context)
        {
        }

        public void OnOpenStructures(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                StructuresMenu.Invoke();
                
                //SignalBus accesses disabled GameObjects, so could be more useful for a close/open case
                //but I don't want to use it for everything in this project
                //_signalBus.Fire<ToggleStructureMenuSignal>();
            }
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
        }

        public void OnZoomCamera(InputAction.CallbackContext context)
        {
        }


        public void OnMovePointer(InputAction.CallbackContext context)
        {

        }
    }
}
// }using System;
//
// using _Scripts.Zenject.Installers;
//
// using UnityEngine;
// using UnityEngine.Events;
// using UnityEngine.InputSystem;
//
// using Zenject;
//
// using static PlayerInputActions;
//
// namespace _Scripts._Game.Managers
// {
//     
//     [CreateAssetMenu(menuName = "InputReader", fileName = "Settings/InputReader", order = 0)]
//     public class InputReader : ScriptableObject, IPlayerActions
//     {
//         private PlayerInputActions _inputActions;
//         private SignalBus _signalBus;
//         public event UnityAction MouseClicked = delegate {};
//         public event UnityAction StructuresMenu = delegate {};
//
//         public Vector3 CameraMoveDir => _inputActions.Player.MoveCamera.ReadValue<Vector2>();
//         public Vector2 CameraRotationDir => _inputActions.Player.CameraRotation.ReadValue<Vector2>();
//         public Vector2 CameraZoomDir => _inputActions.Player.ZoomCamera.ReadValue<Vector2>();
//         public Vector2 PointerPosition => _inputActions.Player.MovePointer.ReadValue<Vector2>();
//         public bool WasCancelPressed => _inputActions.Player.Cancel.ReadValue<float>() > 0.5f;
//         public bool WasMouseClicked => _inputActions.Player.Fire.ReadValue<float>() > 0.5f;
//
//         // [Inject]
//         // public void Construct(SignalBus signalBus)
//         // {
//         //     _signalBus = signalBus;
//         // }
//         
//         public void EnablePlayerActions()
//         {
//             if (_inputActions == null)
//             {
//                 _inputActions = new PlayerInputActions();
//                 _inputActions.Player.SetCallbacks(this);
//             }
//             
//             _inputActions.Enable();
//         }
//
//         public void DisablePlayerActions()
//         {
//             _inputActions.Disable();
//         }
//
//         public void OnFire(InputAction.CallbackContext context)
//         {
//             if (context.performed)
//             {
//                 MouseClicked.Invoke();
//             }
//         }
//
//         public void OnMoveCamera(InputAction.CallbackContext context)
//         {
//         }
//
//         public void OnCameraRotation(InputAction.CallbackContext context)
//         {
//         }
//
//         public void OnOpenStructures(InputAction.CallbackContext context)
//         {
//             if (!context.performed)
//             {
//                 StructuresMenu.Invoke();
//             }
//         }
//
//         public void OnCancel(InputAction.CallbackContext context)
//         {
//         }
//
//         public void OnZoomCamera(InputAction.CallbackContext context)
//         {
//         }
//         
//
//         public void OnMovePointer(InputAction.CallbackContext context)
//         {
//             
//         }
//     }
// }