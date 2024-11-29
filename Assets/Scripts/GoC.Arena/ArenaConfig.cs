using Unity.Mathematics;
using UnityEngine;

namespace GoC.Arena
{
    [CreateAssetMenu]
    public class ArenaConfig : ScriptableObject
    {
        [SerializeField] private int2 _size;
        public int2 size => _size;
    }
}
