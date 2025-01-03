using System;
using System.Collections.Generic;
using System.Linq;
using GoC.ScriptingApi;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

using static Unity.Mathematics.math;

namespace GoC.Arena
{
    public interface IArena : IDisposable
    {
        void Setup(ArenaConfig initConfig);
        void CreateTeams(params TeamConfig[] teamConfigs);
    }

    public struct TeamRecord
    {
        public Engine engine;
        public Team team;
        public JsValue tickFunction;
        public IEnumerable<IUnit> enemyUnits;
    }
    
    public class Arena : MonoBehaviour, IArena
    {
        [SerializeField] private ArenaConfig _config;
        [Space]
        [SerializeField] private VisualEffect _gridEffect;
        [SerializeField] private Team _teamPrefab;
        [SerializeField] private Unit _unitPrefab;
        [SerializeField] private Deposit _depositPrefab;
        
        private Grid<int> _grid;
        private readonly List<Deposit> _deposits = new();
        private readonly List<Deposit> _depositsCache = new();
        private readonly Dictionary<Team, TeamRecord> _teamRecords = new();
        private float _tickCooldown;
        private Team _executingTeam;

        public void Setup(ArenaConfig initConfig)
        {
            _config = initConfig;
            
            _grid = new Grid<int>(_config.size / -2, (_config.size - 1) / 2);
            
            _gridEffect.SetFloat("mapSize", _config.size.x - 1);
            _gridEffect.Play();
        }
        
        public void CreateTeams(params TeamConfig[] teamConfigs)
        {
            for (int iTeam = 0; iTeam < teamConfigs.Length; iTeam++)
            {
                TeamConfig teamConfig = teamConfigs[iTeam];
                Team team = CreateTeam(teamConfig);

                CreateTeamLandscape(team, iTeam, teamConfigs.Length);
            }
        }

        public void Dispose()
        {
            if (gameObject != null)
                Destroy(gameObject);
        }

        public uint currentTick { get; private set; } 
        public int executingTeamId => _executingTeam.teamId;

        public IGrid<int> grid => _grid;
        
        public IEnumerable<IUnit> units { get; private set; }

        public IReadOnlyList<IDeposit> deposits => _deposits;

        public IEnumerable<IUnit> GetEnemyUnits(Team team) => _teamRecords[team].enemyUnits;
        
        public void Register(Team team)
        {
            if (_teamRecords.ContainsKey(team))
                throw new Exception($"Team {team} is already registered in the arena {this}");
            
            Engine engine = EngineFactory.Create();
            engine.SetValue("model", team);
            
            ObjectInstance teamModule = engine.Modules.Import(team.scriptFile);
            JsValue tickFunction = teamModule.Get("tick");

            if (tickFunction.IsObject())
                _teamRecords.Add(
                    team,
                    new TeamRecord
                    {
                        team = team,
                        engine = engine,
                        tickFunction = tickFunction,
                        enemyUnits = _teamRecords.Where(kvp => kvp.Key != team).SelectMany(kvp => kvp.Value.team.myUnits),
                    }
                );
            else
                Debug.LogWarning("Team script doesn't export the tick() function", team);
        }

        public void Unregister(Team team)
        {
            if (_teamRecords.TryGetValue(team, out TeamRecord teamRecord))
            {
                teamRecord.engine.Dispose();
                _teamRecords.Remove(team);
            }
        }
        
        public void Register(Deposit deposit) => _deposits.Add(deposit);
        public void Unregister(Deposit deposit) => _deposits.Remove(deposit);


        private void Awake()
        {
            units ??= _teamRecords.Values.SelectMany(teamRecord => teamRecord.team.myUnits);
        }
        
        private Team CreateTeam(TeamConfig teamConfig)
        {
            Team team = Instantiate(_teamPrefab);
            team.Setup(this, teamConfig);
            Register(team);
            return team;
        }

        private void CreateTeamLandscape(Team team, int iTeam, int teamsCount)
        {
            int2 baseFactoryPos = _grid.size / 4;
            int2 baseDepositPos = _grid.size / 3;

            float teamAngleRadians = 2 * iTeam * PI / teamsCount;

            int2 factoryPos = rotate(Unity.Mathematics.quaternion.RotateY(teamAngleRadians), baseFactoryPos.ToUnityPos()).ToArenaPos();
            Unit unit = Instantiate(_unitPrefab, factoryPos.ToUnityPos(), Quaternion.identity);
            unit.Setup(team, _config.unitConfig);
            team.Register(unit);

            int2 depositPos = rotate(Unity.Mathematics.quaternion.RotateY(teamAngleRadians), baseDepositPos.ToUnityPos()).ToArenaPos();
            Deposit deposit = Instantiate(_depositPrefab, depositPos.ToUnityPos(), Quaternion.identity);
            deposit.Setup(this, _config.depositConfig);
            Register(deposit);
        }

        private void OnDestroy()
        {
            try
            {
                _depositsCache.AddRange(_deposits);
                foreach (Deposit deposit in _depositsCache)
                    Destroy(deposit.gameObject);
            }
            finally
            {
                _depositsCache.Clear();
            }

            foreach (TeamRecord teamRecord in _teamRecords.Values)
            {
                teamRecord.engine.Dispose();
                Destroy(teamRecord.team.gameObject);
            }
            
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
            if (_teamRecords.Count == 0)
                return; // teams haven't been created yet
            
            if ((_tickCooldown -= Time.deltaTime) <= 0)
            {
                //System.Diagnostics.Stopwatch tickStopwatch = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    foreach (TeamRecord teamRecord in _teamRecords.Values)
                        teamRecord.team.BeforeTick();

                    foreach (TeamRecord teamRecord in _teamRecords.Values)
                    {
                        try
                        {
                            _executingTeam = teamRecord.team;
                            teamRecord.engine.Invoke(teamRecord.tickFunction);
                        }
                        catch (Exception exception)
                        {
                            Debug.LogException(exception, teamRecord.team);
                        }
                        finally
                        {
                            _executingTeam = null;
                        }
                    }

                    foreach (TeamRecord teamRecord in _teamRecords.Values)
                        teamRecord.team.AfterTick();
                }
                finally
                {
                    _tickCooldown += Tick.TIME;
                    ++currentTick;

                    //Debug.Log($"Arena tick: {tickStopwatch.ElapsedMilliseconds} ms");
                }
            }
        }
    }
}
