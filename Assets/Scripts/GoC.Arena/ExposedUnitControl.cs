using System;
using System.Collections.Generic;
using System.Linq;
using GoC.ScriptingApi;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Mathematics.math;

namespace GoC.Arena
{
    public class ExposedUnitControl : IUnitControl, IDisposable
    {
        private struct AttackCommand
        {
            public ExposedUnitControl target;
        }

        private struct MineCommand
        {
            public Deposit deposit;
        }

        private struct GiveCommand
        {
            public ExposedUnitControl target;
            public Substance.Type substanceType;
            public int amount;
        }

        private struct ProduceCommand
        {
            public int ticks;
            public int2 productionPos;
            public UnitParams productUnitParams;
        }

        private struct MoveCommand
        {
            public int2 targetPosition;
        }

        private readonly Team _team;
        private readonly UnitParams _unitParams;
        private readonly UnitView _unitView;
        private readonly int[] _storage;
        
        private AttackCommand? _attackCommand;
        private MineCommand? _mineCommand;
        private GiveCommand? _giveCommand;
        private ProduceCommand? _produceCommand;
        private MoveCommand? _moveCommand;
        
        // not really a command, it's current production state.
        // Required to track production through multiple ticks
        private ProduceCommand? _currentProduction;
        
        public ExposedUnitControl(Team team, UnitParams unitParams, int2 pos, UnitView unitView)
        {
            _team = team;
            _unitParams = unitParams;
            _unitView = unitView;
            
            this.pos = pos;

            integrity = this.unitParams.maxIntegrity;
            _storage = Enumerable.Repeat(0, Substance.TYPES_COUNT).ToArray();
            
            _team.arena.grid[pos]++;

            _unitView.transform.position = pos.ToUnityPos();
        }

        public void Dispose()
        {
            if (_unitView != null)
                UnityEngine.Object.Destroy(_unitView.gameObject);
        }

        public int teamId => _team.teamId;
        
        public int2 pos { get; private set; }

        public bool busy => 
            _attackCommand != null
            || _mineCommand != null
            || _giveCommand != null
            || _produceCommand != null
            || _moveCommand != null
            || _currentProduction != null;

        public IUnitParams unitParams => _unitParams;

        public int integrity { get; private set; }
        public IReadOnlyList<int> storage => _storage;
        
        public Result CanMine(object targetObject)
        {
            if (!unitParams.canMine)
                return Result.OperationUnavailable;
            
            if (targetObject is not Deposit targetDeposit)
                return Result.InvalidTarget;

            if (targetDeposit == null)
                return Result.InvalidTarget;

            if (distance(pos, targetDeposit.pos) > 1)
                return Result.OutOfRange;

            if (storage[(int)targetDeposit.substanceType] + targetDeposit.miningSpeed > unitParams.storageCapacityPerSubstance)
                return Result.NotEnoughStorageCapacity;

            return Result.Ok;
        }

        public Result CanProduce(string productName)
        {
            if (TryFindProductByName(productName, out UnitParams productUnitParams))
            {
                for (int substanceIndex = 0; substanceIndex < Substance.TYPES_COUNT; ++substanceIndex)
                    if (storage[substanceIndex] < productUnitParams.productionCost[substanceIndex])
                        return Result.NotEnoughSubstance;
            
                return Result.Ok;
            }
            
            return Result.OperationUnavailable;
        }

        private bool TryFindProductByName(string productName, out UnitParams productUnitParams)
        {
            foreach (UnitParams possibleProductUnitParams in unitParams.products)
            {
                if (possibleProductUnitParams.name == productName)
                {
                    productUnitParams = possibleProductUnitParams;
                    return true;
                }
            }

            productUnitParams = null;
            return false;
        }

        private string _speech;
        
        public Result Say(string phrase)
        {
            _speech = phrase;
            return Result.Ok;
        }

        public Result Attack(object targetObject)
        {
            if (busy)
                return Result.Busy;
            
            if (unitParams.damage <= 0)
                return Result.OperationUnavailable;
            
            if (targetObject is not ExposedUnit targetExposedUnit)
                return Result.InvalidTarget;

            if (distance(pos, targetExposedUnit.pos) > 3)
                return Result.OutOfRange;

            _attackCommand = new AttackCommand { target = targetExposedUnit.exposedUnitControl };

            return Result.Ok;
        }

        public Result Mine(object targetObject)
        {
            if (busy)
                return Result.Busy;
            
            Result canMineIt = CanMine(targetObject);
            if (canMineIt != Result.Ok)
                return canMineIt;

            _mineCommand = new MineCommand { deposit = (Deposit)targetObject };

            return Result.Ok;
        }
        
        public Result Give(object targetObject, Substance.Type substanceType, int amount)
        {
            if (busy)
                return Result.Busy;
            
            if (amount <= 0)
                return Result.InvalidArgument;

            if (targetObject is not ExposedUnitControl target)
                return Result.InvalidTarget;

            if (distance(pos, target.pos) > 1)
                return Result.OutOfRange;

            if (storage[(int)substanceType] < amount)
                return Result.NotEnoughSubstance;

            if (target.storage[(int)substanceType] + amount > target.unitParams.storageCapacityPerSubstance)
                return Result.NotEnoughStorageCapacity;

            _giveCommand = new GiveCommand
            {
                target = target,
                substanceType = substanceType,
                amount = amount
            };

            return Result.Ok;
        }
        
