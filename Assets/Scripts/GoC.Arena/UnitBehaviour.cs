using System;
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
        [SerializeField] private VisualEffect _mine;
        
        private Vector3? _targetPosition;
        private Quaternion? _targetRotation;

        private IStorageBehaviour _storageBehaviour;
        private IStorageBehaviour storageBehaviour => _storageBehaviour ??= GetComponent<IStorageBehaviour>();
        
        public int teamId => _team.teamId;
        public int2 pos => transform.position.ToArenaPos();
        public int2 dir => transform.forward.ToArenaPos();

        public IReadOnlyList<int> storage => storageBehaviour.content;

        private UnitControlBehaviour _unitControlBehaviour;
        public IUnitControl control =>
            _team.arena.executingTeamId == teamId
                ? _unitControlBehaviour ??= GetComponent<UnitControlBehaviour>()
                : null;

        public Result CanMine(object targetObject)
        {
            if (targetObject is not MonoBehaviour target)
                return Result.InvalidTarget;

            DepositBehaviour targetDepositBehaviour = target.GetComponent<DepositBehaviour>();
            if (targetDepositBehaviour == null)
                return Result.InvalidTarget;

            if (!all(pos + dir == targetDepositBehaviour.pos)) 
                return Result.InvalidPosition;

            return Result.Ok;
        }
        
        public Result Mine(object targetObject)
        {
            Result canMine = CanMine(targetObject);
            if (canMine != Result.Ok)
                return canMine;
            
            DepositBehaviour targetDepositBehaviour = ((MonoBehaviour)targetObject).GetComponent<DepositBehaviour>();
            DepositExtractionOutcome depositExtractionOutcome = targetDepositBehaviour.Extract();
            storageBehaviour.Deposit(depositExtractionOutcome.substanceType, depositExtractionOutcome.amount);
            
            _mine.Play();
            
            return Result.Ok;
        }
        
        public Result Give(object targetObject, Substance.Type substanceType, int amount)
        {
            if (amount <= 0)
                return Result.InvalidArgument;
            
            if (targetObject is not MonoBehaviour target)
                return Result.InvalidTarget;

            if (distance(pos, target.transform.position.ToArenaPos()) > 1)
                return Result.InvalidPosition;

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

        public Result MoveTo(object target)
        {
            switch (target)
            {
                case int2 targetPos:
                    return MoveTo(targetPos.x, targetPos.y);
                
                case MonoBehaviour targetMonoBehaviour:
                    int2 targetMonoBehaviourPos = targetMonoBehaviour.transform.position.ToArenaPos();
                    return MoveTo(targetMonoBehaviourPos.x, targetMonoBehaviourPos.y);
                
                case object[] {Length: 2} targetObjArr:
                    if (targetObjArr[0] is double x && targetObjArr[1] is double y)
                        return MoveTo((int)x, (int)y);
                    break;
            }
            return Result.InvalidTarget;
        }

        public Result MoveTo(int x, int y)
        {
            int2 delta = (int2)round(int2(x, y) - transform.position.ToArenaPos());
            int2 absDelta = abs(delta);
            if (all(absDelta == 0))
                return Result.Ok;

            int2 dirTo = sign(delta);

            if (absDelta.x > absDelta.y)
                dirTo.y = 0;
            else
                dirTo.x = 0;

            if (all(dirTo == dir))
                Move();
            else
            {
                if (cross(transform.forward, float3(dirTo.x, 0, dirTo.y)).y < 0)
                    TurnLeft();
                else
                    TurnRight();
            }
            
            return Result.Ok;
        }
        
        public Result Move()
        {
            _targetPosition = transform.position + transform.forward;
            _exhaust.Play();
            return Result.Ok;
        }

        public Result TurnLeft()
        {
            _targetRotation = Quaternion.Euler(0, -90, 0) * transform.rotation;
            return Result.Ok;
        }

        public Result TurnRight()
        {
            _targetRotation = Quaternion.Euler(0, 90, 0) * transform.rotation;
            return Result.Ok;
        }

        // to set up upon instantiation
        public void Setup(Team team) => _team = team;

        public void BeforeTick()
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

            _exhaust.Stop(); // stop engine emission
        }

        public void AfterTick() { }

        void OnEnable()
        {
            if (_team != null)
                _team.Register(this);
            else
                Debug.LogWarning("A unit without a team", this);
        }

        void OnDisable()
        {
            if (_team != null)
                _team.Unregister(this);
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
    }
}
