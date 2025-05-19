function findNearbyDeposit(pos, iSubstance)
{
    let nearest = undefined;
    let distanceToNearest = 1000000;
    for (let deposit of model.deposits)
    {
        if (deposit.substanceType == iSubstance)
        {
            let curDist = distance(deposit.pos, pos);
            if (distanceToNearest > curDist)
            {
                nearest = deposit;
                distanceToNearest = curDist;
            }
        }
    }

    return nearest;
}

function findNearbyEnemy(pos)
{
    let nearest = undefined;
    let distanceToNearest = 1000000;
    for (let enemyUnit of model.enemyUnits) 
    {
        let curDist = distance(enemyUnit.pos, pos);
        if (distanceToNearest > curDist)
        {
            nearest = enemyUnit;
            distanceToNearest = curDist;
        }
    }

    return nearest;
}

let workerParams =
{
    miningTier: 1,
    productionTier: 1,
    movementTier: 1,
    hullTier: 0,
    storageTier: 1,
}

let warriorParams =
{
    attackTier: 1,
    movementTier: 1,
    hullTier: 1,
}

let enemiesMidPos;
let energyDeposit;
let matterDeposit;

let workerCount;
let factoryCount;
let energyMinerCount;
let matterMinerCount;

let defencePos;

export function tick()
{
    if (model.myUnits.count == 0 || model.enemyUnits.count == 0)
        return;
    
    energyDeposit = findNearbyDeposit(model.myUnits[0].pos, Substance.Type.Energy);
    matterDeposit = findNearbyDeposit(model.myUnits[0].pos, Substance.Type.Matter);

    enemiesMidPos = new int2(0, 0);
    for (let enemyUnit of model.enemyUnits)
        enemiesMidPos += enemyUnit.pos;
    enemiesMidPos /= model.enemyUnits.count;

    let defencePosX = enemiesMidPos.x * 0.1 + (energyDeposit.pos.x * 0.9 + matterDeposit.pos.x * 0.9) / 2;
    let defencePosY = enemiesMidPos.y * 0.1 + (energyDeposit.pos.y * 0.9 + matterDeposit.pos.y * 0.9) / 2;
    defencePos = new int2(defencePosX, defencePosY);

    workerCount = 0;
    factoryCount = 0;
    energyMinerCount = 0;
    matterMinerCount = 0;
    for (let myUnit of model.myUnits)
    {
        if (!myUnit.canMove)
        {
            if (CanMineEnergy(myUnit))
                ++energyMinerCount;
            else if (CanMineMatter(myUnit))
                ++matterMinerCount;
            else
                ++factoryCount;
        }
        else if (myUnit.storage.capacityPerSubstance > 0)
            ++workerCount;
    }
    
    //log("workerCount: " + workerCount + ", factoryCount: " + factoryCount + ", energyMinerCount: " + energyMinerCount + ", matterMinerCount: " + matterMinerCount);

    for (let myUnit of model.myUnits)
    {
        if (!myUnit.canMove)
        {
            if (myUnit.Mine(energyDeposit) == Result.Ok)
                continue;
            
            if (myUnit.Mine(matterDeposit) == Result.Ok)
                continue;

            myUnit.Produce(warriorParams);
        }
        else if (myUnit.storage.capacityPerSubstance > 0)
        {
            tickWorker(myUnit);
        }
        else
            tickWarrior(myUnit);
    }
}

function CanMineEnergy(myUnit)
{
    let canMineEnergyResult = myUnit.CanMine(energyDeposit);
    return canMineEnergyResult == Result.Ok || canMineEnergyResult == Result.NotEnoughStorageCapacity;
}

function CanMineMatter(myUnit)
{
    let canMineMatterResult = myUnit.CanMine(matterDeposit);
    return canMineMatterResult == Result.Ok || canMineMatterResult == Result.NotEnoughStorageCapacity;
}

function tickWorker(myUnit)
{
    if (myUnit.busy)
        return;

    let requredProductionParams = workerCount < 2 ? workerParams : warriorParams;
    
    if (myUnit.Produce(requredProductionParams) == Result.Ok)
        return;
    
    for (let iSubstance = 0; iSubstance < Substance.TYPES_COUNT; ++iSubstance)
    {
        if (myUnit.storage.content[iSubstance] < model.ProductionCost(requredProductionParams, iSubstance))
        {
            let nearbyDeposit = findNearbyDeposit(myUnit.pos, iSubstance);

            if (myUnit.Mine(nearbyDeposit) == Result.Ok)
                return;

            myUnit.MoveTo(nearbyDeposit.pos);
            return;
        }
    }
}

function tickWarrior(myUnit)
{
    let nearbyEnemy = findNearbyEnemy(myUnit.pos);
    
    if (nearbyEnemy && myUnit.Attack(nearbyEnemy) == Result.Ok)
        return;
    
    let radius = model.myUnits.count * 2 + 8 - model.units.count;
    if (radius < 3)
        radius = 3;
    
    if (nearbyEnemy && distance(nearbyEnemy.pos, defencePos) < 2 * radius)
        myUnit.MoveTo(nearbyEnemy.pos);
    if (nearbyEnemy && distance(myUnit.pos, defencePos) < radius)
        myUnit.MoveTo(nearbyEnemy.pos);
    else
        myUnit.MoveTo(defencePos);
}