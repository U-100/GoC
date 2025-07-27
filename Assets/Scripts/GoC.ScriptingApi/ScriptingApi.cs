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

    public interface IUnitParams
    {
        string name { get; }
        
        IReadOnlyList<int> productionCost { get; }
        int productionTicks { get; }
        
        bool canMove { get; }
        int maxIntegrity { get; }
        int storageCapacityPerSubstance { get; }
        int damage { get; }
        bool canMine { get; }
        IReadOnlyList<IUnitParams> products { get; }
    }
    
    public interface IUnit
    {
        int teamId { get; } // owner team id
        int2 pos { get; }
        bool busy { get; }

        IUnitParams unitParams { get; }

        int integrity { get; }
        IReadOnlyList<int> storage { get; }

        Result CanMine(object target);
        Result CanProduce(string productName);
    }
    
    public interface IUnitControl : IUnit
    {
        Result Say(string speech);
        Result Attack(object target);
        Result Mine(object target);
        Result Give(object target, Substance.Type substanceType, int amount);
        Result Produce(string productName);
        Result Produce(string productName, int2 p);
        Result Produce(string productName, int x, int y);
        Result Produce(string productName, int[] p);
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

        IReadOnlyDictionary<string, IUnitParams> myUnitParams { get; }
        
        IReadOnlyList<IUnitControl> myUnits { get; }
        IReadOnlyList<IUnit> enemyUnits { get; }
        
        IReadOnlyList<IDeposit> deposits { get; }
    }
}
