using System;
using System.Collections.Generic;
using UnityEngine;

using static Unity.Mathematics.math;

namespace GoC.Arena
{
    [Serializable]
    public struct AttackTier
    {
        [SerializeField] private int[] _productionCost;
        public IReadOnlyList<int> productionCost => _productionCost;

        [SerializeField] private int _productionTicks;
        public int productionTicks => _productionTicks;
        
        [SerializeField] private int _damage;
        public int damage => _damage;
    }
    
    [Serializable]
    public struct MiningTier
    {
        [SerializeField] private int[] _productionCost;
        public IReadOnlyList<int> productionCost => _productionCost;

        [SerializeField] private int _productionTicks;
        public int productionTicks => _productionTicks;
        
        [SerializeField] private bool _canMine;
        public bool canMine => _canMine;
    }
    
    [Serializable]
    public struct ProductionTier
    {
        [SerializeField] private int[] _productionCost;
        public IReadOnlyList<int> productionCost => _productionCost;

        [SerializeField] private int _productionTicks;
        public int productionTicks => _productionTicks;
        
        [Tooltip("highest tier of unit params that can be produced, -1 means no production is possible")]
        [SerializeField] private int _maxProductionTier;
        public int maxProductionTier => _maxProductionTier;
    }
    
    [Serializable]
    public struct MovementTier
    {
        [SerializeField] private int[] _productionCost;
        public IReadOnlyList<int> productionCost => _productionCost;

        [SerializeField] private int _productionTicks;
        public int productionTicks => _productionTicks;
        
        [SerializeField] private bool _canMove;
        public bool canMove => _canMove;
    }
    
    [Serializable]
    public struct HullTier
    {
        [SerializeField] private int[] _productionCost;
        public IReadOnlyList<int> productionCost => _productionCost;

        [SerializeField] private int _productionTicks;
        public int productionTicks => _productionTicks;
        
        [SerializeField] private int _maxIntegrity;
        public int maxIntegrity => _maxIntegrity;
    }
    
    [Serializable]
    public struct StorageTier
    {
        [SerializeField] private int[] _productionCost;
        public IReadOnlyList<int> productionCost => _productionCost;

        [SerializeField] private int _productionTicks;
        public int productionTicks => _productionTicks;
        
        [SerializeField] private int _storageCapacityPerSubstance;
        public int storageCapacityPerSubstance => _storageCapacityPerSubstance;
    }
    
    [CreateAssetMenu]
    public class UnitConfig : ScriptableObject
    {
        [SerializeField] private AttackTier[] _attackTiers;
        public IReadOnlyList<AttackTier> attackTiers => _attackTiers;
        
        [SerializeField] private MiningTier[] _miningTiers;
        public IReadOnlyList<MiningTier> miningTiers => _miningTiers;
        
        [SerializeField] private ProductionTier[] _productionTiers;
        public IReadOnlyList<ProductionTier> productionTiers => _productionTiers;
        
        [SerializeField] private MovementTier[] _movementTiers;
        public IReadOnlyList<MovementTier> movementTiers => _movementTiers;
        
        [SerializeField] private HullTier[] _hullTiers;
        public IReadOnlyList<HullTier> hullTiers => _hullTiers;
        
        [SerializeField] private StorageTier[] _storageTiers;
        public IReadOnlyList<StorageTier> storageTiers => _storageTiers;

        public UnitParams ClampUnitParams(UnitParams unitParams) =>
            new()
            {
                attackTier = clamp(unitParams.attackTier, 0, attackTiers.Count - 1),
                miningTier = clamp(unitParams.miningTier, 0, miningTiers.Count - 1),
                productionTier = clamp(unitParams.productionTier, 0, productionTiers.Count - 1),
                movementTier = clamp(unitParams.movementTier, 0, movementTiers.Count - 1),
                hullTier = clamp(unitParams.hullTier, 0, hullTiers.Count - 1),
                storageTier = clamp(unitParams.storageTier, 0, storageTiers.Count - 1),
            };

        public int ProductionCost(UnitParams unitParams, int substanceIndex)
        {
            unitParams = ClampUnitParams(unitParams);
            
            int cost = 0;

            if (attackTiers[unitParams.attackTier].productionCost.Count > substanceIndex)
                cost += attackTiers[unitParams.attackTier].productionCost[substanceIndex];
                
            if (miningTiers[unitParams.miningTier].productionCost.Count > substanceIndex)
                cost += miningTiers[unitParams.miningTier].productionCost[substanceIndex];
                
            if (productionTiers[unitParams.productionTier].productionCost.Count > substanceIndex)
                cost += productionTiers[unitParams.productionTier].productionCost[substanceIndex];
                
            if (movementTiers[unitParams.movementTier].productionCost.Count > substanceIndex)
                cost += movementTiers[unitParams.movementTier].productionCost[substanceIndex];
                
            if (hullTiers[unitParams.hullTier].productionCost.Count > substanceIndex)
                cost += hullTiers[unitParams.hullTier].productionCost[substanceIndex];
                
            if (storageTiers[unitParams.storageTier].productionCost.Count > substanceIndex)
                cost += storageTiers[unitParams.storageTier].productionCost[substanceIndex];

            return cost;
        }

        public int ProductionTicks(UnitParams unitParams)
        {
            unitParams = ClampUnitParams(unitParams);

            int productionTicks =
                attackTiers[unitParams.attackTier].productionTicks
                + miningTiers[unitParams.miningTier].productionTicks
                + productionTiers[unitParams.productionTier].productionTicks
                + movementTiers[unitParams.movementTier].productionTicks
                + hullTiers[unitParams.hullTier].productionTicks
                + storageTiers[unitParams.storageTier].productionTicks;

            return max(1, productionTicks);
        }
    }
}
