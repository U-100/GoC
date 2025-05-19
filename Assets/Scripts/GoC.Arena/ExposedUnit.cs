using GoC.ScriptingApi;
using Unity.Mathematics;

namespace GoC.Arena
{
    public class ExposedUnit : IUnit
    {
        public ExposedUnit(Unit unit) => this.unit = unit;

        public Unit unit { get; }

        public int teamId => unit.teamId;
        public int2 pos => unit.pos;
        public int2 dir => unit.dir;
        public bool busy => unit.busy;
        public bool canMove => unit.canMove;

        public Result CanMine(object target) => unit.CanMine(target);
        public Result CanProduce(object unitParamsObject) => unit.CanProduce(UnitParams.FromObject(unitParamsObject));

        public IHull hull => unit.hull;
        public IStorage storage => unit.storage;
    }
}
