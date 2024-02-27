using _Scripts._Game.Managers;
using _Scripts._Game.Structures.StructuresData;

using Zenject;

namespace _Scripts.Zenject.Installers
{
    public class UIInstaller : Installer<UIInstaller>
    {
        public override void InstallBindings()
        {

            Container.DeclareSignal<SelectStructureSignal>().OptionalSubscriber();
            Container.BindSignal<SelectStructureSignal>()
                     .ToMethod<StructureManager>(x => x.OnSelectStructureToBuild)
                     .FromResolveAll();
        }
    }

    public class SelectStructureSignal
    {
        public IStructureData StructureData;

        public SelectStructureSignal(IStructureData structureData)
        {
            StructureData = structureData;
        }
    }
}