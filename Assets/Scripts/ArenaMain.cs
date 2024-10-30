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
            _redTickFunction = PrepareTeam(_engine, "red", _redStartingUnit, _redScriptFilePath);
            _blueTickFunction = PrepareTeam(_engine, "blue", _blueStartingUnit, _blueScriptFilePath);
        }

        private static JsValue PrepareTeam(Engine engine, string team, Unit startingUnit, string scriptFilePath)
        {
            Model model = new Model(startingUnit) { team = team };

            string script = File.ReadAllText(scriptFilePath);
            engine.Modules.Add(team, script);
            ObjectInstance moduleInstance = engine.Modules.Import(team);
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
