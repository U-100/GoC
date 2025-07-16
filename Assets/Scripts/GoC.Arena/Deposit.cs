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

        private int2 _pos;

        public int2 pos => _pos;
        public Substance.Type substanceType => _config.substanceType;
        public int miningSpeed => _config.miningSpeed;

        
        public void Setup(Arena initArena, DepositConfig initConfig, int2 initPos)
        {
            _arena = initArena;
            _config = initConfig;
            _pos = initPos;
            
            transform.position = pos.ToUnityPos();
        }
        
        public DepositMiningOutcome Mine() => new DepositMiningOutcome { substanceType = substanceType, amount = miningSpeed };


        void OnDisable()
        {
            if(_arena != null)
                _arena.Unregister(this);
        }
    }
}
