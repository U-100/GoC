using GoC.ScriptingApi;
using UnityEngine;
using UnityEngine.VFX;

namespace GoC.Arena
{
    public class Hull : MonoBehaviour, IHull
    {
        public enum State
        {
            Intact,
            Destroyed
        }
        
        [SerializeField] private int _integrity;
        [Space]
        [SerializeField] private VisualEffect _dyingEffect;
        [SerializeField] private GameObject _destructionEffectPrefab;
        

        public int integrity => _integrity;


        public void Setup(int initialIntegrity) => _integrity = initialIntegrity;
        
        public void Absorb(int damage) => _integrity -= damage;
        
        public State BeforeTick()
        {
            if (_integrity <= 0)
            {
                GameObject destructionEffect = Instantiate(_destructionEffectPrefab, transform.position, transform.rotation);
                Destroy(destructionEffect, 1);
                return State.Destroyed;
            }

            return State.Intact;
        }

        public void AfterTick() { }

        
        private void OnEnable()
        {
            _dyingEffect.enabled = false;
        }

        private void Update()
        {
            if (_integrity <= 0)
                _dyingEffect.enabled = true;
        }
    }
}
