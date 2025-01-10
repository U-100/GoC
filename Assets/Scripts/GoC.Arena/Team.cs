using System;
using System.Collections.Generic;
using GoC.ScriptingApi;

namespace GoC.Arena
{
    public class Team : IModel, IDisposable
    {
        private Arena _arena;
        private TeamConfig _config;

        private readonly List<ExposedUnitControl> _myUnits = new(); 
        private readonly List<ExposedUnit> _enemyUnits = new();

        private readonly List<ExposedUnitControl> _myUnitsCache = new();
        
        public Arena arena => _arena;
        public TeamSkin skin => _config.teamSkin;

        
        public int teamId { get; }

        public IReadOnlyList<IUnitControl> myUnits => _myUnits;
        public IReadOnlyList<IUnit> units => _arena.units;
        public IReadOnlyList<IUnit> enemyUnits => _enemyUnits;
        public IReadOnlyList<IDeposit> deposits => _arena.deposits;


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
                    if (myUnit.unit != null)
                        UnityEngine.Object.Destroy(myUnit.unit.gameObject);
            }
            finally
            {
                _myUnitsCache.Clear();
            }
        }

        public void BeforeTick()
        {
            try
            {
                _myUnitsCache.AddRange(_myUnits);
                foreach (ExposedUnitControl myUnit in _myUnitsCache)
                    myUnit.unit.BeforeTick();
            }
            finally
            {
                _myUnitsCache.Clear();
            }
        }

        public void AfterTick()
        {
            try
            {
                _myUnitsCache.AddRange(_myUnits);
                foreach (ExposedUnitControl myUnit in _myUnitsCache)
                    myUnit.unit.AfterTick();
            }
            finally
            {
                _myUnitsCache.Clear();
            }
        }

        public void AddMyUnit(Unit unit)
        {
            _myUnits.Add(new ExposedUnitControl(unit));
        }

        public void AddEnemyUnit(Unit unit)
        {
            _enemyUnits.Add(new ExposedUnit(unit));
        }
        
        public void RemoveMyUnit(Unit unit)
        {
            _myUnits.RemoveAll(exposedUnitControl => exposedUnitControl.unit == unit);
        }

        public void RemoveEnemyUnit(Unit unit)
        {
            _enemyUnits.RemoveAll(exposedUnitControl => exposedUnitControl.unit == unit);
        }
    }
}
