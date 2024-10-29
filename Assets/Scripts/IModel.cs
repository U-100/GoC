using System.Collections.Generic;

namespace GoC
{
    public interface IUnit
    {
        void Move();
        void TurnLeft();
        void TurnRight();
    }

    public interface IModel
    {
        string team { get; }
        
        IReadOnlyList<IUnit> units { get; }
    }
}
