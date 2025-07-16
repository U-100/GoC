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

    public interface IUnit
    {
        int teamId { get; } // owner team id
        int2 pos { get; }
        bool busy { get; }

        int integrity { get; }

        int storageCapacityPerSubstance { get; }
        IReadOnlyList<int> storage { get; }
        
        bool canMove { get; }
        bool canMine { get; }
        int damage { get; }
        int maxProductionTier { get; }

        Result CanMine(object target);
        Result CanProduce(object unitParamsObject);
    }
    
    public interface IUnitControl : IUnit
    {
        Result Say(string speech);
        Result Attack(object target);
        Result Mine(object target);
        Result Give(object target, Substance.Type substanceType, int amount);
        Result Produce(object unitParamsObject);
        Result Produce(object unitParamsObject, int2 p);
        Result Produce(object unitParamsObject, int x, int y);
        Result Produce(object unitParamsObject, int[] p);
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
        IReadOnlyList<IUnit> enemyUnits { get; }
        
        IReadOnlyList<IDeposit> deposits { get; }
        
        int ProductionCost(object unitParamsObject, Substance.Type substanceType);
        int ProductionTicks(object unitParamsObject);
    }
}
