using Unity.Entities;
using UnityEngine;

namespace _Scripts._Game.DOTS.Authoring.Times
{
    public class GameTimeAuthoring : MonoBehaviour
    {
        [SerializeField]
        private float InGameHourDuration;
        
        private class GameTimeBaker : Baker<GameTimeAuthoring>
        {
            public override void Bake(GameTimeAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new GameTimeData { InGameHourDuration = authoring.InGameHourDuration });
            }
        }
    }

    public struct GameTimeData : IComponentData
    {
        public float InGameHourDuration;
    }
}