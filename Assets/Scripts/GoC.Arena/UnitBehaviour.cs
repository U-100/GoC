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
        
        private Vector3? _movePosition;
        private DepositBehaviour _mineDepositBehaviour;
        private StorageBehaviour _giveStorageBehaviour;
        private Substance.Type? _giveSubstanceType;
        private int? _giveAmount;

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

            if (distance(pos, targetDepositBehaviour.pos) > 1) 
                return Result.InvalidPosition;

            return Result.Ok;
        }
        
        public Result Mine(object targetObject)
        {
            Result canMine = CanMine(targetObject);
            if (canMine != Result.Ok)
                return canMine;
            
            _mineDepositBehaviour = ((MonoBehaviour)targetObject).GetComponent<DepositBehaviour>();
            
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

            StorageBehaviour targetStorageBehaviour = target.GetComponent<StorageBehaviour>();
            if (targetStorageBehaviour == null)
                return Result.InvalidTarget;
            
            if (storageBehaviour.content[(int)substanceType] < amount)
                return Result.NotEnoughSubstance;

            _giveStorageBehaviour = targetStorageBehaviour;
            _giveSubstanceType = substanceType;
            _giveAmount = amount;
            
            return Result.Ok;
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

            _movePosition = transform.position + dirTo.ToUnityPos();
            _exhaust.Play();
            
            return Result.Ok;
        }
        
        // to set up upon instantiation
        public void Setup(Team team) => _team = team;

        public void BeforeTick()
        {
            if (_movePosition.HasValue)
            {
                // eliminate rounding errors in position
                transform.position = new Vector3(Mathf.Round(_movePosition.Value.x), Mathf.Round(_movePosition.Value.y), Mathf.Round(_movePosition.Value.z));
                _movePosition = null;
            }

            _exhaust.Stop(); // stop engine emission

            _mineDepositBehaviour = null;
            _giveStorageBehaviour = null;
            _giveSubstanceType = null;
            _giveAmount = null;
        }

        public void AfterTick()
        {
            if (_mineDepositBehaviour != null)
            {
                DepositExtractionOutcome depositExtractionOutcome = _mineDepositBehaviour.Extract();
                storageBehaviour.Deposit(depositExtractionOutcome.substanceType, depositExtractionOutcome.amount);
            
                _mine.Play();
            }
            else if (_giveStorageBehaviour != null && _giveSubstanceType != null && _giveAmount != null)
            {
                if (storageBehaviour.Withdraw(_giveSubstanceType.Value, _giveAmount.Value))
                    _giveStorageBehaviour.Deposit(_giveSubstanceType.Value, _giveAmount.Value);
            }
        }

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
            if (_movePosition.HasValue)
            {
                if (RotateTowards(_movePosition.Value))
                {
                    float linearSpeed = 2 / Tick.TIME; // units per tick
                    transform.position = Vector3.MoveTowards(transform.position, _movePosition.Value, linearSpeed * Time.deltaTime);
                }
            }
            else if (_mineDepositBehaviour != null)
            {
                RotateTowards(_mineDepositBehaviour.transform.position);
            }
            else if (_giveStorageBehaviour != null)
            {
                RotateTowards(_giveStorageBehaviour.transform.position);
            }
        }

        private bool RotateTowards(Vector3 position)
        {
            if (distancesq(position, transform.position) > float.Epsilon)
            {
                Quaternion targetRotation = Quaternion.LookRotation(position - transform.position, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360 * Time.deltaTime);

                return Quaternion.Angle(transform.rotation, targetRotation) < 10;
            }

            return true;
        }
    }
}
