using _Scripts.Grid;
using _Scripts.Managers;
using _Scripts.Zenject.Signals;

using UnityEngine;

using Zenject;

namespace _Scripts.Zenject.Installers
{
    public class GameSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            Container.Bind<BuildingsManager>()
                     .FromNewComponentOnNewGameObject()
                     .AsSingle()
                     .NonLazy();
            
            Container.Bind<PolarNode>()
                     .FromResource("Prefabs/World/PolarNodePrefab")
                     .WhenInjectedInto<CustomPolarNodeFactory>();
            
            Container.BindFactory<PolarGridSystem, PolarGridPosition, PolarNode, PolarNodeFactory>()
                     .FromFactory<CustomPolarNodeFactory>();
            
            
            Container.Bind<PolarGirdRingsSettings>()
                     .FromNewScriptableObjectResource("Settings/PolarGridRingsSettings")
                     .AsSingle()
                     .NonLazy();
            
            RegisterAndBindSignals();
        }

        private void RegisterAndBindSignals()
        {
            Container.DeclareSignal<BuildNewBuildingSignal>().OptionalSubscriber();
            Container.BindSignal<BuildNewBuildingSignal>()
                     .ToMethod<BuildingsManager>(x => x.OnBuildingBuiltSignal)
                     .FromResolveAll();
        }
    }
}