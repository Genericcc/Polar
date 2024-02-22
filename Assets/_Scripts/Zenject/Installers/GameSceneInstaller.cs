using System.Collections.Generic;

using _Scripts.Data.Dictionaries;
using _Scripts.Grid;
using _Scripts.Managers;
using _Scripts.Structures;
using _Scripts.Structures.StructuresData;
using _Scripts.Zenject.Signals;

using Zenject;

namespace _Scripts.Zenject.Installers
{
    public class GameSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            Container.Bind<PolarGridManager>()
                     .FromComponentInHierarchy()
                     .AsSingle()
                     .NonLazy();

            Container.Bind<StructureManager>()
                     .FromComponentInHierarchy()
                     .AsSingle()
                     .NonLazy();


            Container.BindFactory<PolarGridPosition, Ring, PolarNode, PolarNodeFactory>()
                     .FromFactory<CustomPolarNodeFactory>();
            Container.Bind<PolarNode>()
                     .FromResource("Prefabs/Worlds/PolarGrids/PolarNodePrefab")
                     .WhenInjectedInto<CustomPolarNodeFactory>();

            Container.BindFactory<List<PolarNode>, StructureData, Building, BuildingFactory>()
                     .FromFactory<CustomBuildingFactory>();
            Container.Bind<Building>()
                     .FromResource("Prefabs/Worlds/Buildings/HouseBuildingPrefab")
                     .WhenInjectedInto<CustomBuildingFactory>();


            Container.Bind<PolarGirdRingsSettings>()
                     .FromNewScriptableObjectResource("Settings/PolarGridRingsSettings")
                     .AsSingle()
                     .NonLazy();

            Container.Bind<StructureDictionary>()
                     .FromNewScriptableObjectResource("Settings/StructureDictionary")
                     .AsSingle()
                     .NonLazy();

            RegisterAndBindSignals();
        }

        private void RegisterAndBindSignals()
        {
            Container.DeclareSignal<RequestBuildingPlacementSignal>().OptionalSubscriber();
            Container.BindSignal<RequestBuildingPlacementSignal>()
                     .ToMethod<StructureManager>(x => x.OnRequestBuildingPlacementSignal)
                     .FromResolveAll();
        }
    }
}