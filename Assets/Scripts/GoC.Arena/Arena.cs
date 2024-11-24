using System.Collections.Generic;
using System.IO;
using System.Linq;
using GoC.ScriptingApi;
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

        private IEnumerable<IUnit> _units;
        public IEnumerable<IUnit> units => _units ??= _teamRecords.SelectMany(teamRecord => teamRecord.team.myUnits);
        
        private IEnumerable<IFactory> _factories;
        public IEnumerable<IFactory> factories => _factories ??= _teamRecords.SelectMany(teamRecord => teamRecord.team.myFactories);
        
        private readonly HashSet<DepositBehaviour> _deposits = new();
        public IEnumerable<IDeposit> deposits => _deposits;
        
        public void Register(DepositBehaviour deposit) => _deposits.Add(deposit);
        public void Unregister(DepositBehaviour deposit) => _deposits.Remove(deposit);

        public void Register(Team team)
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

        public void Unregister(Team team)
        {
            int index = _teamRecords.FindIndex(tr => tr.team == team);
            if (index >= 0)
            {
                _teamRecords[index].engine.Dispose();
                _teamRecords.RemoveAt(index);
            }
        }

        void OnDisable()
        {
            foreach (TeamRecord teamRecord in _teamRecords)
                teamRecord.engine.Dispose();
            
            _teamRecords.Clear();
        }

        void Update()
        {
            if ((_tickCooldown -= Time.deltaTime) <= 0)
            {
                foreach (TeamRecord teamRecord in _teamRecords)
                    teamRecord.team.BeforeTick();
                
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
                
                foreach (TeamRecord teamRecord in _teamRecords)
                    teamRecord.team.AfterTick();
            }
        }
    }
}
