using GoC.ScriptingApi;
using UnityEngine;

using static Unity.Mathematics.math;

namespace GoC.Arena
{
    public class UnitControl : MonoBehaviour, IUnitControl
    {
        private Unit _unit;
        private Unit unit => _unit ??= GetComponent<Unit>();

        public Result Attack(object target) => unit.Attack(target);
        public Result Mine(object target) => unit.Mine(target);
        public Result Give(object target, Substance.Type substanceType, int count) => unit.Give(target, substanceType, count);
        public Result MoveTo(object target) => unit.MoveTo(target);
        public Result MoveTo(int x, int y) => unit.MoveTo(int2(x, y));
    }
}
