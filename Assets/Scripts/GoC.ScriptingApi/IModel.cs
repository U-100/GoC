using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace GoC.ScriptingApi
{
    public enum Result
    {
        Ok,
        Busy,
        AccessDenied,
        InvalidArgument,
        TooFar,
        InvalidTarget,
        NotEnoughSubstance,
    }

    public class Substance
    {
        public enum Type
        {
            Energy,
            Matter,
        }

        public static readonly int TYPES_COUNT = Enum.GetValues(typeof(Type)).Length;
    }

    public interface IUnitControl
    {
        Result Mine(object target);
        Result Give(object target, Substance.Type substanceType, int amount);
        Result Move();
        Result TurnLeft();
        Result TurnRight();
    }
    
    public interface IUnit
    {
        int teamId { get; } // owner team id
        int2 pos { get; }
        int2 dir { get; }

        public IReadOnlyList<int> storage { get; }
        
        public IUnitControl control { get; }
    }
    
    public interface IFactoryControl
    {
        Result Produce();
    }

    public interface IFactory
    {
        int teamId { get; } // owner team id
        int2 pos { get; }

        public IReadOnlyList<int> storage { get; }
        
        public IFactoryControl control { get; }
    }

    public interface IDeposit
    {
        int2 pos { get; }
    }
    
    public interface IModel
    {
        int teamId { get; } // team id

        IEnumerable<IFactory> myFactories { get; }
        IEnumerable<IUnit> myUnits { get; }
        
        IEnumerable<IFactory> factories { get; }
        IEnumerable<IUnit> units { get; }
        IEnumerable<IDeposit> deposits { get; }
    }
}
