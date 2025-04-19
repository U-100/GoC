using System.Collections.Generic;

namespace GoC.Arena
{
    public struct ProductionParams
    {
        public int movementTier;
        public int hullTier;
        public int storageTier;

        public static ProductionParams FromObject(object productionParamsObject)
        {
            ProductionParams productionParams = new();
            if (productionParamsObject is IDictionary<string, object> productionParamsDictionary)
            {
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
