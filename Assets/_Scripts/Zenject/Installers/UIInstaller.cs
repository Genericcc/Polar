using _Scripts._Game.Managers;
using _Scripts._Game.UIs;
using _Scripts._Game.UIs.Controllers;

using Zenject;

namespace _Scripts.Zenject.Installers
{
    public class UIInstaller : Installer<UIInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<UIRoot>()
                     .FromComponentInHierarchy()
                     .AsSingle()
                     .NonLazy();

            Container.Bind<GameStatsProvider>()
                     .FromComponentInHierarchy()
                     .AsSingle()
                     .NonLazy();

            Container.Bind<StructureButtonFactory>()
                     .AsSingle();

            Container.BindInterfacesAndSelfTo<BuildBarController>().AsSingle();
            Container.BindInterfacesAndSelfTo<StructureInfoController>().AsSingle();
            Container.BindInterfacesAndSelfTo<ResourceBarController>().AsSingle();
            Container.BindInterfacesAndSelfTo<PopulationController>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlacementStatusController>().AsSingle();

            Container.DeclareSignal<StructureSelectedSignal>().OptionalSubscriber();
            Container.BindSignal<StructureSelectedSignal>()
                     .ToMethod<PlacementManager>(x => x.OnStructureSelectedSignal)
                     .FromResolveAll();
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
}
