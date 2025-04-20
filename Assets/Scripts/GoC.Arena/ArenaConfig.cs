using Unity.Mathematics;
using UnityEngine;

namespace GoC.Arena
{
    [CreateAssetMenu]
    public class ArenaConfig : ScriptableObject
    {
        [SerializeField] private int2 _size;
        public int2 size => _size;

        [SerializeField] private DepositConfig _depositConfig;
        public DepositConfig depositConfig => _depositConfig;

        [SerializeField] private int _depositsCount;
        public int depositsCount => _depositsCount;

        [SerializeField] private ProductionConfig _productionConfig;
        public ProductionConfig productionConfig => _productionConfig;
        
        [SerializeField] private ProductionParams _initialUnitProductionParams;
        public ProductionParams initialUnitProductionParams => _initialUnitProductionParams;
    }
}
