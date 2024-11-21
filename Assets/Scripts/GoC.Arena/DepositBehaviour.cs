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

    public class DepositBehaviour : MonoBehaviour, IDeposit
    {
        public int2 pos => transform.position.ToArenaPos();

        public DepositExtractionOutcome Extract() => new DepositExtractionOutcome { substanceType = Substance.Type.Energy, amount = 1 };
    }
}
