using System.Collections.Generic;
using GoC.ScriptingApi;
using UnityEngine;

namespace GoC.Arena
{
    public class Team : MonoBehaviour, IModel
    {
        [SerializeField] private Arena _arena;
        [SerializeField] private string _scriptFile;

        private readonly HashSet<Unit> _myUnits = new();
        private readonly List<Unit> _myUnitsCache = new();
        
        private readonly HashSet<Factory> _myFactories = new();
        private readonly List<Factory> _myFactoriesCache = new();
        
        public Arena arena => _arena;
        public string scriptFile => _scriptFile;

        
        public int teamId => GetInstanceID();

        public IEnumerable<IFactory> myFactories => _myFactories;
        public IEnumerable<IUnit> myUnits => _myUnits;

        public IEnumerable<IFactory> factories => _arena.factories;
        public IEnumerable<IUnit> units => _arena.units;
        public IEnumerable<IDeposit> deposits => _arena.deposits;


        public void Setup(Arena initArena)
        {
            _arena = initArena;
            _arena.Register(this);
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
        
        
        private void OnEnable() { }

        private void OnDisable()
        {
            if(_arena != null)
                _arena.Unregister(this);
        }
    }
}
