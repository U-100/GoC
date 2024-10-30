using System.IO;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using UnityEngine;

namespace GoC
{
    public class ArenaMain : MonoBehaviour
    {
        [SerializeField] private Unit _redStartingUnit;
        [SerializeField] private string _redScriptFilePath;

        [SerializeField] private Unit _blueStartingUnit;
        [SerializeField] private string _blueScriptFilePath;

        private Engine _engine;
        private JsValue _redTickFunction;
        private JsValue _blueTickFunction;

        private float _tickCooldown;

        private void OnEnable()
        {
            _engine = EngineFactory.Create();
            UnifiedModel unifiedModel = new UnifiedModel(_redStartingUnit, _blueStartingUnit);
            _redTickFunction = PrepareTeam(_engine, unifiedModel.red, _redScriptFilePath);
            _blueTickFunction = PrepareTeam(_engine, unifiedModel.blue, _blueScriptFilePath);
        }

        private static JsValue PrepareTeam(Engine engine, IModel model, string scriptFilePath)
        {
            string script = File.ReadAllText(scriptFilePath);
            engine.Modules.Add(model.team, script);
            ObjectInstance moduleInstance = engine.Modules.Import(model.team);
            JsValue initFunction = engine.GetValue(moduleInstance, "init");
            engine.Invoke(initFunction, model);
            return engine.GetValue(moduleInstance, "tick");
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
                _engine.Invoke(_blueTickFunction);
                _tickCooldown += Tick.TIME;
            }
        }
    }
}
