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

let turretParams =
{
    attackTier: 1,
    hullTier: 1,
}

let enemiesMidPos;
let energyDeposit;
let matterDeposit;

let workerCount;
let warriorCount;
let turretCount;

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
    warriorCount = 0;
    turretCount = 0;
    for (let myUnit of model.myUnits)
    {
        if (!myUnit.canMove)
            ++turretCount;
        else if (myUnit.storage.capacityPerSubstance > 0)
            ++workerCount;
        else
            ++warriorCount;
    }
    
    //log("workerCount: " + workerCount + ", warriorCount: " + warriorCount + ", turretCount: " + turretCount);

    for (let myUnit of model.myUnits)
    {
        if (myUnit.storage.capacityPerSubstance > 0)
            tickWorker(myUnit);
        else
            tickWarrior(myUnit);
    }
}

function tickWorker(myUnit)
{
    if (myUnit.busy)
        return;

    if (workerCount < 3)
    {
        if (myUnit.Produce(workerParams) == Result.Ok)
            return;
        else
            tickFarmResourcesFor(myUnit, workerParams);
    }
    else if (turretCount < 10)
    {
        if (myUnit.CanProduce(turretParams) == Result.Ok)
        {
            if (distance(myUnit.pos, defencePos) > 2)
            {
                myUnit.MoveTo(defencePos);
                return;
            }
            else
            {
                myUnit.Produce(turretParams);
                return;
            }
        }
        else
            tickFarmResourcesFor(myUnit, turretParams);
    }
    else if (myUnit.Produce(warriorParams) != Result.Ok)
        tickFarmResourcesFor(myUnit, warriorParams);
}

function tickFarmResourcesFor(myUnit, productionParams)
{
    for (let iSubstance = 0; iSubstance < Substance.TYPES_COUNT; ++iSubstance)
    {
        if (myUnit.storage.content[iSubstance] < model.ProductionCost(productionParams, iSubstance))
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