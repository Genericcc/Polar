using _Scripts._Game.DOTS.Authoring.Times;
using Unity.Entities;

namespace _Scripts._Game.DOTS.Systems.Times
{
    public partial class GameTimeManager : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override void OnUpdate()
        {
            var gameTime = SystemAPI.GetSingleton<GameTimeData>();
            
        }
    }
}