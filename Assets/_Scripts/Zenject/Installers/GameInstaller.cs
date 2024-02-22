using System.Collections.Generic;

using _Scripts._Game.Grid;
using _Scripts._Game.Managers;
using _Scripts._Game.Structures;
using _Scripts._Game.Structures.StructuresData;
using _Scripts.Data.Dictionaries;
using _Scripts.Zenject.Signals;

using Zenject;

namespace _Scripts.Zenject.Installers
{
    public class GameInstaller : Installer<GameInstaller>
    {
        public override void InstallBindings()
        {

            #region PolarGrid

            Container.Bind<PolarGridManager>()
                     .FromComponentInHierarchy()
                     .AsSingle()
                     .NonLazy();
            
            Container.BindFactory<PolarGridPosition, Ring, PolarNode, PolarNodeFactory>()
                     .FromFactory<CustomPolarNodeFactory>();
            Container.Bind<PolarNode>()
                     .FromResource("Prefabs/Worlds/PolarGrids/PolarNodePrefab")
                     .WhenInjectedInto<CustomPolarNodeFactory>();
            
            Container.Bind<PolarGridRingsSettings>()
                     .FromNewScriptableObjectResource("Settings/PolarGridRingsSettings")
                     .AsSingle()
                     .NonLazy();
            

            #endregion

            #region Structures

            Container.Bind<StructureManager>()
                     .FromComponentInHierarchy()
                     .AsSingle()
                     .NonLazy();
            
            Container.BindFactory<List<PolarNode>, StructureData, Structure, StructureFactory>()
                     .FromFactory<CustomStructureFactory>();
            Container.Bind<Structure>()
                     .FromResource("Prefabs/Worlds/Structures/HouseBuildingPrefab")
                     .WhenInjectedInto<CustomStructureFactory>();

            Container.Bind<StructureDictionary>()
                     .FromNewScriptableObjectResource("Dictionaries/StructureDictionary")
                     .AsSingle()
                     .NonLazy();

            #endregion

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