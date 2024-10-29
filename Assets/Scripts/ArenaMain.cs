using System.IO;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using UnityEngine;

namespace GoC
{
    public class ArenaMain : MonoBehaviour
    {
        [SerializeField] private string _redScriptFilePath;
        [SerializeField] private Unit _unit;

        private Engine _engine;
        private JsValue _redTickFunction;

        private const float TICK = 1;
        private float _tickCooldown;

        private void OnEnable()
        {
            if (!string.IsNullOrEmpty(_redScriptFilePath))
            {
                _engine = EngineFactory.Create();
                
                string redScript = File.ReadAllText(_redScriptFilePath);
                _engine.Modules.Add("red", redScript);

                ObjectInstance redModuleInstance = _engine.Modules.Import("red");

                Model redModel = new Model(_unit) { team = "red" };

                JsValue redInitFunction = _engine.GetValue(redModuleInstance, "init");
                _engine.Invoke(redInitFunction, redModel);

                _redTickFunction = _engine.GetValue(redModuleInstance, "tick");
            }
        }

        private void OnDisable()
        {
            _engine?.Dispose();
        }

        private void Update()
        {
            if((_tickCooldown -= Time.deltaTime) <= 0)
            {
                _engine.Invoke(_redTickFunction);
                _tickCooldown += TICK;
            }
        }
    }
}
