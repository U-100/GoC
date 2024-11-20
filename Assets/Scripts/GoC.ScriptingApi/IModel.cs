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

    public interface IUnitControl
    {
        Result Give(object target, Substance.Type substanceType, int amount);
        Result Move();
        Result TurnLeft();
        Result TurnRight();
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
    
    public static class SubstanceExtensions
    {
        public static int ToIndex(this Substance.Type substanceType) => substanceType - Substance.Type.Energy;
    }
    
    public interface IUnit
    {
        int id { get; } // unique id
        int teamId { get; } // owner team id
        int2 pos { get; }

        public IReadOnlyList<int> storage { get; }
        
        public IUnitControl control { get; }
    }
    
    public interface IFactoryControl
    {
        Result Produce();
    }

    public interface IFactory
    {
        int id { get; } // unique id
        int teamId { get; } // owner team id
        int2 pos { get; }

        public IFactoryControl control { get; }
    }
    
    public interface IModel
    {
        int id { get; } // unique id
        int teamId { get; } // owner team id

        IEnumerable<IFactory> myFactories { get; }
        IEnumerable<IUnit> myUnits { get; }
        
        IReadOnlyDictionary<int, IFactory> factories { get; }
        IReadOnlyDictionary<int, IUnit> units { get; }
    }
}
