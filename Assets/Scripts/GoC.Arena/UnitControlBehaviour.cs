using GoC.ScriptingApi;
using UnityEngine;

namespace GoC.Arena
{
    public class UnitControlBehaviour : MonoBehaviour, IUnitControl
    {
        private UnitBehaviour _unitBehaviour;
        private UnitBehaviour unitBehaviour => _unitBehaviour ??= GetComponent<UnitBehaviour>();

        public Result Attack(object target) => unitBehaviour.Attack(target);
        public Result Mine(object target) => unitBehaviour.Mine(target);
        public Result Give(object target, Substance.Type substanceType, int count) => unitBehaviour.Give(target, substanceType, count);
        public Result MoveTo(object target) => unitBehaviour.MoveTo(target);
        public Result MoveTo(int x, int y) => unitBehaviour.MoveTo(x, y);
    }
}
