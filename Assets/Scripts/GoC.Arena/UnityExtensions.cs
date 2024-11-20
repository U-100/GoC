using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

namespace GoC.Arena
{
    public static class UnityExtensions
    {
        public static int2 ToArenaPos(this Vector3 transformPosition) => (int2)round(float2(transformPosition.x, transformPosition.z));
    }
}
