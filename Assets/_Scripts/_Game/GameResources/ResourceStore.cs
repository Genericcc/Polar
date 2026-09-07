using System;
using System.Collections.Generic;

namespace _Scripts._Game.GameResources
{
    public class ResourceStore
    {
        private readonly Dictionary<ResourceType, int> _amounts = new();

        public event Action Changed;

        public ResourceStore(ResourceSettings settings)
        {
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                _amounts[type] = 0;
            }

            if (settings != null && settings.startingAmounts != null)
            {
                foreach (var starting in settings.startingAmounts)
                {
                    _amounts[starting.ResourceType] = starting.Amount;
                }
            }
        }

        public int Get(ResourceType type)
        {
            return _amounts.GetValueOrDefault(type, 0);
        }

        public bool CanAfford(IReadOnlyList<ResourceAmount> cost)
        {
            if (cost == null)
            {
                return true;
            }

            foreach (var entry in cost)
            {
                if (Get(entry.ResourceType) < entry.Amount)
                {
                    return false;
                }
            }

            return true;
        }

        public bool TrySpend(IReadOnlyList<ResourceAmount> cost)
        {
            if (!CanAfford(cost))
            {
                return false;
            }

            if (cost == null)
            {
                return true;
            }

            foreach (var entry in cost)
            {
                _amounts[entry.ResourceType] -= entry.Amount;
            }

            Changed?.Invoke();
            return true;
        }

        public void Add(ResourceType type, int amount)
        {
            _amounts[type] = Get(type) + amount;
            Changed?.Invoke();
        }
    }
}
