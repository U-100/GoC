using GoC.ScriptingApi;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace GoC.Arena
{
    [RequireComponent(typeof(Hull))]
    [RequireComponent(typeof(Storage))]
    [RequireComponent(typeof(FactoryControl))]
    public class Factory : MonoBehaviour, IFactory
    {
        [SerializeField] private Team _team;
        [SerializeField] private FactoryConfig _config;
        [Space]
        [SerializeField] private Unit _fabricatedUnitPrototype;
        [SerializeField] private VisualEffect _productionEffect;
        

        private int _ticksLeft;
        private ExposedProperty _productionEffectDurationProperty;

        private Hull _myHull;
        private Hull myHull => _myHull ??= GetComponent<Hull>();
        
        private Storage _myStorage;
        private Storage myStorage => _myStorage ??= GetComponent<Storage>();

        private FactoryControl _myFactoryControl;
        private FactoryControl myFactoryControl => _myFactoryControl ??= GetComponent<FactoryControl>();

        
        public int teamId => _team.teamId;
        public int2 pos => transform.position.ToArenaPos();

        public IHull hull => myHull;
        public IStorage storage => myStorage;
        public IFactoryControl control => _team.arena.executingTeamId == teamId ? myFactoryControl : null;
        

        public Result Produce()
        {
            if (_ticksLeft > 0)
                return Result.Busy;

            for (int substanceIndex = 0; substanceIndex < _config.productionCost.Count; substanceIndex++)
            {
                int cost = _config.productionCost[substanceIndex];
                if (myStorage.content[substanceIndex] < cost)
                    return Result.NotEnoughSubstance;
            }

            _ticksLeft = _config.productionTime;
            return Result.Ok;
        }

        
        public void Setup(Team team, FactoryConfig factoryConfig)
        {
            _team = team;

            _config = factoryConfig;
            
            myHull.Setup(_config.integrity);
            myStorage.Setup(_config.storageCapacityPerSubstance);
        }

        public void BeforeTick()
        {
            if (_ticksLeft > 0 && --_ticksLeft == 0)
                Fabricate();
            
            myHull.BeforeTick();
        }

        public void AfterTick()
        {
            if(_ticksLeft == _config.productionTime)
            {
                for (int substanceIndex = 0; substanceIndex < _config.productionCost.Count; substanceIndex++)
                    myStorage.Withdraw((Substance.Type)substanceIndex, _config.productionCost[substanceIndex]);
                
                _productionEffect.SetFloat(_productionEffectDurationProperty, _config.productionTime * Tick.TIME);
                _productionEffect.Play();
            }

            myHull.AfterTick();
        }


        private void Awake()
        {
            _productionEffectDurationProperty = "duration";
        }
        
        private void OnEnable()
        {
            if (_team != null)
                _team.Register(this);
            else
                Debug.LogWarning("A factory without a team", this);
        }

        private void OnDisable()
        {
            if (_team != null)
                _team.Unregister(this);
        }

        private void Fabricate()
        {
            // NOTE: temporarily disable UnitBehaviour on prefab so we can call Setup() before unit's OnEnable() is triggered
            bool savedPrototypeEnabled = _fabricatedUnitPrototype.enabled;
            _fabricatedUnitPrototype.enabled = false;

            Unit unit = Instantiate(_fabricatedUnitPrototype, transform.position, transform.rotation);
            unit.Setup(_team, _config.product);

            _fabricatedUnitPrototype.enabled = savedPrototypeEnabled;
            unit.enabled = savedPrototypeEnabled;
        }
    }
}
