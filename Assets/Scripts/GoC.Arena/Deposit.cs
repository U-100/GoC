using GoC.ScriptingApi;
using Unity.Mathematics;
using UnityEngine;

namespace GoC.Arena
{
    public struct DepositMiningOutcome
    {
        public Substance.Type substanceType;
        public int amount;
    }

    public class Deposit : MonoBehaviour, IDeposit
    {
        [SerializeField] private Arena _arena;
        [SerializeField] private DepositConfig _config;


        public int2 pos => transform.position.ToArenaPos();
        public Substance.Type substanceType => _config.substanceType;
        public int miningSpeed => _config.miningSpeed;

        
        public void Setup(Arena initArena, DepositConfig initConfig)
        {
            _config = initConfig;
            _arena = initArena;
            _arena.Register(this);
        }
        
        public DepositMiningOutcome Mine() => new DepositMiningOutcome { substanceType = substanceType, amount = miningSpeed };


        void OnEnable() { }

        void OnDisable()
        {
            if(_arena != null)
                _arena.Unregister(this);
        }
    }
}
