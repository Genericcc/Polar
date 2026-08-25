using _Scripts._Game.Managers;
using _Scripts._Game.Structures.StructuresData;
using _Scripts._Game.UIs.HUDs.Structures;

using UnityEngine;

using Zenject;

namespace _Scripts.Zenject.Installers
{
    public class UIInstaller : Installer<UIInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<StructureSelectionMenu>()
                     .FromComponentInHierarchy()
                     .AsSingle()
                     .NonLazy();

            Container.BindFactory<BaseStructureData, Transform, StructureSelectionButton, StructureButtonFactory>()
                     .FromFactory<CustomStructureButtonFactory>();
            Container.Bind<StructureSelectionButton>()
                     .FromResource("Prefabs/UIs/StructureButton")
                     .WhenInjectedInto<CustomStructureButtonFactory>();

            Container.DeclareSignal<StructureSelectedSignal>().OptionalSubscriber();
            Container.BindSignal<StructureSelectedSignal>()
                     .ToMethod<PlacementManager>(x => x.OnStructureSelectedSignal)
                     .FromResolveAll();
            
            // Container.DeclareSignal<ToggleStructureMenuSignal>().OptionalSubscriber();
            // Container.BindSignal<ToggleStructureMenuSignal>()
            //          .ToMethod<StructureSelectionMenu>(x => x.OnToggleStructureMenuSignal)
            //          .FromResolveAll();
        }
    }

    public class StructureSelectedSignal
    {
        public int StructureId;

        public StructureSelectedSignal(int structureId)
        {
            StructureId = structureId;
        }
    }

    public struct ToggleStructureMenuSignal { }
}