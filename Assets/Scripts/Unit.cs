using Unity.Mathematics;
using UnityEngine;

namespace GoC
{
    public class Unit : MonoBehaviour, IUnit
    {
        private Vector3 _targetPosition;
        private Quaternion _targetRotation;

        void Start()
        {
            _targetPosition = transform.position;
            _targetRotation = transform.rotation;
        }

        void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, 1 * Time.deltaTime);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, 90 * Time.deltaTime);
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
        }
        
        public void Move()
        {
            Settle();
            _targetPosition = transform.position + transform.forward;
        }

        public void TurnLeft()
        {
            Settle();
            _targetRotation = quaternion.RotateY(90 * Mathf.Deg2Rad) * transform.rotation;
        }

        public void TurnRight()
        {
            Settle();
            _targetRotation = quaternion.RotateY(-90 * Mathf.Deg2Rad) * transform.rotation;
        }
    }
}
