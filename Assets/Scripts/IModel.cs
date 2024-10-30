using System.Collections.Generic;
using Unity.Mathematics;

namespace GoC
{
    public interface IUnit
    {
        int2 pos { get; }
    }
    
    public interface IMyUnit : IUnit
    {
        void Move();
        void TurnLeft();
        void TurnRight();
    }

    public interface IModel
    {
        string team { get; }
        
        IReadOnlyList<IMyUnit> myUnits { get; }
        IReadOnlyList<IUnit> enemies { get; }
    }
}