        public Result Produce(string productName) => ProduceImpl(productName, null);
        public Result Produce(string productName, int2 p) => ProduceImpl(productName, p);
        public Result Produce(string productName, int x, int y) => ProduceImpl(productName, new int2(x, y));
        public Result Produce(string productName, int[] p) => ProduceImpl(productName, new int2(p[0], p[1]));

        private Result ProduceImpl(string productName, int2? productionPos)
        {
            if (busy)
                return Result.Busy;

            if (productionPos != null)
            {
                if (distance(pos, productionPos.Value) > 1)
                    return Result.OutOfRange;

                if (_team.arena.grid[productionPos.Value] > 0)
                    return Result.PathBlocked;
            }
            
            Result canProduce = CanProduce(productName);
            if (canProduce != Result.Ok)
                return canProduce;

            if (!TryFindProductByName(productName, out UnitParams productUnitParams))
                throw new Exception($"Can produce {productName}, but can't find it's unit params");

            _produceCommand = new ProduceCommand
            {
                ticks = productUnitParams.productionTicks,
                productionPos = productionPos ?? _team.arena.grid.FindEmptyCellAround(pos),
                productUnitParams = productUnitParams
            };
            
            return Result.Ok;
        }

        public Result MoveTo(int2 p) => MoveToImpl(p);
        public Result MoveTo(int x, int y) => MoveToImpl(int2(x, y));
        public Result MoveTo(int[] p) => MoveToImpl(int2(p[0], p[1]));

        private Result MoveToImpl(int2 p)
        {
            if (busy)
                return Result.Busy;

            if (!unitParams.canMove)
                return Result.OperationUnavailable;

            if (any(p < _team.arena.grid.min | _team.arena.grid.max < p))
                return Result.OutOfRange;

            AStar aStar = new()
            {
                from = pos - _team.arena.grid.min,
                to = p - _team.arena.grid.min,
                enoughManhattanDistance = 1,
                grid = _team.arena.grid.nativeArray,
                size = _team.arena.grid.size,
                path = new NativeList<int2>(0, Allocator.TempJob)
            };

            try
            {
                aStar.Run();
                if (aStar.path.Length == 0)
                    return Result.PathBlocked;
                else if (aStar.path.Length == 1)
                    return Result.AlreadyThere;
                if (aStar.path.Length > 1)
                {
                    _moveCommand = new MoveCommand { targetPosition = aStar.path[1] + _team.arena.grid.min };

                    _team.arena.grid[pos]--;
                    _team.arena.grid[_moveCommand.Value.targetPosition]++;
                }
            }
            finally
            {
                aStar.path.Dispose();
            }

            return Result.Ok;
        }

        public void ApplyTickPhaseOne()
        {
            _unitView.FinishLastTick();

            _unitView.speech = _speech;
            _speech = null;

            if (_attackCommand.HasValue)
            {
                _attackCommand.Value.target.integrity -= unitParams.damage;
                
                _unitView.StartAttack(_attackCommand.Value.target._unitView.transform);
                
                _attackCommand = null;
            }

            if (_mineCommand != null)
            {
                DepositMiningOutcome depositMiningOutcome = _mineCommand.Value.deposit.Mine();
                _storage[(int)depositMiningOutcome.substanceType] += depositMiningOutcome.amount;

                _unitView.StartMine(depositMiningOutcome.substanceType, _mineCommand.Value.deposit.pos);

                _mineCommand = null;
            }

            if (_giveCommand != null)
            {
                _storage[(int)_giveCommand.Value.substanceType] -= _giveCommand.Value.amount;
                _giveCommand.Value.target._storage[(int)_giveCommand.Value.substanceType] += _giveCommand.Value.amount;

                _unitView.StartGive(_giveCommand.Value.target._unitView.transform);
            
                _giveCommand = null;
            }
            
            if (_moveCommand != null)
            {
                _unitView.StartMove(_moveCommand.Value.targetPosition);
                
                pos = _moveCommand.Value.targetPosition;

                _moveCommand = null;
            }

            if (_produceCommand != null)
            {
                ProduceCommand produceCommand = _produceCommand.Value;
                
                for (int substanceIndex = 0; substanceIndex < Substance.TYPES_COUNT; substanceIndex++)
                    _storage[substanceIndex] -= produceCommand.productUnitParams.productionCost[substanceIndex];

                // in rare cases when two productions are targeting the same grid cell one of them will be pushed away into a nearby empty cell
                if (_team.arena.grid[produceCommand.productionPos] > 0)
                    produceCommand.productionPos = _team.arena.grid.FindEmptyCellAround(produceCommand.productionPos);
                    
                _team.arena.grid[produceCommand.productionPos]++;

                _unitView.StartProduce(produceCommand.productionPos, produceCommand.ticks);

                _currentProduction = produceCommand;
                _produceCommand = null;
            }

            if (_currentProduction != null)
            {
                ProduceCommand currentProduction = _currentProduction.Value;

                if (--currentProduction.ticks <= 0)
                {
                    _team.arena.grid[currentProduction.productionPos]--;

                    _team.arena.CreateUnit(currentProduction.productUnitParams, _team, currentProduction.productionPos);

                    _currentProduction = null;
                }
                else
                    _currentProduction = currentProduction;
            }
        }

        public void ApplyTickPhaseTwo()
        {
            if (integrity <= 0)
            {
                _team.arena.Unregister(this);

                _team.arena.grid[pos]--;

                if (_currentProduction != null)
                    _team.arena.grid[_currentProduction.Value.productionPos]--;

                _unitView.StartDying();
            }
        }
    }
}
