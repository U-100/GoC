using System;
using System.IO;
using Jint;
using Jint.Native;
using Jint.Runtime.Interop;
using UnityEngine;

namespace GoC
{
    [Serializable]
    public struct TeamSetup
    {
        public string scriptFilePath;
        public Factory factory;
    }

    [Serializable]
    public struct Team
    {
        public int teamId;
        public Engine engine;
        public JsValue tickFunction;
    }

    public class ArenaMain : MonoBehaviour
    {
        [SerializeField] private TeamSetup[] _teamSetups;

        private Team[] _teams;

        private float _tickCooldown;

        private void OnEnable()
        {
            UnifiedModel unifiedModel = new UnifiedModel();

            _teams = new Team[_teamSetups.Length];
            for (int iTeam = 0; iTeam < _teamSetups.Length; iTeam++)
            {
                TeamSetup teamSetup = _teamSetups[iTeam];

                Engine engine = EngineFactory.Create();
                IModel model = unifiedModel.AddTeamModel(iTeam, teamSetup.factory);
                JsValue tickFunction = PrepareTeam(engine, model, teamSetup.scriptFilePath);
                
                _teams[iTeam] =
                    new Team
                    {
                        teamId = iTeam,
                        engine = engine,
                        tickFunction = tickFunction
                    };
            }
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
            foreach (Team team in _teams)
                team.engine?.Dispose();
        }

        private void Update()
        {
            if ((_tickCooldown -= Time.deltaTime) <= 0)
            {
                foreach (Team team in _teams)
                    team.engine.Invoke(team.tickFunction);

                _tickCooldown += Tick.TIME;
            }
        }
    }
}
