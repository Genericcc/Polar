using System;
using System.Collections.Generic;

using _Scripts._Game.Grid;
using _Scripts._Game.Structures.StructuresData;

using Zenject;

namespace _Scripts._Game.Structures
{
    public class StructureFactory : PlaceholderFactory<List<PolarNode>, BaseStructureData, Structure>
    {
    }

    public class CustomStructureFactory : IFactory<List<PolarNode>, BaseStructureData, Structure>
    {
        private readonly DiContainer _container;
        private readonly HouseStructure _housePrefab;
        private readonly WallStructure _wallPrefab;

        public CustomStructureFactory(DiContainer container,
            HouseStructure housePrefab,
            WallStructure wallPrefab)
        {
            _container = container;
            _housePrefab = housePrefab;
            _wallPrefab = wallPrefab;
        }

        public Structure Create(List<PolarNode> polarNodes, BaseStructureData baseStructureData)
        {
            var structure = baseStructureData.structureType switch
            {
                StructureType.House => _container.InstantiatePrefabForComponent<Structure>(_housePrefab),
                StructureType.Wall => _container.InstantiatePrefabForComponent<Structure>(_wallPrefab),
                _ => throw new ArgumentOutOfRangeException()
            };

            structure.Initialise(polarNodes, baseStructureData);
            
            return structure;
        }
    }
}