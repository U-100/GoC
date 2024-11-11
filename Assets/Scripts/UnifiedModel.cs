using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

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

        private class EnemyWrapper : IUnit
        {
            private readonly IUnit _inner;
            public EnemyWrapper(IUnit inner) => _inner = inner;

            public int2 pos => _inner.pos;
        }
        
        private class MyUnitWrapper : IMyUnit
        {
            private readonly IMyUnit _inner;
            public MyUnitWrapper(IMyUnit inner) => _inner = inner;

            public int2 pos => _inner.pos;
            public void Move() => _inner.Move();
            public void TurnLeft() => _inner.TurnLeft();
            public void TurnRight() => _inner.TurnRight();
        }
        
        private class Model : IModel
        {
            public Model(string team, IReadOnlyList<IMyUnit> myUnits, IReadOnlyList<IUnit> enemies)
            {
                this.team = team;
                this.myUnits = myUnits.Select(x => new MyUnitWrapper(x)).ToList();
                this.enemies = enemies.Select(x => new EnemyWrapper(x)).ToList();
            }

            public string team { get; }
            public IReadOnlyList<IMyUnit> myUnits { get; }
            public IReadOnlyList<IUnit> enemies { get; }
        }
        
        public IModel red { get; }
        public IModel blue { get; }
    }
}
