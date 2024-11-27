using GoC.ScriptingApi;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

using static Unity.Mathematics.math;

namespace GoC.Arena
{
    [RequireComponent(typeof(Hull))]
    [RequireComponent(typeof(Storage))]
    [RequireComponent(typeof(UnitControl))]
    public class Unit : MonoBehaviour, IUnit
    {
        private readonly struct AttackCommand
        {
            private readonly Unit _unit;
            private readonly Hull _targetHull;
            
            public AttackCommand(Unit unit, Hull targetHull)
            {
                _unit = unit;
                _targetHull = targetHull;
            }

            public void BeforeTick()
            {
                _unit._attackEffect.Stop();
            }

            public void AfterTick()
            {
                _unit._attackEffect.Play();
            }

            public void Update() => _unit.RotateTowards(_targetHull.transform.position);
        }
        
        private readonly struct MineCommand
        {
            private readonly Unit _unit;
            private readonly Deposit _deposit;
            
            public MineCommand(Unit unit, Deposit deposit)
            {
                _unit = unit;
                _deposit = deposit;
            }

            public void BeforeTick()
            {
                _unit._mineEffect.Stop();
            }
            
            public void AfterTick()
            {
                DepositExtractionOutcome depositExtractionOutcome = _deposit.Extract();
                _unit.myStorage.Deposit(depositExtractionOutcome.substanceType, depositExtractionOutcome.amount);
            
                _unit._mineEffect.Play();
            }

            public void Update() => _unit.RotateTowards(_deposit.transform.position);
        }

        private readonly struct GiveCommand
        {
            private readonly Unit _unit;
            private readonly Storage _targetStorage;
            private readonly Substance.Type _substanceType;
            private readonly int _amount;
            
            public GiveCommand(Unit unit, Storage targetStorage, Substance.Type substanceType, int amount)
            {
                _unit = unit;
                _targetStorage = targetStorage;
                _substanceType = substanceType;
                _amount = amount;
            }

            public void BeforeTick() { }
            
            public void AfterTick()
            {
                if (_unit.myStorage.Withdraw(_substanceType, _amount))
                    _targetStorage.Deposit(_substanceType, _amount);
            }

            public void Update() => _unit.RotateTowards(_targetStorage.transform.position);
        }

        private readonly struct MoveCommand
        {
            private readonly Unit _unit;
            private readonly Vector3 _position;

            public MoveCommand(Unit unit, Vector3 position)
            {
                _unit = unit;
                _position = position;
            }

            public void BeforeTick()
            {
                // eliminate rounding errors in position
                _unit.transform.position = new Vector3(Mathf.Round(_position.x), Mathf.Round(_position.y), Mathf.Round(_position.z));
                _unit._moveEffect.Stop();
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
                        _unit._moveEffect.Play();
                    }
                    else
                        _unit._moveEffect.Stop();
                }
            }
        }

        [SerializeField] private Team _team;
        [Space]
        [SerializeField] private VisualEffect _attackEffect;
        [SerializeField] private VisualEffect _mineEffect;
        [SerializeField] private VisualEffect _moveEffect;
        
        private AttackCommand? _attackCommand;
        private MineCommand? _mineCommand;
        private GiveCommand? _giveCommand;
        private MoveCommand? _moveCommand;

        private Hull _myHull;
        private Hull myHull => _myHull ??= GetComponent<Hull>();
        
        private Storage _myStorage;
        private Storage myStorage => _myStorage ??= GetComponent<Storage>();
        
        private UnitControl _myUnitControl;
        private UnitControl myUnitControl => _myUnitControl ??= GetComponent<UnitControl>();

        
        public int teamId => _team.teamId;
        public int2 pos => transform.position.ToArenaPos();
        public int2 dir => transform.forward.ToArenaPos();

        public Result CanMine(object targetObject)
        {
            if (targetObject is not MonoBehaviour target)
                return Result.InvalidTarget;

            Deposit targetDeposit = target.GetComponent<Deposit>();
            if (targetDeposit == null)
                return Result.InvalidTarget;

            if (distance(pos, targetDeposit.pos) > 1) 
                return Result.OutOfRange;

            return Result.Ok;
        }

        public IHull hull => myHull;
        public IStorage storage => _myStorage ??= GetComponent<Storage>();
        public IUnitControl control => _team.arena.executingTeamId == teamId ? myUnitControl : null;

        
        public Result Attack(object targetObject)
        {
            if (targetObject is not MonoBehaviour target)
                return Result.InvalidTarget;

            Hull targetHull = target.GetComponent<Hull>();
            if (targetHull == null)
                return Result.InvalidTarget;
            
            if (distance(pos, target.transform.position.ToArenaPos()) > 3) 
                return Result.OutOfRange;

            _attackCommand = new AttackCommand(this, targetHull);

            return Result.Ok;
        }
        
        public Result Mine(object targetObject)
        {
            Result canMine = CanMine(targetObject);
            if (canMine != Result.Ok)
                return canMine;
        
            _mineCommand = new MineCommand(this, ((MonoBehaviour)targetObject).GetComponent<Deposit>());
            
            return Result.Ok;
        }
        
        public Result Give(object targetObject, Substance.Type substanceType, int amount)
        {
            if (amount <= 0)
                return Result.InvalidArgument;
            
            if (targetObject is not MonoBehaviour target)
                return Result.InvalidTarget;

            if (distance(pos, target.transform.position.ToArenaPos()) > 1)
                return Result.OutOfRange;

            Storage targetStorage = target.GetComponent<Storage>();
            if (targetStorage == null)
                return Result.InvalidTarget;
            
            if (myStorage.content[(int)substanceType] < amount)
                return Result.NotEnoughSubstance;

            _giveCommand = new GiveCommand(this, targetStorage, substanceType, amount);
            
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
            _attackCommand?.BeforeTick();
            _attackCommand = null;
            
            _mineCommand?.BeforeTick();
            _mineCommand = null;

            _giveCommand?.BeforeTick();
            _giveCommand = null;
            
            _moveCommand?.BeforeTick();
            _moveCommand = null;
        }

        public void AfterTick()
        {
            if (_attackCommand != null)
                _attackCommand.Value.AfterTick();
            else if (_mineCommand != null)
                _mineCommand.Value.AfterTick();
            else if (_giveCommand != null)
                _giveCommand.Value.AfterTick();
            else if (_moveCommand != null)
                _moveCommand.Value.AfterTick();
        }

        
        private void OnEnable()
        {
            if (_team != null)
                _team.Register(this);
            else
                Debug.LogWarning("A unit without a team", this);
        }

        private void OnDisable()
        {
            if (_team != null)
                _team.Unregister(this);
        }

        private void Update()
        {
            if (_attackCommand != null)
                _attackCommand.Value.Update();
            else if (_mineCommand != null)
                _mineCommand.Value.Update();
            else if (_giveCommand != null)
                _giveCommand.Value.Update();
            else if (_moveCommand != null)
                _moveCommand.Value.Update();
        }

        private bool RotateTowards(Vector3 position)
        {
            if (distancesq(position, transform.position) > Mathf.Epsilon)
            {
                Quaternion targetRotation = Quaternion.LookRotation(position - transform.position, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360 * Time.deltaTime);

                return Quaternion.Angle(transform.rotation, targetRotation) < 10;
            }

            return true;
        }
    }
}
