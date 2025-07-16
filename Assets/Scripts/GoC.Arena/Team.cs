using System;
using System.Collections.Generic;
using GoC.ScriptingApi;

namespace GoC.Arena
{
    public class Team : IModel, IDisposable
    {
        private readonly Arena _arena;
        private readonly TeamConfig _config;

        private readonly List<ExposedUnitControl> _myUnits = new(); 
        private readonly List<ExposedUnit> _enemyUnits = new();

        private readonly List<ExposedUnitControl> _myUnitsCache = new();
        
        public Arena arena => _arena;
        public TeamSkin skin => _config.teamSkin;

        
        public int teamId { get; }

        public IReadOnlyList<IUnitControl> myUnits => _myUnits;
        public IReadOnlyList<IUnit> enemyUnits => _enemyUnits;
        public IReadOnlyList<IDeposit> deposits => _arena.deposits;

        public int ProductionCost(object unitParamsObject, Substance.Type substanceType) => arena.config.unitConfig.ProductionCost(UnitParams.FromObject(unitParamsObject), (int)substanceType);

        public int ProductionTicks(object unitParamsObject) => arena.config.unitConfig.ProductionTicks(UnitParams.FromObject(unitParamsObject));


        public Team(int initTeamId, Arena initArena, TeamConfig initConfig)
        {
            teamId = initTeamId;
            _arena = initArena;
            _config = initConfig;
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
                    myUnit.ApplyTick();
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
