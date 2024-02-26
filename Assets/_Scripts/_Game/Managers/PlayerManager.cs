using Unity.Entities;
using Unity.Physics;

using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts._Game.Managers
{
    public class PlayerManager : MonoBehaviour
    {
        public InputAction input;
        public Camera mainCamera;
        public int buildingIndex;

        private Entity _entity;
        private World _world;

        private void OnEnable()
        {
            input.started += MouseClicked;
            input.Enable();

            mainCamera = mainCamera == null ? Camera.main : mainCamera;

            _world = World.DefaultGameObjectInjectionWorld;
        }

        private void MouseClicked(InputAction.CallbackContext ctx)
        {
            var screenPostion = ctx.ReadValue<Vector2>();
            var ray = mainCamera.ScreenPointToRay(screenPostion);

            Debug.Log(ray.GetPoint(mainCamera.farClipPlane));

            if (_world.IsCreated && !_world.EntityManager.Exists(_entity))
            {
                _entity = _world.EntityManager.CreateEntity();
                _world.EntityManager.AddBuffer<BuildingPlacementInput>(_entity);
            }

            var input = new RaycastInput
            {
                Start = ray.origin, Filter = CollisionFilter.Default, End = ray.GetPoint(mainCamera.farClipPlane)
            };

            _world.EntityManager.GetBuffer<BuildingPlacementInput>(_entity)
                 .Add(new BuildingPlacementInput { Value = input, index = buildingIndex });
        }

        private void OnDisable()
        {
            input.started -= MouseClicked;
            input.Disable();

            if (_world.IsCreated && _world.EntityManager.Exists(_entity))
            {
                _world.EntityManager.DestroyEntity(_entity);
            }
        }
    }

    public struct BuildingPlacementInput : IBufferElementData
    {
        public RaycastInput Value;
        internal int index;
    }
}