using GoC.ScriptingApi;
using UnityEngine;
using UnityEngine.VFX;

namespace GoC
{
    public class Unit : FieldObjectBehaviour, IUnit
    {
        private class Control : IUnitControl
        {
            private readonly Unit _unit;
            public Control(Unit unit) => _unit = unit;

            public Result Move() => _unit.Move();
            public Result TurnLeft() => _unit.TurnLeft();
            public Result TurnRight() => _unit.TurnRight();
        }

        [SerializeField] private VisualEffect _exhaust;

        private Vector3? _targetPosition;
        private Quaternion? _targetRotation;
        private IUnitControl _control;

        public new void Init(int initTeamId, IExecutionContext executionContext)
        {
            base.Init(initTeamId, executionContext);
            _control = new Control(this);
        }

        public IUnitControl control => CheckExecutionRestrictions() ? _control : null;

        private Result Move()
        {
            // TODO: implement busy-ness
            //if (_targetPosition.HasValue)
              //  return Result.Busy;
            
            Settle();
            _targetPosition = transform.position + transform.forward;
            _exhaust.Play();

            return Result.Ok;
        }

        private Result TurnLeft()
        {
            Settle();
            _targetRotation = Quaternion.Euler(0, -90, 0) * transform.rotation;

            return Result.Ok;
        }

        private Result TurnRight()
        {
            Settle();
            _targetRotation = Quaternion.Euler(0, 90, 0) * transform.rotation;

            return Result.Ok;
        }

        void Start()
        {
            _exhaust.Stop();
        }

        void Update()
        {
            if(_targetPosition.HasValue)
            {
                float linearSpeed = 1 / Tick.TIME; // units per tick
                transform.position = Vector3.MoveTowards(transform.position, _targetPosition.Value, linearSpeed * Time.deltaTime);
            }

            if(_targetRotation.HasValue)
            {
                float rotationSpeed = 90 / Tick.TIME; // degrees per tick
                transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation.Value, rotationSpeed * Time.deltaTime);
            }
        }

        private void Settle()
        {
            if (_targetPosition.HasValue)
            {
                // eliminate rounding errors in position
                transform.position = new Vector3(Mathf.Round(_targetPosition.Value.x), Mathf.Round(_targetPosition.Value.y), Mathf.Round(_targetPosition.Value.z));
                _targetPosition = null;
            }

            if (_targetRotation.HasValue)
            {
                // eliminate rounding errors in rotation
                transform.rotation = Quaternion.Euler(0, Mathf.Round(_targetRotation.Value.eulerAngles.y / 90) * 90, 0);
                _targetRotation = null;
            }

            // stop engine emission
            _exhaust.Stop();
        }
    }
}
