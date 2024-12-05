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
        OutOfRange,
        InvalidTarget,
        NotEnoughSubstance,
        NotEnoughStorageCapacity,
        AlreadyThere,
        PathBlocked
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

    public interface IStorage
    {
        public int capacityPerSubstance { get; }
        public IReadOnlyList<int> content { get; }
    }

    public interface IHull
    {
        public int integrity { get; }
    }
    
    public interface IUnitControl
    {
        Result Attack(object target);
        Result Mine(object target);
        Result Give(object target, Substance.Type substanceType, int amount);
        Result MoveTo(object target);
        Result MoveTo(int x, int y);
    }
    
    public interface IUnit
    {
        int teamId { get; } // owner team id
        int2 pos { get; }
        int2 dir { get; }
        
        Result CanMine(object target);

        public IHull hull { get; }
        public IStorage storage { get; }
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

        public IHull hull { get; }
        public IStorage storage { get; }
        public IFactoryControl control { get; }
    }

    public interface IDeposit
    {
        int2 pos { get; }
        public Substance.Type substanceType { get; }
        public int miningSpeed { get; }
    }
    
    public interface IModel
    {
        int teamId { get; } // team id

        IReadOnlyList<IFactory> myFactories { get; }
        IReadOnlyList<IUnit> myUnits { get; }
        
        IEnumerable<IFactory> factories { get; }
        IEnumerable<IUnit> units { get; }
        
        IReadOnlyList<IDeposit> deposits { get; }
    }
}
