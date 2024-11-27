using GoC.ScriptingApi;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

namespace GoC.Arena
{
    [RequireComponent(typeof(Hull))]
    [RequireComponent(typeof(Storage))]
    [RequireComponent(typeof(FactoryControl))]
    public class Factory : MonoBehaviour, IFactory
    {
        [SerializeField] private Team _team;
        [SerializeField] private Unit _fabricatedUnitPrototype;
        [Space]
        [SerializeField] private VisualEffect _spawn;

        private float _cooldown;

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
            if (_cooldown > 0)
                return Result.Busy;

            if (myStorage.Withdraw(Substance.Type.Energy, 1))
            {
                _cooldown = Tick.TIME - Time.deltaTime; // slightly less than a tick
                _spawn.Play();
                return Result.Ok;
            }
            
            return Result.NotEnoughSubstance;
        }

        
        public void Setup(Team team) => _team = team;

        public void BeforeTick()
        {
            myHull.BeforeTick();
        }

        public void AfterTick()
        {
            myHull.AfterTick();
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

        private void Update()
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

            Unit unit = Instantiate(_fabricatedUnitPrototype, transform.position, transform.rotation);
            unit.Setup(_team);

            _fabricatedUnitPrototype.enabled = savedPrototypeEnabled;
            unit.enabled = savedPrototypeEnabled;
        }
    }
}
