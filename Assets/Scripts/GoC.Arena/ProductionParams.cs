using System;
using System.Collections.Generic;

namespace GoC.Arena
{
    [Serializable]
    public struct ProductionParams
    {
        public int attackTier;
        public int miningTier;
        public int productionTier;
        public int movementTier;
        public int hullTier;
        public int storageTier;

        public static ProductionParams FromObject(object productionParamsObject)
        {
            ProductionParams productionParams = new();
            if (productionParamsObject is IDictionary<string, object> productionParamsDictionary)
            {
                if (productionParamsDictionary.TryGetValue("attackTier", out object attackTierObject) && attackTierObject is double attackTier)
                    productionParams.attackTier = (int)attackTier;
                
                if (productionParamsDictionary.TryGetValue("miningTier", out object miningTierObject) && miningTierObject is double miningTier)
                    productionParams.miningTier = (int)miningTier;
                
                if (productionParamsDictionary.TryGetValue("productionTier", out object productionTierObject) && productionTierObject is double productionTier)
                    productionParams.productionTier = (int)productionTier;
                
                if (productionParamsDictionary.TryGetValue("movementTier", out object movementTierObject) && movementTierObject is double movementTier)
                    productionParams.movementTier = (int)movementTier;
                
                if (productionParamsDictionary.TryGetValue("hullTier", out object hullTierObject) && hullTierObject is double hullTier)
                    productionParams.hullTier = (int)hullTier;
                
                if (productionParamsDictionary.TryGetValue("storageTier", out object storageTierObject) && storageTierObject is double storageTier)
                    productionParams.storageTier = (int)storageTier;
            }
            return productionParams;
        }
    }
}
