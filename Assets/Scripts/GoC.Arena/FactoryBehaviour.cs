using System.Collections.Generic;
using GoC.ScriptingApi;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

namespace GoC.Arena
{
    public delegate void AddUnit(UnitBehaviour unitBehaviour);

    [RequireComponent(typeof(StorageBehaviour))]
    public class FactoryBehaviour : MonoBehaviour, IFactory
    {
        private class Control : IFactoryControl
        {
            private readonly FactoryBehaviour _factoryBehaviour;
            public Control(FactoryBehaviour factoryBehaviour) => _factoryBehaviour = factoryBehaviour;

            public Result Produce() => _factoryBehaviour.Produce();
        }
        
        [SerializeField] private UnitBehaviour _prototype;
        [SerializeField] private VisualEffect _spawn;

        private AddUnit _addUnit;
        private float _cooldown;
        private RestrictedControl<IFactoryControl> _restrictedControl;

        private IStorageBehaviour _storageBehaviour;
        private IStorageBehaviour storageBehaviour => _storageBehaviour ??= GetComponent<IStorageBehaviour>();

        public void Init(IArenaContext arenaContext, int initTeamId, AddUnit addUnit)
        {
            _addUnit = addUnit;
            _restrictedControl = new RestrictedControl<IFactoryControl>(arenaContext, initTeamId, new Control(this));
        }

        public int id => GetInstanceID();
        public int teamId => _restrictedControl.teamId;
        public int2 pos => transform.position.ToArenaPos();
        
        public IReadOnlyList<int> storage => storageBehaviour.content;
        public IFactoryControl control => _restrictedControl.control;
        
        private Result Produce()
        {
            if (_cooldown > 0)
                return Result.Busy;
            
            _cooldown = Tick.TIME - Time.deltaTime; // slightly less than a tick
            
            _spawn.Play();
            
            return Result.Ok;
        }
        
        void Update()
        {
            if (_cooldown > 0 && (_cooldown -= Time.deltaTime) <= 0)
            {
                _cooldown = 0;
                Fabricate();
            }
        }

        private void Fabricate()
        {
            UnitBehaviour unitBehaviour = Instantiate(_prototype, transform.position, transform.rotation);
            _addUnit(unitBehaviour);
        }
    }
}
