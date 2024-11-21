using GoC.ScriptingApi;
using Unity.Mathematics;
using UnityEngine;

namespace GoC.Arena
{
    public class DepositBehaviour : MonoBehaviour, IDeposit
    {
        public int2 pos => transform.position.ToArenaPos();

        public struct Outcome
        {
            public Substance.Type substanceType;
            public int amount;
        }

        public Outcome Mine() => new Outcome { substanceType = Substance.Type.Energy, amount = 1 };
    }
}
