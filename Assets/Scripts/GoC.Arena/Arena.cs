using System.Collections.Generic;
using GoC.ScriptingApi;

namespace GoC.Arena
{
    public class Arena
    {
        private readonly IArenaContext _arenaContext;
        private readonly Dictionary<int, IFactory> _factories = new();
        private readonly Dictionary<int, IUnit> _units = new();

        public Arena(IArenaContext arenaContext) => _arenaContext = arenaContext;

        public IModel AddTeamModel(int teamId, Factory factory)
        {
            factory.Init(teamId, _arenaContext, unit => AddUnit(teamId, unit));
            _factories.Add(factory.id, factory);

            return new TeamModel(teamId, _factories, _units);
        }

        private void AddUnit(int teamId, Unit unit)
        {
            unit.Init(teamId, _arenaContext);
            _units.Add(unit.id, unit);
        }
    }
}
