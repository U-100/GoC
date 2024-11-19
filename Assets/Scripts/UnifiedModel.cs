using System;
using System.Collections.Generic;
using System.Linq;

namespace GoC
{
    public class UnifiedModel
    {
        private readonly IExecutionContext _executionContext;
        private readonly Dictionary<int, IFactory> _factories = new();
        private readonly Dictionary<int, IUnit> _units = new();

        public UnifiedModel(IExecutionContext executionContext) => _executionContext = executionContext;

        private class Model : IModel
        {
            private readonly IReadOnlyDictionary<int, IFactory> _factories;
            private readonly IReadOnlyDictionary<int, IUnit> _units;

            private readonly Func<IApiObject, bool> _hasMyTeamId;

            public Model(int teamId, IReadOnlyDictionary<int, IFactory> factories, IReadOnlyDictionary<int, IUnit> units)
            {
                this.teamId = teamId;
                _factories = factories;
                _units = units;
                _hasMyTeamId = factory => factory.teamId == this.teamId;
            }

            public int id => teamId;
            public object data { get; set; }
            
            public int teamId { get; }

            private IEnumerable<IFactory> _myFactories;
            public IEnumerable<IFactory> myFactories => _myFactories ??= _factories.Values.Where<IFactory>(_hasMyTeamId);

            private IEnumerable<IUnit> _myUnits;
            public IEnumerable<IUnit> myUnits => _myUnits ??= _units.Values.Where<IUnit>(_hasMyTeamId);

            public IReadOnlyDictionary<int, IFactory> factories => _factories;
            public IReadOnlyDictionary<int, IUnit> units => _units;
        }

        public IModel AddTeamModel(int teamId, Factory factory)
        {
            factory.Init(teamId, _executionContext, unit => AddUnit(teamId, unit));
            _factories.Add(factory.id, factory);

            return new Model(teamId, _factories, _units);
        }

        private void AddUnit(int teamId, Unit unit)
        {
            unit.Init(teamId, _executionContext);
            _units.Add(unit.id, unit);
        }
    }
}
