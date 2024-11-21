using GoC.ScriptingApi;
using UnityEngine;

namespace GoC.Arena
{
    public class UnitControlBehaviour : MonoBehaviour, IUnitControl
    {
        private UnitBehaviour _unitBehaviour;
        private UnitBehaviour unitBehaviour => _unitBehaviour ??= GetComponent<UnitBehaviour>();

        public Result Mine(object target) => unitBehaviour.Mine(target);
        public Result Give(object target, Substance.Type substanceType, int count) => unitBehaviour.Give(target, substanceType, count);
        public Result Move() => unitBehaviour.Move();
        public Result TurnLeft() => unitBehaviour.TurnLeft();
        public Result TurnRight() => unitBehaviour.TurnRight();
    }
}
