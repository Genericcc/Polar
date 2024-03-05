using System;
using System.Collections.Generic;

using _Scripts._Game.DOTS.Components.Buffers;
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
        private List<PolarNode> _triedNodes;

        [Inject]
        public void Construct(
            SignalBus signalBus,
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

            _triedNodes = new List<PolarNode>();
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


            if (_triedNodes.Contains(node))
            {
                return;
            }
            
            _signalBus.Fire(new RequestStructurePlacementSignal(node));

            _triedNodes.Add(node);
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
}