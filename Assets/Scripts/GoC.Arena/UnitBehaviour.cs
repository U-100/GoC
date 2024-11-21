using System.Collections.Generic;
using GoC.ScriptingApi;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

using static Unity.Mathematics.math;

namespace GoC.Arena
{
    [RequireComponent(typeof(StorageBehaviour))]
    [RequireComponent(typeof(UnitControlBehaviour))]
    public class UnitBehaviour : MonoBehaviour, IUnit
    {
        [SerializeField] private Team _team;
        [Space]
        [SerializeField] private VisualEffect _exhaust;
        
        private Vector3? _targetPosition;
        private Quaternion? _targetRotation;

        private IStorageBehaviour _storageBehaviour;
        private IStorageBehaviour storageBehaviour => _storageBehaviour ??= GetComponent<IStorageBehaviour>();
        
        // to set up upon instantiation
        public void Setup(Team team)
        {
            _team = team;
        }

        public int teamId => _team.teamId;
        public int2 pos => transform.position.ToArenaPos();

        public IReadOnlyList<int> storage => storageBehaviour.content;

        private UnitControlBehaviour _unitControlBehaviour;
        public IUnitControl control =>
            _team.arena.executingTeamId == teamId
                ? _unitControlBehaviour ??= GetComponent<UnitControlBehaviour>()
                : null;

        public Result Mine(object targetObject)
        {
            return Result.Ok;
        }
        
        public Result Give(object targetObject, Substance.Type substanceType, int amount)
        {
            if (amount <= 0)
                return Result.InvalidArgument;
            
            if (targetObject is not MonoBehaviour target)
                return Result.InvalidTarget;

            if (distance(pos, target.transform.position.ToArenaPos()) > 1)
                return Result.TooFar;

            IStorageBehaviour targetStorageBehaviour = target.GetComponent<IStorageBehaviour>();
            if (targetStorageBehaviour == null)
                return Result.InvalidTarget;
            
            if (storageBehaviour.Withdraw(substanceType, amount))
            {
                targetStorageBehaviour.Deposit(substanceType, amount);
                return Result.Ok;
            } 

            return Result.NotEnoughSubstance;
        }

        public Result Move()
        {
            // TODO: implement busy-ness
            //if (_targetPosition.HasValue)
              //  return Result.Busy;
            
            Settle();
            _targetPosition = transform.position + transform.forward;
            _exhaust.Play();

            return Result.Ok;
        }

        public Result TurnLeft()
        {
            Settle();
            _targetRotation = Quaternion.Euler(0, -90, 0) * transform.rotation;

            return Result.Ok;
        }

        public Result TurnRight()
        {
            Settle();
            _targetRotation = Quaternion.Euler(0, 90, 0) * transform.rotation;

            return Result.Ok;
        }

        private void OnEnable()
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
