using System;
using System.Collections.Generic;

namespace GoC.Arena
{
    [Serializable]
    public struct UnitParams
    {
        public int attackTier;
        public int miningTier;
        public int productionTier;
        public int movementTier;
        public int hullTier;
        public int storageTier;

        public static UnitParams FromObject(object unitParamsObject)
        {
            UnitParams unitParams = new();
            if (unitParamsObject is IDictionary<string, object> unitParamsDictionary)
            {
                if (unitParamsDictionary.TryGetValue("attackTier", out object attackTierObject) && attackTierObject is double attackTier)
                    unitParams.attackTier = (int)attackTier;
                
                if (unitParamsDictionary.TryGetValue("miningTier", out object miningTierObject) && miningTierObject is double miningTier)
                    unitParams.miningTier = (int)miningTier;
                
                if (unitParamsDictionary.TryGetValue("productionTier", out object productionTierObject) && productionTierObject is double productionTier)
                    unitParams.productionTier = (int)productionTier;
                
                if (unitParamsDictionary.TryGetValue("movementTier", out object movementTierObject) && movementTierObject is double movementTier)
                    unitParams.movementTier = (int)movementTier;
                
                if (unitParamsDictionary.TryGetValue("hullTier", out object hullTierObject) && hullTierObject is double hullTier)
                    unitParams.hullTier = (int)hullTier;
                
                if (unitParamsDictionary.TryGetValue("storageTier", out object storageTierObject) && storageTierObject is double storageTier)
                    unitParams.storageTier = (int)storageTier;
            }
            return unitParams;
        }
    }
}
