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
        public Factory factory;
    }

    public class Team : IDisposable
    {
        private readonly Engine _engine;
        private readonly IModel _model;
        private readonly JsValue _tickFunction;
        private readonly IArenaContext _arenaContext;

        public Team(int teamId, TeamSetup teamSetup, Arena arena, IArenaContext arenaContext)
        {
            _arenaContext = arenaContext;
            _model = arena.AddTeamModel(teamId, teamSetup.factory);
            
            _engine = EngineFactory.Create();
            _engine.SetValue("model", _model);

            string script = File.ReadAllText(teamSetup.scriptFilePath);
            _engine.Execute(script);
            
            _tickFunction = _engine.GetValue("tick");
        }

        public void Dispose() => _engine.Dispose();

        public void Update()
        {
            using(_arenaContext.ForModel(_model))
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
            IArenaContext arenaContext = new ArenaContext();
            Arena arena = new Arena(arenaContext);

            _teams = new Team[_teamSetups.Length];
            for (int iTeam = 0; iTeam < _teamSetups.Length; iTeam++)
                _teams[iTeam] = new Team(iTeam, _teamSetups[iTeam], arena, arenaContext);
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
