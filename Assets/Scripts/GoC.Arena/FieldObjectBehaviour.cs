using GoC.ScriptingApi;
using Unity.Mathematics;

using static Unity.Mathematics.math;

namespace GoC.Arena
{
    public abstract class FieldObjectBehaviour : ApiObjectBehaviour
    {
        public int2 pos => (int2)round(float2(transform.position.x, transform.position.z));
    }
}
