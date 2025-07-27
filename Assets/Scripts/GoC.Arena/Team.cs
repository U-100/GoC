using System;
using System.Collections.Generic;
using GoC.ScriptingApi;

namespace GoC.Arena
{
    public class Team : IModel, IDisposable
    {
        private readonly Arena _arena;
        private readonly TeamConfig _config;

        private readonly Dictionary<string, IUnitConfig> _myUnitConfigs = new();
        
        private readonly List<ExposedUnitControl> _myUnits = new(); 
        private readonly List<ExposedUnit> _enemyUnits = new();

        private readonly List<ExposedUnitControl> _myUnitsCache = new();

        public Arena arena => _arena;
        public TeamSkin skin => _config.teamSkin;
        
        public int teamId { get; }

        public UnitConfig starterUnitConfig => _config.starterUnitConfig;
        
        public IReadOnlyDictionary<string, IUnitConfig> myUnitConfigs => _myUnitConfigs;

        public IReadOnlyList<IUnitControl> myUnits => _myUnits;
        public IReadOnlyList<IUnit> enemyUnits => _enemyUnits;
        
        public IReadOnlyList<IDeposit> deposits => _arena.deposits;


        public Team(int teamId, Arena arena, TeamConfig config)
        {
            this.teamId = teamId;
            
            _arena = arena;
            _config = config;
            
            RecursiveFillMyUnitConfigs(_config.starterUnitConfig);
        }

        private void RecursiveFillMyUnitConfigs(IUnitConfig unitConfig)
        {
            _myUnitConfigs.Add(unitConfig.name, unitConfig);
            
            //UnityEngine.Debug.Log($"Added {unitConfig.name} to team {teamId} myUnitConfigs");

            foreach (IUnitConfig productConfig in unitConfig.productConfigs)
                if (!_myUnitConfigs.ContainsKey(productConfig.name))
                    RecursiveFillMyUnitConfigs(productConfig);
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

        public void ApplyTickPhaseOne()
        {
            try
            {
                _myUnitsCache.AddRange(_myUnits);
                foreach (ExposedUnitControl myUnit in _myUnitsCache)
                    myUnit.ApplyTickPhaseOne();
            }
            finally
            {
                _myUnitsCache.Clear();
            }
        }

        public void ApplyTickPhaseTwo()
        {
            try
            {
                _myUnitsCache.AddRange(_myUnits);
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
