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

        public IUnitParams unitParams => exposedUnitControl.unitParams;

        public int integrity => exposedUnitControl.integrity;
        public IReadOnlyList<int> storage => exposedUnitControl.storage;
        
        public Result CanMine(object target) => exposedUnitControl.CanMine(target);
        public Result CanProduce(string productName) => exposedUnitControl.CanProduce(productName);
    }
}
