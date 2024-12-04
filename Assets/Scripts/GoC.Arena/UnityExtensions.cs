using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace GoC.Arena
{
    public static class UnityExtensions
    {
        public static int2 ToArenaPos(this Vector3 unityPos) => (int2)round(float2(unityPos.x, unityPos.z));
        
        public static int2 ToArenaPos(this float3 unityPos) => (int2)round(float2(unityPos.x, unityPos.z));

        public static Vector3 ToUnityPos(this int2 arenaPos) => new Vector3(arenaPos.x, 0, arenaPos.y);
        
        public static float3 ToFloat3Pos(this int2 arenaPos) => new float3(arenaPos.x, 0, arenaPos.y);
    }
}
