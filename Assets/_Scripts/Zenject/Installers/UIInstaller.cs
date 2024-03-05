using _Scripts._Game.Managers;
using _Scripts._Game.Structures.StructuresData;
using _Scripts._Game.UIs.HUDs.Structures;

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
        public IStructureData StructureData;

        public StructureSelectedSignal(IStructureData structureData)
        {
            StructureData = structureData;
        }
    }

    public struct ToggleStructureMenuSignal { }
}