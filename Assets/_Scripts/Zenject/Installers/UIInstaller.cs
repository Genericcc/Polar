using _Scripts._Game.Managers;
using _Scripts._Game.Structures.StructuresData;

using Zenject;

namespace _Scripts.Zenject.Installers
{
    public class UIInstaller : Installer<UIInstaller>
    {
        public override void InstallBindings()
        {

            Container.DeclareSignal<StructureSelectedSignal>().OptionalSubscriber();
            Container.BindSignal<StructureSelectedSignal>()
                     .ToMethod<PlacementManager>(x => x.OnStructureSelectedSignal)
                     .FromResolveAll();
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
}