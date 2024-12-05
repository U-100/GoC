using GoC.ScriptingApi;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;
using static Unity.Mathematics.math;

namespace GoC.Arena
{
    [RequireComponent(typeof(Hull))]
    [RequireComponent(typeof(Storage))]
    [RequireComponent(typeof(UnitControl))]
    public class Unit : MonoBehaviour, IUnit
    {
        private struct AttackCommand
        {
            public Hull targetHull;
        }

        private struct MineCommand
        {
            public Deposit deposit;
        }

        private struct GiveCommand
        {
            public Storage targetStorage;
            public Substance.Type substanceType;
            public int amount;
        }

        private struct MoveCommand
        {
            public int2 targetPosition;
        }

        [SerializeField] private Team _team;
        [SerializeField] private UnitConfig _config;
        [Space]
        [SerializeField] private VisualEffect _attackEffect;
        [SerializeField] private VisualEffect _mineEffect;
        [SerializeField] private VisualEffect _moveEffect;
        [SerializeField] private Renderer _skinColorSubjectRenderer; 

        private AttackCommand? _attackCommand;
        private MineCommand? _mineCommand;
        private GiveCommand? _giveCommand;
        private MoveCommand? _moveCommand;

        private ExposedProperty _attackEffectDistanceProperty;
        
        private Hull _myHull;
        private Hull myHull => _myHull ??= GetComponent<Hull>();

        private Storage _myStorage;
        private Storage myStorage => _myStorage ??= GetComponent<Storage>();

        private UnitControl _myUnitControl;
        private UnitControl myUnitControl => _myUnitControl ??= GetComponent<UnitControl>();


        public int teamId => _team.teamId;
        public int2 pos => transform.position.ToArenaPos();
        public int2 dir => transform.forward.ToArenaPos();

        private bool busy => _attackCommand != null || _mineCommand != null || _giveCommand != null || _moveCommand != null;
        
        public Result CanMine(object targetObject)
        {
            if (targetObject is not MonoBehaviour target)
                return Result.InvalidTarget;

            Deposit targetDeposit = target.GetComponent<Deposit>();
            if (targetDeposit == null)
                return Result.InvalidTarget;

            if (distance(pos, targetDeposit.pos) > 1)
                return Result.OutOfRange;

            if (myStorage.content[(int)targetDeposit.substanceType] + targetDeposit.miningSpeed > myStorage.capacityPerSubstance)
                return Result.NotEnoughStorageCapacity;

            return Result.Ok;
        }

        public IHull hull => myHull;
        public IStorage storage => myStorage;
        public IUnitControl control => _team.arena.executingTeamId == teamId ? myUnitControl : null;


        public Result Attack(object targetObject)
        {
            if (busy)
                return Result.Busy;
            
            if (targetObject is not MonoBehaviour target)
                return Result.InvalidTarget;

            Hull targetHull = target.GetComponent<Hull>();
            if (targetHull == null)
                return Result.InvalidTarget;

            if (distance(pos, target.transform.position.ToArenaPos()) > 3)
                return Result.OutOfRange;

            _attackCommand = new AttackCommand { targetHull = targetHull };

            return Result.Ok;
        }

        public Result Mine(object targetObject)
        {
            if (busy)
                return Result.Busy;
            
            Result canMine = CanMine(targetObject);
            if (canMine != Result.Ok)
                return canMine;

            _mineCommand = new MineCommand { deposit = ((MonoBehaviour)targetObject).GetComponent<Deposit>() };

            return Result.Ok;
        }

        public Result Give(object targetObject, Substance.Type substanceType, int amount)
        {
            if (busy)
                return Result.Busy;
            
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

            if (targetStorage.content[(int)substanceType] + amount > targetStorage.capacityPerSubstance)
                return Result.NotEnoughStorageCapacity;

            _giveCommand = new GiveCommand
            {
                targetStorage = targetStorage,
                substanceType = substanceType,
                amount = amount
            };

            return Result.Ok;
        }

        public Result MoveTo(object target)
        {
            if (busy)
                return Result.Busy;
            
            switch (target)
            {
                case int2 targetPos:
                    return MoveTo(targetPos);

                case MonoBehaviour targetMonoBehaviour:
                    int2 targetMonoBehaviourPos = targetMonoBehaviour.transform.position.ToArenaPos();
                    return MoveTo(targetMonoBehaviourPos);

                case object[] { Length: 2 } targetObjArr:
                    if (targetObjArr[0] is double x && targetObjArr[1] is double y)
                        return MoveTo(int2((int)x, (int)y));
                    break;
            }

            return Result.InvalidTarget;
        }

