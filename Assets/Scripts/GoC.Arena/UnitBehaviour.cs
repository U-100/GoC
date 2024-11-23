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
        private readonly struct MineCommand
        {
            private readonly UnitBehaviour _unit;
            private readonly DepositBehaviour _depositBehaviour;
            
            public MineCommand(UnitBehaviour unit, DepositBehaviour depositBehaviour)
            {
                _unit = unit;
                _depositBehaviour = depositBehaviour;
            }

            public void BeforeTick() { }
            
            public void AfterTick()
            {
                DepositExtractionOutcome depositExtractionOutcome = _depositBehaviour.Extract();
                _unit.storageBehaviour.Deposit(depositExtractionOutcome.substanceType, depositExtractionOutcome.amount);
            
                _unit._mine.Play();
            }

            public void Update() => _unit.RotateTowards(_depositBehaviour.transform.position);
        }

        private readonly struct GiveCommand
        {
            private readonly UnitBehaviour _unit;
            private readonly StorageBehaviour _targetStorageBehaviour;
            private readonly Substance.Type _substanceType;
            private readonly int _amount;
            
            public GiveCommand(UnitBehaviour unit, StorageBehaviour targetStorageBehaviour, Substance.Type substanceType, int amount)
            {
                _unit = unit;
                _targetStorageBehaviour = targetStorageBehaviour;
                _substanceType = substanceType;
                _amount = amount;
            }

            public void BeforeTick() { }
            
            public void AfterTick()
            {
                if (_unit.storageBehaviour.Withdraw(_substanceType, _amount))
                    _targetStorageBehaviour.Deposit(_substanceType, _amount);
            }

            public void Update() => _unit.RotateTowards(_targetStorageBehaviour.transform.position);
        }

        private readonly struct MoveCommand
        {
            private readonly UnitBehaviour _unit;
            private readonly Vector3 _position;

            public MoveCommand(UnitBehaviour unit, Vector3 position)
            {
                _unit = unit;
                _position = position;
            }

            public void BeforeTick()
            {
                // eliminate rounding errors in position
                _unit.transform.position = new Vector3(Mathf.Round(_position.x), Mathf.Round(_position.y), Mathf.Round(_position.z));
                _unit._exhaust.Stop();
            }
            
            public void AfterTick() { }

            public void Update()
            {
                if (_unit.RotateTowards(_position))
                {
                    if(distance(_unit.transform.position, _position) > float.Epsilon)
                    {
                        float linearSpeed = 2 / Tick.TIME; // units per tick
                        _unit.transform.position = Vector3.MoveTowards(_unit.transform.position, _position, linearSpeed * Time.deltaTime);
                        _unit._exhaust.Play();
                    }
                    else
                        _unit._exhaust.Stop();
                }
            }
        }

        [SerializeField] private Team _team;
        [Space]
        [SerializeField] private VisualEffect _exhaust;
        [SerializeField] private VisualEffect _mine;
        
        private MineCommand? _mineCommand;
        private GiveCommand? _giveCommand;
        private MoveCommand? _moveCommand;

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
        
            _mineCommand = new MineCommand(this, ((MonoBehaviour)targetObject).GetComponent<DepositBehaviour>());
            
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

            _giveCommand = new GiveCommand(this, targetStorageBehaviour, substanceType, amount);
            
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

            _moveCommand = new MoveCommand(this, transform.position + dirTo.ToUnityPos());
            
            return Result.Ok;
        }
        
        // to set up upon instantiation
        public void Setup(Team team) => _team = team;

        public void BeforeTick()
        {
            _mineCommand?.BeforeTick();
            _mineCommand = null;

            _giveCommand?.BeforeTick();
            _giveCommand = null;
            
            _moveCommand?.BeforeTick();
            _moveCommand = null;
        }

        public void AfterTick()
        {
            if (_mineCommand != null)
                _mineCommand.Value.AfterTick();
            else if (_giveCommand != null)
                _giveCommand.Value.AfterTick();
            else if (_moveCommand != null)
                _moveCommand.Value.AfterTick();
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
            if (_mineCommand != null)
                _mineCommand.Value.Update();
            else if (_giveCommand != null)
                _giveCommand.Value.Update();
            else if (_moveCommand != null)
                _moveCommand.Value.Update();
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
