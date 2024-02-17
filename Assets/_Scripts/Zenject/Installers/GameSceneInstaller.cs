using System.Collections.Generic;

using _Scripts.Buildings;
using _Scripts.Buildings.BuildingsData;
using _Scripts.Grid;
using _Scripts.Managers;
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

            Container.Bind<BuildingsManager>()
                     .FromNewComponentOnNewGameObject()
                     .AsSingle()
                     .NonLazy();
            
            
            Container.BindFactory<PolarGridSystem, PolarGridPosition, RingSettings, PolarNode, PolarNodeFactory>()
                     .FromFactory<CustomPolarNodeFactory>();
            Container.Bind<PolarNode>()
                     .FromResource("Prefabs/World/PolarNodePrefab")
                     .WhenInjectedInto<CustomPolarNodeFactory>();
            
            Container.BindFactory<List<PolarNode>, BuildingData, Building, BuildingFactory>()
                     .FromFactory<CustomBuildingFactory>();
            Container.Bind<Building>()
                     .FromResource("Prefabs/World/HouseBuildingPrefab")
                     .WhenInjectedInto<CustomBuildingFactory>();
            
            
            Container.Bind<PolarGirdRingsSettings>()
                     .FromNewScriptableObjectResource("Settings/PolarGridRingsSettings")
                     .AsSingle()
                     .NonLazy();
            
            RegisterAndBindSignals();
        }

        private void RegisterAndBindSignals()
        {
            Container.DeclareSignal<RequestBuildingPlacementSignal>().OptionalSubscriber();
            Container.BindSignal<RequestBuildingPlacementSignal>()
                     .ToMethod<BuildingsManager>(x => x.OnRequestBuildingPlacementSignal)
                     .FromResolveAll();
        }
    }
}