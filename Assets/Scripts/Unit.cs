using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

using static Unity.Mathematics.math;

namespace GoC
{
    public class Unit : MonoBehaviour, IMyUnit
    {
        [SerializeField] private VisualEffect _exhaust;
        
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;

        void Start()
        {
            _targetPosition = transform.position;
            _targetRotation = transform.rotation;

            _exhaust.Stop();
        }

        void Update()
        {
            float linearSpeed = 1 / Tick.TIME; // units per tick
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, linearSpeed * Time.deltaTime);

            float rotationSpeed = 90 / Tick.TIME; // degrees per tick
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        private void Settle()
        {
            // eliminate rounding errors in position
            _targetPosition.x = Mathf.Round(_targetPosition.x);
            _targetPosition.y = Mathf.Round(_targetPosition.y);
            _targetPosition.z = Mathf.Round(_targetPosition.z);

            // eliminate rounding errors in rotation
            _targetRotation = Quaternion.Euler(0, Mathf.Round(_targetRotation.eulerAngles.y / 90) * 90, 0);
            
            // settle transform
            transform.SetPositionAndRotation(_targetPosition, _targetRotation);

            // stop engine emission
            _exhaust.Stop();
        }

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
            _targetRotation = Quaternion.Euler(0, 90, 0) * transform.rotation;
        }

        public void TurnRight()
        {
            Settle();
            _targetRotation = Quaternion.Euler(0, -90, 0) * transform.rotation;
        }
    }
}
