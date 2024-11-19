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
        object data { get; set; } // custom data store
    }

    public interface IPositionedObject : IApiObject
    {
        int2 pos { get; }
    }
    
    public interface IUnit : IPositionedObject { }
    
    public interface IMyUnit : IUnit
    {
        Result Move();
        Result TurnLeft();
        Result TurnRight();
    }

    public interface IFactory : IPositionedObject { }
    
    public interface IMyFactory : IFactory
    {
        Result Produce();
    }

    public interface IModel : IApiObject
    {
        int teamId { get; }
        
        IMyFactory myFactory { get; }
        IEnumerable<IMyUnit> myUnits { get; }
        
        IEnumerable<IFactory> factories { get; }
        IEnumerable<IUnit> units { get; }
    }
}
