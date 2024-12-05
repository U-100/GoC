using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace GoC.Arena
{
    [CreateAssetMenu]
    public class ArenaConfig : ScriptableObject
    {
        [SerializeField] private int2 _size;
        public int2 size => _size;

        [SerializeField] private TeamConfig[] _teamConfigs;
        public IReadOnlyList<TeamConfig> teamConfigs => _teamConfigs;
        
        [SerializeField] private FactoryConfig _factoryConfig;
        public FactoryConfig factoryConfig => _factoryConfig;

        [SerializeField] private DepositConfig _depositConfig;
        public DepositConfig depositConfig => _depositConfig;
    }
}
