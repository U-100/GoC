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
        NotEnoughSubstance,
        TooFar,
    }

    public interface IUnitControl
    {
        Result Move();
        Result TurnLeft();
        Result TurnRight();
    }

    public static class Substance
    {
        public enum Type
        {
            Energy,
            Matter,
        }

        public static int ToIndex(this Substance.Type substanceType) => substanceType - Substance.Type.Energy;
        
        public static readonly int count = Enum.GetValues(typeof(Type)).Length;
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
