using System;
using System.IO;
using GoC.ScriptingApi;
using Jint;
using Jint.Native;
using UnityEngine;

namespace GoC.Arena
{
    [Serializable]
    public struct TeamSetup
    {
        public string scriptFilePath;
        public FactoryBehaviour factoryBehaviour;
    }

    public struct TeamRecord
    {
        public Engine engine;
        public IModel model;
        public JsValue tickFunction;
    }
    
    public class ArenaMain : MonoBehaviour
    {
        [SerializeField] private TeamSetup[] _teamSetups;

        private Arena _arena;
        private TeamRecord[] _teamRecords;
        private float _tickCooldown;

        private void OnEnable()
        {
            _arena = new Arena();

            _teamRecords = new TeamRecord[_teamSetups.Length];
            for (int teamId = 0; teamId < _teamSetups.Length; ++teamId)
            {
                TeamSetup teamSetup = _teamSetups[teamId];
                IModel model = _arena.AddTeamModel(teamId, teamSetup.factoryBehaviour);
            
                Engine engine = EngineFactory.Create();
                engine.SetValue("model", model);

                string script = File.ReadAllText(teamSetup.scriptFilePath);
                engine.Execute(script);
                JsValue tickFunction = engine.GetValue("tick");
                
                _teamRecords[teamId] = new TeamRecord { model = model, engine = engine, tickFunction = tickFunction };
            }
        }

        private void OnDisable()
        {
            foreach (TeamRecord teamRecord in _teamRecords)
                teamRecord.engine.Dispose();
        }

        private void Update()
        {
            if ((_tickCooldown -= Time.deltaTime) <= 0)
            {
                foreach (TeamRecord teamRecord in _teamRecords)
                    using(_arena.ForModel(teamRecord.model))
                        teamRecord.engine.Invoke(teamRecord.tickFunction);

                _tickCooldown += Tick.TIME;
            }
        }
    }
}
