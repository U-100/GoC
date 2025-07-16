using System.Collections.Generic;
using GoC.ScriptingApi;
using Unity.Mathematics;

namespace GoC.Arena
{
    public class ExposedUnit : IUnit
    {
        public ExposedUnit(ExposedUnitControl exposedUnitControl) => this.exposedUnitControl = exposedUnitControl;
        
        public ExposedUnitControl exposedUnitControl { get; }

        public int teamId => exposedUnitControl.teamId;
        public int2 pos => exposedUnitControl.pos;
        public bool busy => exposedUnitControl.busy;

        public int integrity => exposedUnitControl.integrity;
        
        public int storageCapacityPerSubstance => exposedUnitControl.storageCapacityPerSubstance;
        public IReadOnlyList<int> storage => exposedUnitControl.storage;
        
        public bool canMove => exposedUnitControl.canMove;
        public bool canMine => exposedUnitControl.canMine;
        public int damage => exposedUnitControl.damage;
        public int maxProductionTier => exposedUnitControl.maxProductionTier;

        public Result CanMine(object target) => exposedUnitControl.CanMine(target);
        public Result CanProduce(object unitParamsObject) => exposedUnitControl.CanProduce(unitParamsObject);
    }
}
