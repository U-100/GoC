using System;
using System.Collections.Generic;
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
        public uint currentTick { get; }
        
        void Setup(ArenaConfig initConfig);
        void CreateTeams(params TeamConfig[] teamConfigs);
    }

    public struct TeamRecord
    {
        public Team team;
        public Engine engine;
        public JsValue tickFunction;
    }
    
    public class Arena : MonoBehaviour, IArena
    {
        [SerializeField] private ArenaConfig _config;
        [Space]
        [SerializeField] private VisualEffect _gridEffect;
        [SerializeField] private Unit _stationaryUnitPrefab;
        [SerializeField] private Unit _movingUnitPrefab;
        [SerializeField] private Deposit _depositPrefab;
        
        private Grid<int> _grid;
        private readonly List<Deposit> _deposits = new();
        private readonly List<Deposit> _depositsCache = new();

        private readonly List<ExposedUnit> _units = new();
        
        private readonly List<TeamRecord> _teamRecords = new();
        private float _tickCooldown;
        private Team _executingTeam;

        public ArenaConfig config => _config;
        
        public uint currentTick { get; private set; } 
        public int executingTeamId => _executingTeam.teamId;

        public IGrid<int> grid => _grid;

        public IReadOnlyList<IDeposit> deposits => _deposits;
        public IReadOnlyList<IUnit> units => _units;

        
        public void Setup(ArenaConfig initConfig)
        {
            _config = initConfig;
            
            _grid = new Grid<int>(_config.size / -2, (_config.size - 1) / 2);
            
            _gridEffect.SetFloat("mapSize", _config.size.x - 1);
            _gridEffect.Play();
        }

        public void Dispose()
        {
            if (gameObject != null)
                Destroy(gameObject);
        }
        
        public void CreateTeams(params TeamConfig[] teamConfigs)
        {
            for (int iTeam = 0; iTeam < teamConfigs.Length; iTeam++)
            {
                TeamConfig teamConfig = teamConfigs[iTeam];
                Team team = new Team(iTeam, this, teamConfig);

                Engine engine = EngineFactory.Create();
                engine.SetValue("model", team);
            
                ObjectInstance teamModule = engine.Modules.Import(teamConfig.scriptFile);
                JsValue tickFunction = teamModule.Get("tick");

                if (tickFunction.IsObject())
                    _teamRecords.Add(
                        new TeamRecord
                        {
                            team = team,
                            engine = engine,
                            tickFunction = tickFunction,
                        }
                    );
                else
                    Debug.LogWarning($"Team script {teamConfig.scriptFile} doesn't export the tick() function");
            }

            CreateDeposits();
            
            // NOTE: create all teams before adding units so that enemy unit lists get properly populated for all teams
            for (int iTeam = 0; iTeam < _teamRecords.Count; iTeam++)
            {
                Team team = _teamRecords[iTeam].team;
                CreateTeamLandscape(team, iTeam, _teamRecords.Count);
            }
        }

        private void CreateDeposits()
        {
            int2 baseDepositPos = _grid.size / 6;

            float anglePerTeam = 2 * PI / _teamRecords.Count;
            float anglePerDeposit = anglePerTeam / 2 / _config.depositConfigs.Count;

            for (int iTeam = 0; iTeam < _teamRecords.Count; iTeam++)
            {
                for (int iDeposit = 0; iDeposit < _config.depositConfigs.Count; ++iDeposit)
                {
                    float depositAngleRadians = anglePerTeam * iTeam + anglePerDeposit * iDeposit;

                    int2 depositPos = rotate(Unity.Mathematics.quaternion.RotateY(depositAngleRadians), baseDepositPos.ToUnityPos()).ToArenaPos();
                    CreateDeposit(_config.depositConfigs[iDeposit], depositPos);
                }
            }
        }
        
        private void CreateTeamLandscape(Team team, int iTeam, int teamsCount)
        {
            int2 baseUnitPos = _grid.size / 4;

            float teamAngleRadians = 2 * iTeam * PI / teamsCount;

            int2 unitPos = rotate(Unity.Mathematics.quaternion.RotateY(teamAngleRadians), baseUnitPos.ToUnityPos()).ToArenaPos();
            CreateUnit(_config.initialUnitParams, team, unitPos);
        }

        public Deposit CreateDeposit(DepositConfig depositConfig, int2 depositPos)
        {
            Deposit deposit = Instantiate(_depositPrefab, depositPos.ToUnityPos(), Quaternion.identity);
            deposit.Setup(this, depositConfig);
            _deposits.Add(deposit);
            return deposit;
        }

        public void Unregister(Deposit deposit) => _deposits.Remove(deposit);

        public Unit CreateUnit(UnitParams unitParams, Team team, int2 unitPos)
        {
            Unit unitPrefab = _config.unitConfig.movementTiers[unitParams.movementTier].canMove ? _movingUnitPrefab : _stationaryUnitPrefab;
            
            Unit unit = Instantiate(unitPrefab, unitPos.ToUnityPos(), Quaternion.identity);
            unit.Setup(team, unitParams);

            _units.Add(new ExposedUnit(unit));
            
            foreach (TeamRecord teamRecord in _teamRecords)
            {
                if (teamRecord.team.teamId == unit.teamId)
                    teamRecord.team.AddMyUnit(unit);
                else
                    teamRecord.team.AddEnemyUnit(unit);
            }
            
            return unit;
        }
        
        public void Unregister(Unit unit)
        {
            _units.RemoveAll(exposedUnit => exposedUnit.unit == unit);
            
            foreach (TeamRecord teamRecord in _teamRecords)
            {
                if (teamRecord.team.teamId == unit.teamId)
                    teamRecord.team.RemoveMyUnit(unit);
                else
                    teamRecord.team.RemoveEnemyUnit(unit);
            }
        }

        private void OnDestroy()
        {
            try
            {
                _depositsCache.AddRange(_deposits);
                foreach (Deposit deposit in _depositsCache)
                    if (deposit != null)
                        Destroy(deposit.gameObject);
            }
            finally
            {
                _depositsCache.Clear();
            }

            foreach (TeamRecord teamRecord in _teamRecords)
            {
                teamRecord.engine.Dispose();
                teamRecord.team.Dispose();
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
                    foreach (TeamRecord teamRecord in _teamRecords)
                        teamRecord.team.BeforeTick();

                    foreach (TeamRecord teamRecord in _teamRecords)
                    {
                        try
                        {
                            _executingTeam = teamRecord.team;
                            teamRecord.engine.Invoke(teamRecord.tickFunction);
                        }
                        catch (Exception exception)
                        {
                            Debug.LogException(exception);
                        }
                        finally
                        {
                            _executingTeam = null;
                        }
                    }

                    foreach (TeamRecord teamRecord in _teamRecords)
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
