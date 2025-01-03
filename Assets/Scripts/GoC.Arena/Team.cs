using System.Collections.Generic;
using GoC.ScriptingApi;
using UnityEngine;

namespace GoC.Arena
{
    public class Team : MonoBehaviour, IModel
    {
        [SerializeField] private Arena _arena;
        [SerializeField] private TeamConfig _config;

        private readonly List<Unit> _myUnits = new();
        private readonly List<Unit> _myUnitsCache = new();
        
        public Arena arena => _arena;
        public string scriptFile => _config.scriptFile;
        public TeamSkin skin => _config.teamSkin;

        
        public int teamId => GetInstanceID();

        public IReadOnlyList<IUnit> myUnits => _myUnits;
        public IEnumerable<IUnit> units => _arena.units;
        public IEnumerable<IUnit> enemyUnits => _arena.GetEnemyUnits(this);
        public IReadOnlyList<IDeposit> deposits => _arena.deposits;


        public void Setup(Arena initArena, TeamConfig initConfig)
        {
            _arena = initArena;
            _config = initConfig;
        }

        public void BeforeTick()
        {
            try
            {
                _myUnitsCache.AddRange(_myUnits);
                foreach (Unit unit in _myUnitsCache)
                    unit.BeforeTick();
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
                foreach (Unit unit in _myUnitsCache)
                    unit.AfterTick();
            }
            finally
            {
                _myUnitsCache.Clear();
            }
        }

        public void Register(Unit unit) => _myUnits.Add(unit);
        public void Unregister(Unit unit) => _myUnits.Remove(unit);
        
        
        private void OnDestroy()
        {
            if(_arena != null)
                _arena.Unregister(this);

            try
            {
                _myUnitsCache.AddRange(_myUnits);
                foreach (Unit myUnit in _myUnits)
                    Destroy(myUnit.gameObject);
            }
            finally
            {
                _myUnitsCache.Clear();
            }
        }
    }
}
