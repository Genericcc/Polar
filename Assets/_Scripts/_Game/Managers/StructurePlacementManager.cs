using System;

using _Scripts._Game.Grid;
using _Scripts.Zenject.Signals;

using Unity.Entities;
using Unity.Physics;

using UnityEngine;
using UnityEngine.InputSystem;

using Zenject;

namespace _Scripts._Game.Managers
{
    public class StructurePlacementManager : MonoBehaviour
    {
        [SerializeField]
        private Camera mainCamera;
        
        private InputReader _input;
        private PolarGridManager _polarGridManager;
        private SignalBus _signalBus;
        
        public int buildingIndex;

        private Entity _entity;
        private World _world;
        private MouseWorld _mouseWorld;

        [Inject]
        public void Construct(SignalBus signalBus, 
            InputReader inputReader, 
            PolarGridManager polarGridManager,
            MouseWorld mouseWorld)
        {
            _signalBus = signalBus;
            _input = inputReader;
            _polarGridManager = polarGridManager;
            _mouseWorld = mouseWorld;
        }

        private void Start()
        {
            if (_input == null)
            {
                _input = FindObjectOfType<InputReader>();
            }
        }

        private void OnEnable()
        {
            _input.MouseClicked += OnMouseClicked;
            _input.EnablePlayerActions();

            mainCamera = mainCamera == null ? Camera.main : mainCamera;

            _world = World.DefaultGameObjectInjectionWorld;
        }

        private void OnMouseClicked()
        {
            var screenPosition = _input.PointerPosition;
            var ray = mainCamera.ScreenPointToRay(screenPosition);

            var node = _mouseWorld.CurrentNode;
            if (node == null)
            {
                return;
            }

            _signalBus.Fire(new RequestStructurePlacementSignal(node));

            //------ENTITIES-------
            // var raycastInput = new RaycastInput
            // {
            //     Start = ray.origin, 
            //     Filter = CollisionFilter.Default, 
            //     End = ray.GetPoint(mainCamera.farClipPlane)
            // };
            
            if (_world.IsCreated && !_world.EntityManager.Exists(_entity))
            {
                _entity = _world.EntityManager.CreateEntity();
                _world.EntityManager.AddBuffer<StructurePlacementInput>(_entity);
            }
            
            _world.EntityManager.GetBuffer<StructurePlacementInput>(_entity)
                  .Add(new StructurePlacementInput 
                    { 
                        //RaycastInput = raycastInput, 
                        //PolarGridPosition = polar, 
                        StructureIndex = buildingIndex 
                    });
        }

        private void OnDisable()
        {
            _input.MouseClicked -= OnMouseClicked;
            _input.DisablePlayerActions();

            if (_world.IsCreated && _world.EntityManager.Exists(_entity))
            {
                _world.EntityManager.DestroyEntity(_entity);
            }
        }
    }

    public struct StructurePlacementInput : IBufferElementData
    {
        
        public PolarGridPosition PolarGridPosition;
        internal int StructureIndex;
    }
}