using _Scripts.Data.Dictionaries;

using Unity.Entities;

using UnityEngine;

namespace _Scripts._Game.DOTS.Authoring.Structures
{
    public class StructureRegister : MonoBehaviour
    {
        [SerializeField]
        private StructureDictionary dictionary;

        class Baker : Baker<StructureRegister>
        {
            public override void Bake(StructureRegister authoring)
            {
                var registerEntity = GetEntity(TransformUsageFlags.Dynamic);
                var structureBuffer = AddBuffer<AvailableStructure>(registerEntity);

                if (authoring.dictionary == null)
                {
                    return;
                }

                // Baking only tracks the authoring component's own fields, so every read that
                // reaches through a reference has to be declared - otherwise the buffer goes stale
                DependsOn(authoring.dictionary);

                foreach (var structure in authoring.dictionary.structures)
                {
                    if (structure == null)
                    {
                        continue;
                    }

                    DependsOn(structure);

                    if (structure.Prefab == null)
                    {
                        Debug.LogWarning($"{structure.name} has no prefab assigned, skipping it in the register");
                        continue;
                    }

                    structureBuffer.Add(new AvailableStructure
                    {
                        Prefab = GetEntity(structure.Prefab, TransformUsageFlags.Dynamic),
                        StructureId = structure.ID,
                    });
                }
            }
        }
    }
}
