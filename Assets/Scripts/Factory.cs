using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

using static Unity.Mathematics.math;

namespace GoC
{
    public delegate void AddUnit(Unit unit);
    
    public class Factory : MonoBehaviour, IMyFactory
    {
        [SerializeField] private ExecutionContext _executionContext;
        [SerializeField] private Unit _unit;
        [SerializeField] private VisualEffect _spawn;

        private AddUnit _addUnit;
        private float _cooldown;

        public void Init(int initTeamId, AddUnit addUnit)
        {
            teamId = initTeamId;
            _addUnit = addUnit;
        }

        public int id => GetInstanceID();
        public object data { get; set; }

        public int2 pos => (int2)round(float2(transform.position.x, transform.position.z));

        public int teamId { get; private set; }
        
        public Result Produce()
        {
            if (!CheckExecutionRestrictions())
                return Result.AccessDenied;
            
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

        private bool CheckExecutionRestrictions() => _executionContext.teamId == teamId;

        private void Fabricate()
        {
            Unit unit = Instantiate(_unit, transform.position, transform.rotation);
            _addUnit(unit);
        } 
    }
}
