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
            
            Container.BindFactory<int, Ring, RingFactory>()
                     .FromFactory<CustomRingFactory>();
            Container.Bind<Ring>()
                     .FromResource("Prefabs/Worlds/PolarGrids/RingPlanePrefab")
                     .WhenInjectedInto<CustomRingFactory>();
            
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
            
            // Container.BindFactory<List<PolarNode>, IStructureData, Structure, StructureFactory>()
            //          .FromFactory<CustomStructureFactory>();
            // Container.Bind<HouseStructure>()
            //          .FromResource("Prefabs/Worlds/Structures/HouseStructurePrefab")
            //          .WhenInjectedInto<CustomStructureFactory>();
            // Container.Bind<WallStructure>()
            //          .FromResource("Prefabs/Worlds/Structures/WallStructurePrefab")
            //          .WhenInjectedInto<CustomStructureFactory>();

            Container.Bind<StructureDictionary>()
                     .FromNewScriptableObjectResource("Dictionaries/StructureDictionary")
                     .AsSingle()
                     .NonLazy();

            #endregion

            #region Player

            Container.Bind<MouseWorld>()
                     .FromComponentInHierarchy()
                     .AsSingle()
                     .NonLazy();

            Container.Bind<InputReader>()
                     .FromResource("Settings/InputReader")
                     .AsSingle()
                     .NonLazy();

            #endregion

            RegisterAndBindSignals();
        }

        private void RegisterAndBindSignals()
        {
            Container.DeclareSignal<RequestStructurePlacementSignal>().OptionalSubscriber();
            Container.BindSignal<RequestStructurePlacementSignal>()
                     .ToMethod<StructureManager>(x => x.OnRequestBuildingPlacementSignal)
                     .FromResolveAll();
        }
    }
}