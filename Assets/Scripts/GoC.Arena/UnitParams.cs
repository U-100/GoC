using System.Collections.Generic;
using GoC.ScriptingApi;
using UnityEngine;

namespace GoC.Arena
{
    [CreateAssetMenu]
    public class UnitParams : ScriptableObject, IUnitParams
    {
        [Header("Exposed to scripting:")]
        [Space]
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

        [SerializeField] private List<UnitParams> _products;
        public IReadOnlyList<IUnitParams> products => _products ??= new List<UnitParams>();
        
        [HideInInspector]
        [SerializeField] private string _nameCopy;
        public new string name => _nameCopy; // we can't use Unity-provided name property because we need multithreaded access

        void Awake() => _nameCopy = base.name; // store name for multithreaded access

        [Header("Not exposed to scripting:")]
        [Space]
        [SerializeField] private UnitView _unitViewPrefab;
        public UnitView unitViewPrefab => _unitViewPrefab;
    }
}
