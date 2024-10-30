using System.Collections.Generic;

namespace GoC
{
    public class UnifiedModel
    {
        private readonly List<IMyUnit> _redUnits;
        private readonly List<IMyUnit> _blueUnits;

        public UnifiedModel(IMyUnit redUnit, IMyUnit blueUnit)
        {
            _redUnits = new List<IMyUnit> { redUnit };
            _blueUnits = new List<IMyUnit> { blueUnit };

            red = new Model("red", _redUnits, _blueUnits);
            blue = new Model("blue", _blueUnits, _redUnits);
        }

        private class Model : IModel
        {
            public Model(string team, IReadOnlyList<IMyUnit> myUnits, IReadOnlyList<IUnit> enemies)
            {
                this.team = team;
                this.myUnits = myUnits;
                this.enemies = enemies;
            }

            public string team { get; }
            public IReadOnlyList<IMyUnit> myUnits { get; }
            public IReadOnlyList<IUnit> enemies { get; }
        }
        
        public IModel red { get; }
        public IModel blue { get; }
    }
}
