using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace GoC.Arena
{
    public static class UnityExtensions
    {
        public static int2 ToArenaPos(this Vector3 unityPos) => (int2)round(float2(unityPos.x, unityPos.z));

        public static Vector3 ToUnityPos(this int2 arenaPos) => new Vector3(arenaPos.x, 0, arenaPos.y);
    }
}
