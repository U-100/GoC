using UnityEngine;
using UnityEngine.VFX;

namespace GoC
{
    public delegate void AddUnit(Unit unit);

    public class Factory : FieldObjectBehaviour, IFactory
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
        private Control _control;
        
        private float _cooldown;

        public void Init(int initTeamId, IExecutionContext executionContext, AddUnit addUnit)
        {
            base.Init(initTeamId, executionContext);
            _addUnit = addUnit;
            _control = new Control(this);
        }

        public IFactoryControl control => CheckExecutionRestrictions() ? _control : null;
        
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
