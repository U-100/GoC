using UnityEngine;

namespace GoC.Arena
{
    public class CameraTargetController : MonoBehaviour
    {
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private Transform _cameraTargetTransform;
        [SerializeField] private float _cameraElevationFactor = 0.1f;
        
        private ArenaControls _arenaControls;

        void Awake()
        {
            _arenaControls = new ArenaControls();
            _arenaControls.Enable();
        }

        void OnDestroy()
        {
            _arenaControls.Disable();
            _arenaControls.Dispose();
        }

        void Update()
        {
            Vector3 forward = _cameraTransform.forward;
            forward.y = 0;
            forward.Normalize();
            
            Vector3 right = _cameraTransform.right;
            right.y = 0;
            right.Normalize();
            
            Vector2 move = _arenaControls.Arena.Move.ReadValue<Vector2>();
            
            // scale by camera elevation
            move *= _cameraTransform.position.y * _cameraElevationFactor;
            
            _cameraTargetTransform.position += move.x * right + move.y * forward;
        }
    }
}
