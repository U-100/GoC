using System;
using System.Collections.Generic;
using System.Linq;
using GoC.ScriptingApi;

namespace GoC.Arena
{
    public class TeamModel : IModel
    {
        private readonly IReadOnlyDictionary<int, IFactory> _factories;
        private readonly IReadOnlyDictionary<int, IUnit> _units;

        private readonly Func<IApiObject, bool> _hasMyTeamId;

        public TeamModel(int teamId, IReadOnlyDictionary<int, IFactory> factories, IReadOnlyDictionary<int, IUnit> units)
        {
            this.teamId = teamId;
            _factories = factories;
            _units = units;
            _hasMyTeamId = factory => factory.teamId == this.teamId;
        }

        public int id => teamId;
            
        public int teamId { get; }

        private IEnumerable<IFactory> _myFactories;
        public IEnumerable<IFactory> myFactories => _myFactories ??= _factories.Values.Where<IFactory>(_hasMyTeamId);

        private IEnumerable<IUnit> _myUnits;
        public IEnumerable<IUnit> myUnits => _myUnits ??= _units.Values.Where<IUnit>(_hasMyTeamId);

        public IReadOnlyDictionary<int, IFactory> factories => _factories;
        public IReadOnlyDictionary<int, IUnit> units => _units;
    }
}
