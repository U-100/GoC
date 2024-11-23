using System.Collections.Generic;
using System.IO;
using Jint;
using Jint.Native;
using UnityEngine;

namespace GoC.Arena
{
    public struct TeamRecord
    {
        public Engine engine;
        public Team team;
        public JsValue tickFunction;
    }
    
    public class Arena : MonoBehaviour, IArenaContext
    {
        private readonly List<TeamRecord> _teamRecords = new();
        private float _tickCooldown;
        private Team _executingTeam;

        public uint currentTick { get; private set; } 
        public int executingTeamId => _executingTeam.teamId;
        
        public void TeamEnabled(Team team)
        {
            Engine engine = EngineFactory.Create();
            engine.SetValue("model", team);

            string script = File.ReadAllText(team.scriptFilePath);
            engine.Execute(script);
            JsValue tickFunction = engine.GetValue("tick");

            _teamRecords.Add(
                new TeamRecord
                {
                    team = team,
                    engine = engine,
                    tickFunction = tickFunction
                }
            );
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
                {
                    try
                    {
                        _executingTeam = teamRecord.team;
                        teamRecord.engine.Invoke(teamRecord.tickFunction);
                    }
                    finally
                    {
                        _executingTeam = null;
                    }
                }

                _tickCooldown += Tick.TIME;
                ++currentTick;
            }
        }
    }
}
