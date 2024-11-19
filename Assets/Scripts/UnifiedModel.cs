using System.Collections.Generic;
using System.Linq;

namespace GoC
{
    public class UnifiedModel
    {
        private readonly List<Factory> _factories = new();
        private readonly List<Unit> _units = new();

        private class Model : IModel
        {
            private readonly IEnumerable<Factory> _factories;
            private readonly IEnumerable<Unit> _units;

            public Model(int teamId, IEnumerable<Factory> factories, IEnumerable<Unit> units)
            {
                this.teamId = teamId;
                _factories = factories;
                _units = units;
            }

            public int id => teamId;
            public object data { get; set; }
            
            public int teamId { get; }

            public IMyFactory myFactory => _factories.FirstOrDefault(factory => factory.teamId == teamId);
            public IEnumerable<IMyUnit> myUnits => _units.Where(unit => unit.teamId == teamId);

            public IEnumerable<IFactory> factories => _factories;
            public IEnumerable<IUnit> units => _units;
        }

        public IModel AddTeamModel(int teamId, Factory factory)
        {
            factory.Init(teamId, unit => AddUnit(teamId, unit));
            _factories.Add(factory);

            return new Model(teamId, _factories, _units);
        }

        public void AddUnit(int teamId, Unit unit)
        {
            unit.Init(teamId);
            _units.Add(unit);
        }
    }
}
