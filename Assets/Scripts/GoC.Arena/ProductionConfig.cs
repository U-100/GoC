using System;
using System.Collections.Generic;
using UnityEngine;

using static Unity.Mathematics.math;

namespace GoC.Arena
{
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
        [SerializeField] private MovementTier[] _movementTiers;
        public IReadOnlyList<MovementTier> movementTiers => _movementTiers;
        
        [SerializeField] private HullTier[] _hullTiers;
        public IReadOnlyList<HullTier> hullTiers => _hullTiers;
        
        [SerializeField] private StorageTier[] _storageTiers;
        public IReadOnlyList<StorageTier> storageTiers => _storageTiers;

        public ProductionParams ClampProductionParams(ProductionParams productionParams) =>
            new()
            {
                movementTier = clamp(productionParams.movementTier, 0, movementTiers.Count - 1),
                hullTier = clamp(productionParams.hullTier, 0, hullTiers.Count - 1),
                storageTier = clamp(productionParams.storageTier, 0, storageTiers.Count - 1),
            };

        public int ProductionCost(ProductionParams productionParams, int substanceIndex)
        {
            productionParams = ClampProductionParams(productionParams);
            
            int cost = 0;
                
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
                movementTiers[productionParams.movementTier].productionTicks
                + hullTiers[productionParams.hullTier].productionTicks
                + storageTiers[productionParams.storageTier].productionTicks;

            return max(1, productionTicks);
        }
    }
}
