using System.IO;
using Jint;
using Jint.Native;
using UnityEngine;

namespace GoC
{
    public class ArenaMain : MonoBehaviour
    {
        [SerializeField] private Unit _redStartingUnit;
        [SerializeField] private string _redScriptFilePath;

        [SerializeField] private Unit _blueStartingUnit;
        [SerializeField] private string _blueScriptFilePath;

        private Engine _redEngine;
        private JsValue _redTickFunction;
        
        private Engine _blueEngine;
        private JsValue _blueTickFunction;

        private float _tickCooldown;

        private void OnEnable()
        {
            UnifiedModel unifiedModel = new UnifiedModel(_redStartingUnit, _blueStartingUnit);
            
            _redEngine = EngineFactory.Create();
            _redTickFunction = PrepareTeam(_redEngine, unifiedModel.red, _redScriptFilePath);
            
            _blueEngine = EngineFactory.Create();
            _blueTickFunction = PrepareTeam(_blueEngine, unifiedModel.blue, _blueScriptFilePath);
        }

        private static JsValue PrepareTeam(Engine engine, IModel model, string scriptFilePath)
        {
            string script = File.ReadAllText(scriptFilePath);
            engine.SetValue("model", model);
            engine.Execute(script);
            return engine.GetValue("tick");
        }

        private void OnDisable()
        {
            _redEngine?.Dispose();
            _blueEngine?.Dispose();
        }

        private void Update()
        {
            if((_tickCooldown -= Time.deltaTime) <= 0)
            {
                _redEngine.Invoke(_redTickFunction);
                _blueEngine.Invoke(_blueTickFunction);
                _tickCooldown += Tick.TIME;
            }
        }
    }
}
