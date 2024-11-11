using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

using static Unity.Mathematics.math;

namespace GoC
{
    public class Unit : MonoBehaviour, IMyUnit
    {
        [SerializeField] private VisualEffect _exhaust;
        
        private Vector3? _targetPosition;
        private Quaternion? _targetRotation;

        public int id => GetInstanceID();
        public int2 pos => (int2)round(float2(transform.position.x, transform.position.z));

        public void Move()
        {
            Settle();
            _targetPosition = transform.position + transform.forward;
            _exhaust.Play();
        }

        public void TurnLeft()
        {
            Settle();
            _targetRotation = Quaternion.Euler(0, -90, 0) * transform.rotation;
        }

        public void TurnRight()
        {
            Settle();
            _targetRotation = Quaternion.Euler(0, 90, 0) * transform.rotation;
        }

        void Start()
        {
            _exhaust.Stop();
        }

        void Update()
        {
            if(_targetPosition.HasValue)
            {
                float linearSpeed = 1 / Tick.TIME; // units per tick
                transform.position = Vector3.MoveTowards(transform.position, _targetPosition.Value, linearSpeed * Time.deltaTime);
            }

            if(_targetRotation.HasValue)
            {
                float rotationSpeed = 90 / Tick.TIME; // degrees per tick
                transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation.Value, rotationSpeed * Time.deltaTime);
            }
        }
        
        private void Settle()
        {
            if (_targetPosition.HasValue)
            {
                // eliminate rounding errors in position
                transform.position = new Vector3(Mathf.Round(_targetPosition.Value.x), Mathf.Round(_targetPosition.Value.y), Mathf.Round(_targetPosition.Value.z));
                _targetPosition = null;
            }

            if (_targetRotation.HasValue)
            {
                // eliminate rounding errors in rotation
                transform.rotation = Quaternion.Euler(0, Mathf.Round(_targetRotation.Value.eulerAngles.y / 90) * 90, 0);
                _targetRotation = null;
            }

            // stop engine emission
            _exhaust.Stop();
        }
    }
}
