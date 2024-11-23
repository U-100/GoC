using System.Collections.Generic;
using GoC.ScriptingApi;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

namespace GoC.Arena
{
    [RequireComponent(typeof(StorageBehaviour))]
    [RequireComponent(typeof(FactoryControlBehaviour))]
    public class FactoryBehaviour : MonoBehaviour, IFactory
    {
        [SerializeField] private Team _team;
        [SerializeField] private UnitBehaviour _fabricatedUnitPrototype;
        [Space]
        [SerializeField] private VisualEffect _spawn;

        private float _cooldown;

        private IStorageBehaviour _storageBehaviour;
        private IStorageBehaviour storageBehaviour => _storageBehaviour ??= GetComponent<IStorageBehaviour>();

        public int teamId => _team.teamId;
        public int2 pos => transform.position.ToArenaPos();
        
        public IReadOnlyList<int> storage => storageBehaviour.content;
        
        private FactoryControlBehaviour _factoryControlBehaviour;
        public IFactoryControl control =>
            _team.arena.executingTeamId == teamId
                ? _factoryControlBehaviour ??= GetComponent<FactoryControlBehaviour>()
                : null;

        public Result Produce()
        {
            if (_cooldown > 0)
                return Result.Busy;

            if (storageBehaviour.Withdraw(Substance.Type.Energy, 1))
            {
                _cooldown = Tick.TIME - Time.deltaTime; // slightly less than a tick
                _spawn.Play();
                return Result.Ok;
            }
            
            return Result.NotEnoughSubstance;
        }

        public void Setup(Team team) => _team = team;
        
        public void BeforeTick() { }

        public void AfterTick() { }

        void OnEnable()
        {
            if (_team != null)
                _team.Register(this);
            else
                Debug.LogWarning("A factory without a team", this);
        }

        void OnDisable()
        {
            if (_team != null)
                _team.Unregister(this);
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
            // NOTE: temporarily disable UnitBehaviour on prefab so we can call Setup() before unit's OnEnable() is triggered
            bool savedPrototypeEnabled = _fabricatedUnitPrototype.enabled;
            _fabricatedUnitPrototype.enabled = false;

            UnitBehaviour unitBehaviour = Instantiate(_fabricatedUnitPrototype, transform.position, transform.rotation);
            unitBehaviour.Setup(_team);

            _fabricatedUnitPrototype.enabled = savedPrototypeEnabled;
            unitBehaviour.enabled = savedPrototypeEnabled;
        }
    }
}
