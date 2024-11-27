using System.Collections.Generic;
using System.Linq;
using GoC.ScriptingApi;
using UnityEngine;

namespace GoC.Arena
{
    public interface IStorage
    {
        public IReadOnlyList<int> content { get; }
        bool Deposit(Substance.Type substanceType, int amount);
        bool Withdraw(Substance.Type substanceType, int amount);
    }

    public class Storage : MonoBehaviour, IStorage
    {
        [SerializeField]
        private int[] _content = Enumerable.Repeat(0, Substance.TYPES_COUNT).ToArray();
        
        public IReadOnlyList<int> content => _content;
        
        public bool Deposit(Substance.Type substanceType, int amount)
        {
            _content[(int)substanceType] += amount;
            return true;
        }

        public bool Withdraw(Substance.Type substanceType, int amount)
        {
            if(_content[(int)substanceType] >= amount)
            {
                _content[(int)substanceType] -= amount;
                return true;
            }

            return false;
        }
    }
}
