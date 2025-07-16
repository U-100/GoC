using GoC.ScriptingApi;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

using static Unity.Mathematics.math;

namespace GoC.Arena
{
    public class Unit : MonoBehaviour
    {
        [Space]
        [SerializeField] private VisualEffect _attackEffect;
        [SerializeField] private VisualEffect _mineEffect;
        [SerializeField] private VisualEffect _giveEffect;
        [SerializeField] private VisualEffect _productionEffect;
        [SerializeField] private VisualEffect _moveEffect;
        [SerializeField] private VisualEffect _dyingEffect;
        [SerializeField] private GameObject _destructionEffectPrefab;
        
        [Space]
        [SerializeField] private Renderer _skinColorSubjectRenderer;

        [Space]
        [SerializeField] private UIDocument _speechBubble;

        private ExposedProperty _attackEffectColorProperty;
        private ExposedProperty _attackEffectDistanceProperty;
        private ExposedProperty _mineEffectTexIndexProperty;
        private ExposedProperty _mineEffectSourcePositionProperty;
        private ExposedProperty _productionEffectDurationProperty;
        private ExposedProperty _productionEffectSpawnPositionProperty;
        private ExposedProperty _productionEffectColorProperty;

        [CreateProperty] public string speech { get; set; }
        [CreateProperty] public bool speaking => speech != null;

        private int2? _moveTargetPos;
        private int2? _rotateTargetPos;
        private Transform _rotateTargetTransform;

        // to set up upon instantiation
        public void Setup(TeamSkin teamSkin)
        {
            _skinColorSubjectRenderer.material = teamSkin.mainColorMaterial;
            _attackEffect.SetVector4(_attackEffectColorProperty, teamSkin.vfxColorTint);
            _productionEffect.SetVector4(_productionEffectColorProperty, teamSkin.vfxColorTint);
        }

        public void FinishLastTick()
        {
            speech = null;
            _attackEffect.Stop();
            _mineEffect.Stop();
            _giveEffect.Stop();
            _moveEffect.Stop();

            if (_moveTargetPos != null)
            {
                transform.position = _moveTargetPos.Value.ToUnityPos();
                _moveTargetPos = null;
            }
        }

        public void StartAttack(Transform targetTransform)
        {
            _rotateTargetTransform = targetTransform;
            
            float distance = Vector3.Distance(transform.position, targetTransform.position);
            _attackEffect.SetFloat(_attackEffectDistanceProperty, distance);
            _attackEffect.Play();
        }

        public void StartMine(Substance.Type substanceType, int2 depositPos)
        {
            _rotateTargetPos = depositPos;
            
            _mineEffect.SetUInt(_mineEffectTexIndexProperty, (uint)substanceType);
            _mineEffect.SetVector3(_mineEffectSourcePositionProperty, depositPos.ToUnityPos());
            _mineEffect.Play();
        }

        public void StartGive(Transform targetTransform)
        {
            _rotateTargetTransform = targetTransform;
            
            _giveEffect.Play();
        }

        public void StartMove(int2 targetPos)
        {
            _moveTargetPos = targetPos;
        }

        public void StartProduce(int2 productionPos, int ticksLeft)
        {
            _rotateTargetPos = productionPos;
            
            _productionEffect.SetVector3(_productionEffectSpawnPositionProperty, productionPos.ToUnityPos());
            _productionEffect.SetFloat(_productionEffectDurationProperty, ticksLeft * Tick.TIME);
            _productionEffect.Play();
        }

        public void StartDying()
        {
            _dyingEffect.enabled = true;
        }

        public void StartDestruction()
        {
            GameObject destructionEffect = Instantiate(_destructionEffectPrefab, transform.position, transform.rotation);
            Destroy(destructionEffect, 1);
        }
        
        private void Awake()
        {
            _attackEffectColorProperty = "color";
            _attackEffectDistanceProperty = "distance";
            _mineEffectTexIndexProperty = "texIndex";
            _mineEffectSourcePositionProperty = "sourcePosition_position";
            _productionEffectSpawnPositionProperty = "spawnPosition_position";
            _productionEffectDurationProperty = "duration";
            _productionEffectColorProperty = "color";
            
            _speechBubble.rootVisualElement.dataSource = this;
        }

        private void OnEnable()
        {
            _dyingEffect.enabled = false;
        }

        private void Update()
        {
            if (_rotateTargetTransform != null)
                RotateTowards(_rotateTargetTransform.position);

            if (_moveTargetPos != null)
            {
                Vector3 targetPosition = _moveTargetPos.Value.ToUnityPos();
                if (distance(transform.position, targetPosition) > Mathf.Epsilon)
                {
                    if (RotateTowards(targetPosition))
                    {
                        float linearSpeed = 2 / Tick.TIME; // units per tick
                        transform.position = Vector3.MoveTowards(transform.position, targetPosition, linearSpeed * Time.deltaTime);
                        _moveEffect.Play();
                    }
                    else
                        _moveEffect.Stop();
                }
            }
        }

        private bool RotateTowards(Vector3 position)
        {
            if (Vector3.Distance(position, transform.position) > 0.5f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(position - transform.position, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360 * Time.deltaTime);

                return Quaternion.Angle(transform.rotation, targetRotation) < 10;
            }

            return true;
        }
    }
}
