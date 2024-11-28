using GoC.ScriptingApi;
using UnityEngine;

namespace GoC.Arena
{
    public interface IDepositConfig
    {
        Substance.Type substanceType { get; }
        int miningSpeed { get; }
    }
    
    [CreateAssetMenu]
    public class DepositConfig : ScriptableObject, IDepositConfig
    {
        [SerializeField] private Substance.Type _substanceType;
        public Substance.Type substanceType => _substanceType;

        [SerializeField] private int _miningSpeed;
        public int miningSpeed => _miningSpeed;
    }
}
