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
        
        private readonly List<Factory> _myFactories = new();
        private readonly List<Factory> _myFactoriesCache = new();

        public Arena arena => _arena;
        public string scriptFile => _config.scriptFile;
        public TeamSkin skin => _config.teamSkin;

        
        public int teamId => GetInstanceID();

        public IReadOnlyList<IFactory> myFactories => _myFactories;
        public IReadOnlyList<IUnit> myUnits => _myUnits;

        public IEnumerable<IFactory> factories => _arena.factories;
        public IEnumerable<IUnit> units => _arena.units;
        public IEnumerable<IFactory> enemyFactories => _arena.GetEnemyFactories(this);
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

                _myFactoriesCache.AddRange(_myFactories);
                foreach (Factory factory in _myFactoriesCache)
                    factory.BeforeTick();
            }
            finally
            {
                _myUnitsCache.Clear();
                _myFactoriesCache.Clear();
            }
        }

        public void AfterTick()
        {
            try
            {
                _myUnitsCache.AddRange(_myUnits);
                foreach (Unit unit in _myUnitsCache)
                    unit.AfterTick();

                _myFactoriesCache.AddRange(_myFactories);
                foreach (Factory factory in _myFactoriesCache)
                    factory.AfterTick();
            }
            finally
            {
                _myUnitsCache.Clear();
                _myFactoriesCache.Clear();
            }
        }

        public void Register(Unit unit) => _myUnits.Add(unit);
        public void Unregister(Unit unit) => _myUnits.Remove(unit);

        public void Register(Factory factory) => _myFactories.Add(factory);
        public void Unregister(Factory factory) => _myFactories.Remove(factory);
        
        
        private void OnDisable()
        {
            if(_arena != null)
                _arena.Unregister(this);
        }
    }
}
