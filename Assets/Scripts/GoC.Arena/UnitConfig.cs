using System.Collections.Generic;
using UnityEngine;

namespace GoC.Arena
{
    public interface IUnitConfig
    {
        int integrity { get; }
        int damage { get; }
        int storageCapacityPerSubstance { get; }
    }
    
    [CreateAssetMenu]
    public class UnitConfig : ScriptableObject
    {
        [SerializeField] private int _integrity;
        public int integrity => _integrity;

        [SerializeField] private int _damage;
        public int damage => _damage;

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
