using System.Collections.Generic;
using UnityEngine;

namespace GoC.Arena
{
    public interface IFactoryConfig
    {
        int integrity { get; }
        int storageCapacityPerSubstance { get; }
        IReadOnlyList<int> productionCost { get; }
        int productionTime { get; }
        UnitConfig product { get; }
    }

    [CreateAssetMenu]
    public class FactoryConfig : ScriptableObject, IFactoryConfig
    {
        [SerializeField] private int _integrity;
        public int integrity => _integrity;
        
        [SerializeField] private int _storageCapacityPerSubstance;
        public int storageCapacityPerSubstance => _storageCapacityPerSubstance;

        [SerializeField] private int[] _productionCost;
        public IReadOnlyList<int> productionCost => _productionCost;

        [SerializeField] private int _productionTime;
        public int productionTime => _productionTime;
        
        [SerializeField] private UnitConfig _product;
        public UnitConfig product => _product;
    }
}
