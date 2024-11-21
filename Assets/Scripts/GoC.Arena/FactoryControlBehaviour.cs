using GoC.ScriptingApi;
using UnityEngine;

namespace GoC.Arena
{
    public class FactoryControlBehaviour : MonoBehaviour, IFactoryControl
    {
        private FactoryBehaviour _factoryBehaviour;
        private FactoryBehaviour factoryBehaviour => _factoryBehaviour ??= GetComponent<FactoryBehaviour>();

        public Result Produce() => factoryBehaviour.Produce();
    }
}
