using System.Collections.Generic;
using Unity.Mathematics;

namespace GoC
{
    public interface IUnit
    {
        int id { get; }
        int2 pos { get; }
    }
    
    public interface IMyUnit : IUnit
    {
        void Move();
        void TurnLeft();
        void TurnRight();
    }

    public interface IFactory
    {
        int id { get; }
        int2 pos { get; }
    }

    public enum Result
    {
        Ok,
        Busy,
    }
    
    public interface IMyFactory : IFactory
    {
        Result Produce();
    }

    public interface IModel
    {
        int teamId { get; }
        
        IMyFactory myFactory { get; }
        IEnumerable<IMyUnit> myUnits { get; }
        
        IEnumerable<IFactory> factories { get; }
        IEnumerable<IUnit> units { get; }
    }
}
