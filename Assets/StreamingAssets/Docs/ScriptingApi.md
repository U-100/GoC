Enum Result
  Result.Ok = 0
  Result.Busy = 1
  Result.AccessDenied = 2
  Result.OperationUnavailable = 3
  Result.InvalidArgument = 4
  Result.OutOfRange = 5
  Result.InvalidTarget = 6
  Result.NotEnoughSubstance = 7
  Result.NotEnoughStorageCapacity = 8
  Result.AlreadyThere = 9
  Result.PathBlocked = 10

class Substance
  Int32 TYPES_COUNT

interface IUnitConfig
  String name
  IReadOnlyList<Int32> productionCost
  Int32 productionTicks
  Boolean canMove
  Int32 maxIntegrity
  Int32 storageCapacityPerSubstance
  Int32 damage
  Boolean canMine
  IReadOnlyList<IUnitConfig> productConfigs

interface IUnit
  Result CanMine(Object target)
  Result CanProduce(String productName)
  Int32 teamId
  int2 pos
  Boolean busy
  IUnitConfig config
  Int32 integrity
  IReadOnlyList<Int32> storage

interface IUnitControl
  Result Say(String speech)
  Result Attack(Object target)
  Result Mine(Object target)
  Result Give(Object target, Substance.Type substanceType, Int32 amount)
  Result Produce(String productName)
  Result Produce(String productName, int2 p)
  Result Produce(String productName, Int32 x, Int32 y)
  Result Produce(String productName, Int32[] p)
  Result MoveTo(int2 p)
  Result MoveTo(Int32 x, Int32 y)
  Result MoveTo(Int32[] p)

interface IDeposit
  int2 pos
  Substance.Type substanceType
  Int32 miningSpeed

interface IModel
  Int32 teamId
  IReadOnlyDictionary<String,IUnitConfig> myUnitConfigs
  IReadOnlyList<IUnitControl> myUnits
  IReadOnlyList<IUnit> enemyUnits
  IReadOnlyList<IDeposit> deposits

Enum Substance.Type
  Substance.Type.Energy = 0
  Substance.Type.Matter = 1

