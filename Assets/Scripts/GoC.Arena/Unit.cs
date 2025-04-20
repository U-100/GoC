using System.Linq;
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
    public class Unit : MonoBehaviour
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

        private struct ProduceCommand
        {
            public int ticksLeft;
            public int2 productionPos;
            public ProductionParams productionParams;
        }

        private struct MoveCommand
        {
            public int2 targetPosition;
        }

        [SerializeField] private UnitConfig _config;
        [Space]
        [SerializeField] private VisualEffect _attackEffect;
        [SerializeField] private VisualEffect _mineEffect;
        [SerializeField] private VisualEffect _giveEffect;
        [Space]
        [SerializeField] private Unit _fabricatedUnitPrototype;
        [SerializeField] private VisualEffect _productionEffect;
        [Space]
        [SerializeField] private VisualEffect _moveEffect;
        [SerializeField] private Renderer _skinColorSubjectRenderer; 

        private ExposedProperty _attackEffectColorProperty;
        private ExposedProperty _attackEffectDistanceProperty;
        private ExposedProperty _mineEffectTexIndexProperty;
        private ExposedProperty _productionEffectDurationProperty;
        private ExposedProperty _productionEffectSpawnPositionProperty;
        private ExposedProperty _productionEffectColorProperty;

        private Team _team;
        
        private AttackCommand? _attackCommand;
        private MineCommand? _mineCommand;
        private GiveCommand? _giveCommand;
        private ProduceCommand? _produceCommand;
        private MoveCommand? _moveCommand;
        
        private Hull _myHull;
        private Hull myHull => _myHull ??= GetComponent<Hull>();

        private Storage _myStorage;
        private Storage myStorage => _myStorage ??= GetComponent<Storage>();

        private static readonly int[] EMPTY_INIT_STORAGE = Enumerable.Repeat(0, Substance.TYPES_COUNT).ToArray();

        
        public int teamId => _team.teamId;
        public int2 pos => transform.position.ToArenaPos();
        public int2 dir => transform.forward.ToArenaPos();
        public bool busy => _attackCommand != null || _mineCommand != null || _giveCommand != null || _produceCommand != null || _moveCommand != null;
        
        public Result CanMine(object targetObject)
        {
            if (targetObject is not Deposit targetDeposit)
                return Result.InvalidTarget;

            if (targetDeposit == null)
                return Result.InvalidTarget;

            if (distance(pos, targetDeposit.pos) > 1)
                return Result.OutOfRange;

            if (myStorage.content[(int)targetDeposit.substanceType] + targetDeposit.miningSpeed > myStorage.capacityPerSubstance)
                return Result.NotEnoughStorageCapacity;

            return Result.Ok;
        }
        
        public Result CanProduce(ProductionParams productionParams)
        {
            productionParams = _team.arena.config.productionConfig.ClampProductionParams(productionParams);
            for (int substanceIndex = 0; substanceIndex < Substance.TYPES_COUNT; ++substanceIndex)
                if (storage.content[substanceIndex] < _team.arena.config.productionConfig.ProductionCost(productionParams, substanceIndex))
                    return Result.NotEnoughSubstance;
            
            return Result.Ok;
        }

        public IHull hull => myHull;
        public IStorage storage => myStorage;


        public Result Attack(object targetObject)
        {
            if (busy)
                return Result.Busy;
            
            if (targetObject is not ExposedUnit targetExposedUnit)
                return Result.InvalidTarget;

            Hull targetHull = targetExposedUnit.unit.GetComponent<Hull>();
            if (targetHull == null)
                return Result.InvalidTarget;

            if (distance(pos, targetExposedUnit.unit.transform.position.ToArenaPos()) > 3)
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

            if (targetObject is not ExposedUnit targetExposedUnit)
                return Result.InvalidTarget;

            if (distance(pos, targetExposedUnit.unit.transform.position.ToArenaPos()) > 1)
                return Result.OutOfRange;

            Storage targetStorage = targetExposedUnit.unit.GetComponent<Storage>();
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
        
        public Result Produce(ProductionParams productionParams)
        {
            if (busy)
                return Result.Busy;

            Result canProduce = CanProduce(productionParams);
            if (canProduce != Result.Ok)
                return canProduce;
            
            _produceCommand = new ProduceCommand
            {
                ticksLeft = _config.productionTime,
                productionParams = _team.arena.config.productionConfig.ClampProductionParams(productionParams),
            };
            
            return Result.Ok;
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
            myStorage.Setup(_config.storageCapacityPerSubstance, EMPTY_INIT_STORAGE);

            _team.arena.grid[pos]++;

            ApplySkin(_team.skin);
        }

        private void ApplySkin(TeamSkin teamSkin)
        {
            _skinColorSubjectRenderer.material = teamSkin.mainColorMaterial;
            _attackEffect.SetVector4(_attackEffectColorProperty, teamSkin.vfxColorTint);
            _productionEffect.SetVector4(_productionEffectColorProperty, teamSkin.vfxColorTint);
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
                _giveEffect.Stop();
                _giveCommand = null;
            }

            if (_produceCommand != null)
            {
                ProduceCommand produceCommand = _produceCommand.Value;
                if (produceCommand.ticksLeft > 0)
                {
                    if (--produceCommand.ticksLeft == 0)
                    {
                        _team.arena.grid[produceCommand.productionPos]--;

                        Unit unit = Instantiate(_fabricatedUnitPrototype, produceCommand.productionPos.ToUnityPos(), Quaternion.identity);
                        unit.Setup(_team, _config.product);
                        _team.arena.Register(unit);

                        _produceCommand = null;
                    }
                    else
                        _produceCommand = produceCommand;
                }
            }

            if (_moveCommand != null)
            {
                transform.position = _moveCommand.Value.targetPosition.ToUnityPos();
                _moveEffect.Stop();
                _moveCommand = null;
            }

            if (myHull.BeforeTick() == Hull.State.Destroyed)
            {
                _team.arena.Unregister(this);

                _team.arena.grid[pos]--;

                if (_produceCommand != null)
                    _team.arena.grid[_produceCommand.Value.productionPos]--;

                Destroy(gameObject);
            }
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

                _mineEffect.SetUInt(_mineEffectTexIndexProperty, (uint)depositMiningOutcome.substanceType);
                _mineEffect.Play();
            }

            if (_giveCommand != null)
            {
                myStorage.Withdraw(_giveCommand.Value.substanceType, _giveCommand.Value.amount);
                _giveCommand.Value.targetStorage.Deposit(_giveCommand.Value.substanceType, _giveCommand.Value.amount);

                _giveEffect.Play();
            }

            if (_produceCommand != null)
            {
                ProduceCommand produceCommand = _produceCommand.Value;
                if(produceCommand.ticksLeft == _config.productionTime) // just started production
                {
                    for (int substanceIndex = 0; substanceIndex < Substance.TYPES_COUNT; substanceIndex++)
                        myStorage.Withdraw((Substance.Type)substanceIndex, _team.arena.config.productionConfig.ProductionCost(produceCommand.productionParams, substanceIndex));

                    produceCommand.productionPos = _team.arena.grid.FindEmptyCellAround(pos);
                    
                    _team.arena.grid[produceCommand.productionPos]++;
                
                    _productionEffect.SetVector3(_productionEffectSpawnPositionProperty, produceCommand.productionPos.ToUnityPos());
                    _productionEffect.SetFloat(_productionEffectDurationProperty, _config.productionTime * Tick.TIME);
                    _productionEffect.Play();
                }
                _produceCommand = produceCommand;
            }
            
            if (_moveCommand != null)
            {
            }

            myHull.AfterTick();
        }


        private void Awake()
        {
            _attackEffectColorProperty = "color";
            _attackEffectDistanceProperty = "distance";
            _mineEffectTexIndexProperty = "texIndex";
            _productionEffectSpawnPositionProperty = "spawnPosition_position";
            _productionEffectDurationProperty = "duration";
            _productionEffectColorProperty = "color";
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

            if (_produceCommand != null)
            {
                RotateTowards(_produceCommand.Value.productionPos.ToUnityPos());
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
