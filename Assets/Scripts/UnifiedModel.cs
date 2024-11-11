using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace GoC
{
    public class UnifiedModel
    {
        private readonly List<IMyUnit> _redUnits;
        private readonly List<IMyUnit> _blueUnits;

        public UnifiedModel(Factory redFactory, Factory blueFactory)
        {
            _redUnits = new List<IMyUnit>();
            _blueUnits = new List<IMyUnit>();
            
            redFactory.Init(AddRed);
            blueFactory.Init(AddBlue);

            redModel = new Model("red", redFactory, _redUnits, blueFactory, _blueUnits);
            blueModel = new Model("blue", blueFactory, _blueUnits, redFactory, _redUnits);
        }

        private void AddRed(IMyUnit unit) => _redUnits.Add(unit);
        private void AddBlue(IMyUnit unit) => _blueUnits.Add(unit);
        
        private class EnemyWrapper : IUnit
        {
            private readonly IUnit _inner;
            public EnemyWrapper(IUnit inner) => _inner = inner;

            public int id => _inner.id;
            public int2 pos => _inner.pos;
        }
        
        private class MyUnitWrapper : IMyUnit
        {
            private readonly IMyUnit _inner;
            public MyUnitWrapper(IMyUnit inner) => _inner = inner;

            public int id => _inner.id;
            public int2 pos => _inner.pos;
            public void Move() => _inner.Move();
            public void TurnLeft() => _inner.TurnLeft();
            public void TurnRight() => _inner.TurnRight();
        }

        private class Model : IModel
        {
            public Model(string team, IMyFactory myFactory, IReadOnlyList<IMyUnit> myUnits, IFactory enemyFactory, IReadOnlyList<IUnit> enemyUnits)
            {
                this.team = team;
                this.myFactory = myFactory;
                this.myUnits = myUnits;//.Select(x => new MyUnitWrapper(x)).ToList();
                this.enemyFactory = enemyFactory;
                this.enemyUnits = enemyUnits;//.Select(x => new EnemyWrapper(x)).ToList();
            }

            public string team { get; }
            
            public IMyFactory myFactory { get; }
            public IReadOnlyList<IMyUnit> myUnits { get; }
            
            public IFactory enemyFactory { get; set; }
            public IReadOnlyList<IUnit> enemyUnits { get; }
        }
        
        public IModel redModel { get; }
        public IModel blueModel { get; }
    }
}
