using System;
using System.Collections.Generic;
using GoC.ScriptingApi;

namespace GoC.Arena
{
    public class Team : IModel, IDisposable
    {
        private readonly Arena _arena;
        private readonly TeamConfig _config;

        private readonly Dictionary<string, IUnitParams> _myUnitParams = new();
        
        private readonly List<ExposedUnitControl> _myUnits = new(); 
        private readonly List<ExposedUnit> _enemyUnits = new();

        private readonly List<ExposedUnitControl> _myUnitsCache = new();

        public Arena arena => _arena;
        public TeamSkin skin => _config.teamSkin;
        
        public int teamId { get; }

        public IUnitParams starterUnitParams => _config.starterUnitParams;
        
        public IReadOnlyDictionary<string, IUnitParams> myUnitParams => _myUnitParams;

        public IReadOnlyList<IUnitControl> myUnits => _myUnits;
        public IReadOnlyList<IUnit> enemyUnits => _enemyUnits;
        
        public IReadOnlyList<IDeposit> deposits => _arena.deposits;


        public Team(int teamId, Arena arena, TeamConfig config)
        {
            this.teamId = teamId;
            
            _arena = arena;
            _config = config;
            
            RecursiveFillMyUnitParams(_config.starterUnitParams);
        }

        private void RecursiveFillMyUnitParams(IUnitParams unitParams)
        {
            _myUnitParams.Add(unitParams.name, unitParams);

            foreach (IUnitParams productUnitParams in unitParams.products)
                if (!_myUnitParams.ContainsKey(productUnitParams.name))
                    RecursiveFillMyUnitParams(productUnitParams);
        }
        
        public void Dispose()
        {
            try
            {
                _myUnitsCache.AddRange(_myUnits);
                foreach (ExposedUnitControl myUnit in _myUnitsCache)
                    myUnit.Dispose();
            }
            finally
            {
                _myUnitsCache.Clear();
            }
        }

        public void ApplyTick()
        {
            try
            {
                _myUnitsCache.AddRange(_myUnits);
                foreach (ExposedUnitControl myUnit in _myUnitsCache)
                    myUnit.ApplyTickPhaseOne();
                foreach (ExposedUnitControl myUnit in _myUnitsCache)
                    myUnit.ApplyTickPhaseTwo();
            }
            finally
            {
                _myUnitsCache.Clear();
            }
        }

        public void AddMyUnit(ExposedUnitControl exposedUnitControl) => _myUnits.Add(exposedUnitControl);

        public void AddEnemyUnit(ExposedUnitControl exposedUnitControl) => _enemyUnits.Add(new ExposedUnit(exposedUnitControl));

        public void RemoveMyUnit(ExposedUnitControl exposedUnitControl) => _myUnits.Remove(exposedUnitControl);

        public void RemoveEnemyUnit(ExposedUnitControl exposedUnitControl) => _enemyUnits.RemoveAll(exposedUnit => exposedUnit.exposedUnitControl == exposedUnitControl);
    }
}
