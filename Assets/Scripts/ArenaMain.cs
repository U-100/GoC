using System;
using System.IO;
using GoC.ScriptingApi;
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

    public class Team : IDisposable
    {
        private readonly Engine _engine;
        private readonly IModel _model;
        private readonly JsValue _tickFunction;
        private readonly IExecutionContext _executionContext;

        public Team(int teamId, TeamSetup teamSetup, UnifiedModel unifiedModel, IExecutionContext executionContext)
        {
            _executionContext = executionContext;
            
            _engine = EngineFactory.Create();
            
            _model = unifiedModel.AddTeamModel(teamId, teamSetup.factory);

            _engine.SetValue("model", _model);
            _engine.SetValue("Result", TypeReference.CreateTypeReference<Result>(_engine));

            string script = File.ReadAllText(teamSetup.scriptFilePath);
            _engine.Execute(script);
            
            _tickFunction = _engine.GetValue("tick");
        }

        public void Dispose() => _engine.Dispose();

        public void Update()
        {
            using(_executionContext.ForModel(_model))
                _engine.Invoke(_tickFunction);
        }
    }

    public class ArenaMain : MonoBehaviour
    {
        [SerializeField] private TeamSetup[] _teamSetups;

        private Team[] _teams;

        private float _tickCooldown;

        private void OnEnable()
        {
            IExecutionContext executionContext = new ExecutionContext();
            UnifiedModel unifiedModel = new UnifiedModel(executionContext);

            _teams = new Team[_teamSetups.Length];
            for (int iTeam = 0; iTeam < _teamSetups.Length; iTeam++)
                _teams[iTeam] = new Team(iTeam, _teamSetups[iTeam], unifiedModel, executionContext);
        }

        private void OnDisable()
        {
            foreach (Team team in _teams)
                team.Dispose();
        }

        private void Update()
        {
            if ((_tickCooldown -= Time.deltaTime) <= 0)
            {
                foreach (Team team in _teams)
                    team.Update();

                _tickCooldown += Tick.TIME;
            }
        }
    }
}
