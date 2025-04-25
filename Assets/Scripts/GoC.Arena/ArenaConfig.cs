using Unity.Mathematics;
using UnityEngine;

namespace GoC.Arena
{
    [CreateAssetMenu]
    public class ArenaConfig : ScriptableObject
    {
        [SerializeField] private UnitConfig _unitConfig;
        public UnitConfig unitConfig => _unitConfig;

        [SerializeField] private DepositConfig _depositConfig;
        public DepositConfig depositConfig => _depositConfig;

        [SerializeField] private int2 _size;
        public int2 size => _size;

        [SerializeField] private int _depositsCount;
        public int depositsCount => _depositsCount;
        
        [SerializeField] private UnitParams _initialUnitParams;
        public UnitParams initialUnitParams => _initialUnitParams;
    }
}
