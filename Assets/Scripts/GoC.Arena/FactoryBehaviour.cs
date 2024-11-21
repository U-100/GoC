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

        public void Setup(Team team)
        {
            _team = team;
        }

        public int id => GetInstanceID();
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

        private void Fabricate()
        {
            UnitBehaviour unitBehaviour = Instantiate(_fabricatedUnitPrototype, transform.position, transform.rotation);
            unitBehaviour.Setup(_team);
        }
    }
}
