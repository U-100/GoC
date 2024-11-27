using GoC.ScriptingApi;
using UnityEngine;

namespace GoC.Arena
{
    public class FactoryControl : MonoBehaviour, IFactoryControl
    {
        private Factory _factory;
        private Factory factory => _factory ??= GetComponent<Factory>();

        public Result Produce() => factory.Produce();
    }
}
