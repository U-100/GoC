using System.Collections.Generic;
using GoC.ScriptingApi;
using UnityEngine;

namespace GoC.Arena
{
    [CreateAssetMenu]
    public class UnitParams : ScriptableObject, IUnitParams
    {
        [SerializeField] private string _nameCopy;
        string IUnitParams.name => _nameCopy;
        
        [SerializeField] private int[] _productionCost;
        public IReadOnlyList<int> productionCost => _productionCost;

        [SerializeField] private int _productionTicks;
        public int productionTicks => _productionTicks;

        [SerializeField] private bool _canMove;
        public bool canMove => _canMove;

        [SerializeField] private int _maxIntegrity;
        public int maxIntegrity => _maxIntegrity;

        [SerializeField] private int _storageCapacityPerSubstance;
        public int storageCapacityPerSubstance => _storageCapacityPerSubstance;

        [SerializeField] private int _damage;
        public int damage => _damage;

        [SerializeField] private bool _canMine;
        public bool canMine => _canMine;

        [SerializeField] private List<UnitParams> _productParams;
        public IReadOnlyList<IUnitParams> products => _productParams ??= new List<UnitParams>();

        private void Awake() => _nameCopy = name; // store name for multithreaded access
    }
}
