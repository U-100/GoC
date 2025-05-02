using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace GoC.Arena
{
    [CreateAssetMenu]
    public class ArenaConfig : ScriptableObject
    {
        [SerializeField] private UnitConfig _unitConfig;
        public UnitConfig unitConfig => _unitConfig;

        [SerializeField] private DepositConfig[] _depositConfigs;
        public IReadOnlyList<DepositConfig> depositConfigs => _depositConfigs;

        [SerializeField] private int2 _size;
        public int2 size => _size;

        [SerializeField] private UnitParams _initialUnitParams;
        public UnitParams initialUnitParams => _initialUnitParams;
    }
}
