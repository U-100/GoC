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
        private readonly HashSet<Factory> _myFactories = new();

        public void Register(Unit unit) => _myUnits.Add(unit);
        public void Unregister(Unit unit) => _myUnits.Remove(unit);

        public void Register(Factory factory) => _myFactories.Add(factory);
        public void Unregister(Factory factory) => _myFactories.Remove(factory);

        public IArenaContext arena => _arena;
        public string scriptFile => _scriptFile;

        public int teamId => GetInstanceID();

        public IEnumerable<IFactory> myFactories => _myFactories;
        public IEnumerable<IUnit> myUnits => _myUnits;

        public IEnumerable<IFactory> factories => _arena.factories;
        public IEnumerable<IUnit> units => _arena.units;
        public IEnumerable<IDeposit> deposits => _arena.deposits;

        public void BeforeTick()
        {
            foreach (Unit unit in _myUnits)
                unit.BeforeTick();

            foreach (Factory factory in _myFactories)
                factory.BeforeTick();
        }

        public void AfterTick()
        {
            foreach (Unit unit in _myUnits)
                unit.AfterTick();

            foreach (Factory factory in _myFactories)
                factory.AfterTick();
        }
        
        void OnEnable()
        {
            if(_arena != null)
                _arena.Register(this);
            else
                Debug.LogWarning("A team without arena", this);
        }

        void OnDisable()
        {
            if(_arena != null)
                _arena.Unregister(this);
        }
    }
}
