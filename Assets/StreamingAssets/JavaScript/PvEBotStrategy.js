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

let defencePos;

let workerCount;

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
    for (let myUnit of model.myUnits)
    {
        if (myUnit.storageCapacityPerSubstance > 0)
            ++workerCount;
    }

    for (let myUnit of model.myUnits)
    {
        if (myUnit.storageCapacityPerSubstance > 0)
            tickWorker(myUnit);
        else
            tickWarrior(myUnit);
    }
}

function tickWorker(myUnit)
{
    if (myUnit.busy)
        return;

    let requredProductionParams = workerCount < 5 ? workerParams : warriorParams;

    if (myUnit.Produce(requredProductionParams) == Result.Ok)
        return;

    for (let iSubstance = 0; iSubstance < Substance.TYPES_COUNT; ++iSubstance)
    {
        if (myUnit.storage[iSubstance] < model.ProductionCost(requredProductionParams, iSubstance))
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

    if (nearbyEnemy)
    {
        if (myUnit.Attack(nearbyEnemy) == Result.Ok)
            return;

        myUnit.MoveTo(nearbyEnemy.pos);
        return;
    }
}