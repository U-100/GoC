using System.Collections.Generic;
using Unity.Mathematics;

namespace GoC
{
    public enum Result
    {
        Ok,
        Busy,
        AccessDenied,
    }

    public interface IApiObject
    {
        int id { get; } // unique id
        int teamId { get; } // owner team id
    }

    public interface IPositionedObject : IApiObject
    {
        int2 pos { get; }
    }

    public interface IUnitControl
    {
        Result Move();
        Result TurnLeft();
        Result TurnRight();
    }
    
    public interface IUnit : IPositionedObject
    {
        public IUnitControl control { get; }
    }
    
    public interface IFactoryControl
    {
        Result Produce();
    }

    public interface IFactory : IPositionedObject
    {
        public IFactoryControl control { get; }
    }

    public interface IModel : IApiObject
    {
        IEnumerable<IFactory> myFactories { get; }
        IEnumerable<IUnit> myUnits { get; }
        
        IReadOnlyDictionary<int, IFactory> factories { get; }
        IReadOnlyDictionary<int, IUnit> units { get; }
    }
}
