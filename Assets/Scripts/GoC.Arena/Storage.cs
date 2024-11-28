using System.Collections.Generic;
using System.Linq;
using GoC.ScriptingApi;
using UnityEngine;

namespace GoC.Arena
{
    public class Storage : MonoBehaviour, IStorage
    {
        [SerializeField] private int _capacityPerSubstance;
        [SerializeField] private int[] _content = Enumerable.Repeat(0, Substance.TYPES_COUNT).ToArray();

        
        public int capacityPerSubstance => _capacityPerSubstance;
        public IReadOnlyList<int> content => _content;
        
        
        public void Setup(int initialCapacityPerSubstance) => _capacityPerSubstance = initialCapacityPerSubstance;

        public void Deposit(Substance.Type substanceType, int amount) => _content[(int)substanceType] += amount;

        public void Withdraw(Substance.Type substanceType, int amount) => _content[(int)substanceType] -= amount;
    }
}
