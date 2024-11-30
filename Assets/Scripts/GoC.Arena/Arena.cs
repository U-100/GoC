using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GoC.ScriptingApi;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;
using Debug = UnityEngine.Debug;

namespace GoC.Arena
{
    public struct TeamRecord
    {
        public Engine engine;
        public Team team;
        public JsValue tickFunction;
    }
    
    public class Arena : MonoBehaviour
    {
        [SerializeField] private ArenaConfig _config;
        [Space]
        [SerializeField] private VisualEffect _gridEffect;
        [SerializeField] private GameObject _quad;
        
        private readonly List<TeamRecord> _teamRecords = new();
        private float _tickCooldown;
        private Team _executingTeam;

        private IEnumerable<IUnit> _units;
        private IEnumerable<IFactory> _factories;
        private readonly HashSet<Deposit> _deposits = new();

        private Grid<int> _grid;

        
        public uint currentTick { get; private set; } 
        public int executingTeamId => _executingTeam.teamId;

        public IGrid<int> grid => _grid;
        
        public IEnumerable<IUnit> units => _units ??= _teamRecords.SelectMany(teamRecord => teamRecord.team.myUnits);
        
        public IEnumerable<IFactory> factories => _factories ??= _teamRecords.SelectMany(teamRecord => teamRecord.team.myFactories);
        
        public IEnumerable<IDeposit> deposits => _deposits;

        public void Register(Team team)
        {
            Engine engine = EngineFactory.Create();
            engine.SetValue("model", team);
            
            ObjectInstance teamModule = engine.Modules.Import(team.scriptFile);
            JsValue tickFunction = teamModule.Get("tick");

            if (tickFunction.IsObject())
                _teamRecords.Add(
                    new TeamRecord
                    {
                        team = team,
                        engine = engine,
                        tickFunction = tickFunction
                    }
                );
            else
                Debug.LogWarning("Team script doesn't export the tick() function", team);
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
        
        public void Register(Deposit deposit) => _deposits.Add(deposit);
        public void Unregister(Deposit deposit) => _deposits.Remove(deposit);


        private void OnEnable()
        {
            _grid = new Grid<int>(_config.size / -2, (_config.size - 1) / 2);
            
            _gridEffect.SetFloat("mapSize", _config.size.x - 1);
            _gridEffect.Play();
        }

        private void OnDisable()
        {
            foreach (TeamRecord teamRecord in _teamRecords)
                teamRecord.engine.Dispose();
            
            _teamRecords.Clear();
        }

        private void OnDrawGizmos()
        {
            if (grid == null)
                return;
            
            for(int2 p = grid.min; p.y <= grid.max.y; ++p.y)
            for (p.x = grid.min.x; p.x <= grid.max.x; ++p.x)
                if (grid[p] > 0)
                    Gizmos.DrawWireCube(p.ToUnityPos(), new Vector3(1, .1f, 1));
        }

        private void Update()
        {
            if ((_tickCooldown -= Time.deltaTime) <= 0)
            {
                Stopwatch tickStopwatch = Stopwatch.StartNew();
                try
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
                finally
                {
                    //Debug.Log($"Arena tick: {tickStopwatch.ElapsedMilliseconds} ms");
                }
            }
        }
    }
}
