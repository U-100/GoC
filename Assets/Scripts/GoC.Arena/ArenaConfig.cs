using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace GoC.Arena
{
    [CreateAssetMenu]
    public class ArenaConfig : ScriptableObject
    {
        [SerializeField] private Arena _prefab;
        public Arena prefab => _prefab;

        [SerializeField] private int2 _size;
        public int2 size => _size;
        
        [SerializeField] private DepositConfig[] _depositConfigs;
        public IReadOnlyList<DepositConfig> depositConfigs => _depositConfigs;
    }
}
