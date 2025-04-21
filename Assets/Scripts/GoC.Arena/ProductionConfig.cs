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
        
        [Tooltip("highest tier of production params that can be produced, -1 means no production is possible")]
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
    public class ProductionConfig : ScriptableObject
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

        public ProductionParams ClampProductionParams(ProductionParams productionParams) =>
            new()
            {
                attackTier = clamp(productionParams.attackTier, 0, attackTiers.Count - 1),
                miningTier = clamp(productionParams.miningTier, 0, miningTiers.Count - 1),
                productionTier = clamp(productionParams.productionTier, 0, productionTiers.Count - 1),
                movementTier = clamp(productionParams.movementTier, 0, movementTiers.Count - 1),
                hullTier = clamp(productionParams.hullTier, 0, hullTiers.Count - 1),
                storageTier = clamp(productionParams.storageTier, 0, storageTiers.Count - 1),
            };

        public int ProductionCost(ProductionParams productionParams, int substanceIndex)
        {
            productionParams = ClampProductionParams(productionParams);
            
            int cost = 0;

            if (attackTiers[productionParams.attackTier].productionCost.Count > substanceIndex)
                cost += attackTiers[productionParams.attackTier].productionCost[substanceIndex];
                
            if (miningTiers[productionParams.miningTier].productionCost.Count > substanceIndex)
                cost += miningTiers[productionParams.miningTier].productionCost[substanceIndex];
                
            if (productionTiers[productionParams.productionTier].productionCost.Count > substanceIndex)
                cost += productionTiers[productionParams.productionTier].productionCost[substanceIndex];
                
            if (movementTiers[productionParams.movementTier].productionCost.Count > substanceIndex)
                cost += movementTiers[productionParams.movementTier].productionCost[substanceIndex];
                
            if (hullTiers[productionParams.hullTier].productionCost.Count > substanceIndex)
                cost += hullTiers[productionParams.hullTier].productionCost[substanceIndex];
                
            if (storageTiers[productionParams.storageTier].productionCost.Count > substanceIndex)
                cost += storageTiers[productionParams.storageTier].productionCost[substanceIndex];

            return cost;
        }

        public int ProductionTicks(ProductionParams productionParams)
        {
            productionParams = ClampProductionParams(productionParams);

            int productionTicks =
                attackTiers[productionParams.attackTier].productionTicks
                + miningTiers[productionParams.miningTier].productionTicks
                + productionTiers[productionParams.productionTier].productionTicks
                + movementTiers[productionParams.movementTier].productionTicks
                + hullTiers[productionParams.hullTier].productionTicks
                + storageTiers[productionParams.storageTier].productionTicks;

            return max(1, productionTicks);
        }
    }
}
