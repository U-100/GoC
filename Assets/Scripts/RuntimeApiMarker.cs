using System.Threading;
using Jint;
using UnityEngine;

namespace GoC
{
    public abstract class RuntimeApiMarker : MonoBehaviour
    {
        public abstract void SetupRuntimeApi(Engine engine, CancellationToken ct);
    }
}
