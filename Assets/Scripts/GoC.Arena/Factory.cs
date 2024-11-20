using GoC.ScriptingApi;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

namespace GoC.Arena
{
    public delegate void AddUnit(Unit unit);

    public class Factory : MonoBehaviour, IFactory
    {
        private class Control : IFactoryControl
        {
            private readonly Factory _factory;
            public Control(Factory factory) => _factory = factory;

            public Result Produce() => _factory.Produce();
        }
        
        [SerializeField] private Unit _unit;
        [SerializeField] private VisualEffect _spawn;

        private AddUnit _addUnit;
        private RestrictedControl<IFactoryControl> _restrictedControl;
        
        private float _cooldown;

        public void Init(IArenaContext arenaContext, int initTeamId, AddUnit addUnit)
        {
            _addUnit = addUnit;
            _restrictedControl = new RestrictedControl<IFactoryControl>(arenaContext, initTeamId, new Control(this));
        }

        public int id => GetInstanceID();
        public int teamId => _restrictedControl.teamId;
        public int2 pos => transform.position.ToArenaPos();
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
            Unit unit = Instantiate(_unit, transform.position, transform.rotation);
            _addUnit(unit);
        }
    }
}
