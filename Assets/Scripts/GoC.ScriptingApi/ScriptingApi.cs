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
        OperationUnavailable,
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
    
    public interface IUnit
    {
        int teamId { get; } // owner team id
        int2 pos { get; }
        int2 dir { get; }
        bool busy { get; }
        
        Result CanMine(object target);
        Result CanProduce(object productionParamsObject);

        public IHull hull { get; }
        public IStorage storage { get; }
    }
    
    public interface IUnitControl : IUnit
    {
        Result Attack(object target);
        Result Mine(object target);
        Result Give(object target, Substance.Type substanceType, int amount);
        Result Produce(object productionParamsObject);
        Result Produce(object productionParamsObject, int2 p);
        Result Produce(object productionParamsObject, int x, int y);
        Result Produce(object productionParamsObject, int[] p);
        Result MoveTo(int2 p);
        Result MoveTo(int x, int y);
        Result MoveTo(int[] p);
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

        IReadOnlyList<IUnitControl> myUnits { get; }
        IReadOnlyList<IUnit> units { get; }
        IReadOnlyList<IUnit> enemyUnits { get; }
        
        IReadOnlyList<IDeposit> deposits { get; }
    }
}
