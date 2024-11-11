using System.IO;
using Jint;
using Jint.Native;
using Jint.Runtime.Interop;
using UnityEngine;

namespace GoC
{
    public class ArenaMain : MonoBehaviour
    {
        [SerializeField] private string _redScriptFilePath;
        [SerializeField] private Factory _redFactory;
        [SerializeField] private Unit[] _redUnits;

        [SerializeField] private string _blueScriptFilePath;
        [SerializeField] private Factory _blueFactory;
        [SerializeField] private Unit[] _blueUnits;

        private Engine _redEngine;
        private JsValue _redTickFunction;
        
        private Engine _blueEngine;
        private JsValue _blueTickFunction;

        private float _tickCooldown;

        private void OnEnable()
        {
            UnifiedModel unifiedModel = new UnifiedModel(_redFactory, _blueFactory);
            
            _redEngine = EngineFactory.Create();
            _redTickFunction = PrepareTeam(_redEngine, unifiedModel.redModel, _redScriptFilePath);
            
            _blueEngine = EngineFactory.Create();
            _blueTickFunction = PrepareTeam(_blueEngine, unifiedModel.blueModel, _blueScriptFilePath);
        }

        private static JsValue PrepareTeam(Engine engine, IModel model, string scriptFilePath)
        {
            string script = File.ReadAllText(scriptFilePath);
            engine.SetValue("model", model);
            engine.SetValue("Result", TypeReference.CreateTypeReference<Result>(engine));
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
