using GoC.ScriptingApi;
using Unity.Mathematics;
using UnityEngine;

namespace GoC.Arena
{
    public struct DepositExtractionOutcome
    {
        public Substance.Type substanceType;
        public int amount;
    }

    public class Deposit : MonoBehaviour, IDeposit
    {
        [SerializeField] private Arena _arena;

        public int2 pos => transform.position.ToArenaPos();

        public DepositExtractionOutcome Extract() => new DepositExtractionOutcome { substanceType = Substance.Type.Energy, amount = 1 };
        
        void OnEnable()
        {
            if(_arena != null)
                _arena.Register(this);
            else
                Debug.LogWarning("A deposit without arena", this);
        }

        void OnDisable()
        {
            if(_arena != null)
                _arena.Unregister(this);
        }
    }
}
