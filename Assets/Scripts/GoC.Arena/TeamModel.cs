using System;
using System.Collections.Generic;
using System.Linq;
using GoC.ScriptingApi;

namespace GoC.Arena
{
    public class TeamModel : IModel
    {
        private readonly Func<IFactory, bool> _factoryIsMine;
        private readonly Func<IUnit, bool> _unitIsMine;

        public TeamModel(int teamId, IReadOnlyDictionary<int, IFactory> factories, IReadOnlyDictionary<int, IUnit> units)
        {
            this.teamId = teamId;
            this.factories = factories;
            this.units = units;
            
            _factoryIsMine = factory => factory.teamId == this.teamId;
            _unitIsMine = unit => unit.teamId == this.teamId;
        }

        public int id => teamId;
            
        public int teamId { get; }

        private IEnumerable<IFactory> _myFactories;
        public IEnumerable<IFactory> myFactories => _myFactories ??= factories.Values.Where(_factoryIsMine);

        private IEnumerable<IUnit> _myUnits;
        public IEnumerable<IUnit> myUnits => _myUnits ??= units.Values.Where(_unitIsMine);

        public IReadOnlyDictionary<int, IFactory> factories { get; }
        public IReadOnlyDictionary<int, IUnit> units { get; }
    }
}
