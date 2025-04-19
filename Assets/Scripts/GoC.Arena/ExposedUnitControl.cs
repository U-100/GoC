using GoC.ScriptingApi;
using Unity.Mathematics;

using static Unity.Mathematics.math;

namespace GoC.Arena
{
    public class ExposedUnitControl : ExposedUnit, IUnitControl
    {
        public ExposedUnitControl(Unit unit) : base(unit) { }
        
        public Result Attack(object target) => unit.Attack(target);
        public Result Mine(object target) => unit.Mine(target);
        public Result Give(object target, Substance.Type substanceType, int amount) => unit.Give(target, substanceType, amount);
        public Result Produce(object productionParamsObject) => unit.Produce(ProductionParams.FromObject(productionParamsObject));
        public Result MoveTo(int2 p) => unit.MoveTo(p);
        public Result MoveTo(int x, int y) => unit.MoveTo(int2(x, y));
        public Result MoveTo(int[] p) => unit.MoveTo(int2(p[0], p[1]));
    }
}
