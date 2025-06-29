﻿using Zenject;

namespace _Scripts.Zenject.Installers
{
    public class GameSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            //Defaultowy installer od Busa 
            SignalBusInstaller.Install(Container);
            
            GameInstaller.Install(Container);
            UIInstaller.Install(Container);
        }
    }
}