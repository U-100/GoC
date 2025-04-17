using System;
using System.Collections.Generic;
using UnityEngine;

namespace GoC.Arena
{
    [Serializable]
    public struct MovementTier
    {
        [SerializeField] private int[] _productionCost;
        public IReadOnlyList<int> productionCost => _productionCost;
        
        [SerializeField] private bool _canMove;
        public bool canMove => _canMove;
    }
    
    [Serializable]
    public struct HullTier
    {
        [SerializeField] private int[] _productionCost;
        public IReadOnlyList<int> productionCost => _productionCost;
        
        [SerializeField] private int _maxIntegrity;
        public int maxIntegrity => _maxIntegrity;
    }
    
    [Serializable]
    public struct StorageTier
    {
        [SerializeField] private int[] _productionCost;
        public IReadOnlyList<int> productionCost => _productionCost;
        
        [SerializeField] private int _storageCapacityPerSubstance;
        public int storageCapacityPerSubstance => _storageCapacityPerSubstance;
    }
    
    [CreateAssetMenu]
    public class ProductionConfig : ScriptableObject
    {
        [SerializeField] private MovementTier[] _movementTiers;
        public IReadOnlyList<MovementTier> movementTiers => _movementTiers;
        
        [SerializeField] private HullTier[] _hullTiers;
        public IReadOnlyList<HullTier> hullTiers => _hullTiers;
        
        [SerializeField] private StorageTier[] _storageTiers;
        public IReadOnlyList<StorageTier> storageTiers => _storageTiers;
    }
}
