using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace GoC
{
    public delegate void AddUnit(Unit unit);
    
    public class Factory : MonoBehaviour, IMyFactory
    {
        [SerializeField] private Unit _unit; 

        private AddUnit _addUnit;
        private float _cooldown;

        public void Init(AddUnit addUnit) => _addUnit = addUnit;

        public int id => GetInstanceID();
        public int2 pos => (int2)round(float2(transform.position.x, transform.position.z));
        
        public Result Produce()
        {
            if (_cooldown > 0)
                return Result.Busy;
            
            _cooldown = Tick.TIME - Time.deltaTime; // slightly less than a tick
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