        public Result MoveTo(int2 p)
        {
            if (busy)
                return Result.Busy;
            
            int2 delta = (int2)round(p - transform.position.ToArenaPos());
            if (all(delta == 0))
                return Result.AlreadyThere;

            if (any(p < _team.arena.grid.min | _team.arena.grid.max < p))
                return Result.OutOfRange;

            int2 absDelta = abs(delta);

            int2 dirTo = sign(delta);
            int2 dir1 = absDelta.x > absDelta.y ? int2(dirTo.x, 0) : int2(0, dirTo.y);
            int2 dir2 = absDelta.x > absDelta.y ? int2(0, dirTo.y) : int2(dirTo.x, 0);

            if (_team.arena.grid[pos + dir1] == 0)
                _moveCommand = new MoveCommand { targetPosition = pos + dir1 };
            else if (_team.arena.grid[pos + dir2] == 0)
                _moveCommand = new MoveCommand { targetPosition = pos + dir2 };
            else
                return Result.PathBlocked;

            _team.arena.grid[pos]--;
            _team.arena.grid[_moveCommand.Value.targetPosition]++;

            return Result.Ok;
        }

        // to set up upon instantiation
        public void Setup(Team team, UnitConfig unitConfig)
        {
            _team = team;
            _config = unitConfig;

            myHull.Setup(_config.integrity);
            myStorage.Setup(_config.storageCapacityPerSubstance);

            _team.arena.grid[pos]++;

            ApplySkin(_team.skin);
        }

        private void ApplySkin(TeamSkin teamSkin)
        {
            _skinColorSubjectRenderer.material = teamSkin.mainColorMaterial;
        }

        public void BeforeTick()
        {
            if (_attackCommand != null)
            {
                _attackEffect.Stop();
                _attackCommand = null;
            }

            if (_mineCommand != null)
            {
                _mineEffect.Stop();
                _mineCommand = null;
            }

            // ReSharper disable once RedundantCheckBeforeAssignment
            if (_giveCommand != null)
            {
                _giveCommand = null;
            }

            if (_moveCommand != null)
            {
                transform.position = _moveCommand.Value.targetPosition.ToUnityPos();
                _moveEffect.Stop();
                _moveCommand = null;
            }

            myHull.BeforeTick();
        }

        public void AfterTick()
        {
            if (_attackCommand.HasValue)
            {
                _attackCommand.Value.targetHull.Absorb(_config.damage);
                
                float distance = Vector3.Distance(transform.position, _attackCommand.Value.targetHull.transform.position);
                _attackEffect.SetFloat(_attackEffectDistanceProperty, distance);
                _attackEffect.Play();
            }

            if (_mineCommand != null)
            {
                DepositMiningOutcome depositMiningOutcome = _mineCommand.Value.deposit.Mine();
                myStorage.Deposit(depositMiningOutcome.substanceType, depositMiningOutcome.amount);

                _mineEffect.Play();
            }

            if (_giveCommand != null)
            {
                myStorage.Withdraw(_giveCommand.Value.substanceType, _giveCommand.Value.amount);
                _giveCommand.Value.targetStorage.Deposit(_giveCommand.Value.substanceType, _giveCommand.Value.amount);
            }
            
            if (_moveCommand != null)
            {
            }

            myHull.AfterTick();
        }


        private void Awake()
        {
            _attackEffectDistanceProperty = "distance";
        }

        private void OnDisable()
        {
            _team.arena.grid[pos]--;

            if (_team != null)
                _team.Unregister(this);
        }

        private void Update()
        {
            if (_attackCommand != null)
            {
                RotateTowards(_attackCommand.Value.targetHull.transform.position);
            }
            
            if (_mineCommand != null)
            {
                RotateTowards(_mineCommand.Value.deposit.transform.position);
            }
            
            if (_giveCommand != null)
            {
                RotateTowards(_giveCommand.Value.targetStorage.transform.position);
            }
            
            if (_moveCommand != null)
            {
                Vector3 targetPosition = _moveCommand.Value.targetPosition.ToUnityPos();
                if (distance(transform.position, targetPosition) > Mathf.Epsilon)
                {
                    if (RotateTowards(targetPosition))
                    {
                        float linearSpeed = 2 / Tick.TIME; // units per tick
                        transform.position = Vector3.MoveTowards(transform.position, targetPosition, linearSpeed * Time.deltaTime);
                        _moveEffect.Play();
                    }
                    else
                        _moveEffect.Stop();
                }
            }
        }

        private bool RotateTowards(Vector3 position)
        {
            if (Vector3.Distance(position, transform.position) > 0.5f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(position - transform.position, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360 * Time.deltaTime);

                return Quaternion.Angle(transform.rotation, targetRotation) < 10;
            }

            return true;
        }
    }
}
