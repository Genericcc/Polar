using System.Collections.Generic;

using UnityEngine;

namespace _Scripts._Game.GameResources
{
    [CreateAssetMenu(
        menuName = PolarAssetMenu.Root + "Data/" + nameof(ResourceSettings),
        fileName = nameof(ResourceSettings),
        order = PolarAssetMenu.Order)]
    public class ResourceSettings : ScriptableObject
    {
        [SerializeField]
        public List<ResourceAmount> startingAmounts; 
    }
}
