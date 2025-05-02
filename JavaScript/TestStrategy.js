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

let warriorParams =
{
    attackTier: 1,
    movementTier: 1,
    hullTier: 1,
}

export function tick()
{
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

    if (myUnit.Produce(warriorParams) == Result.Ok)
        return;
    
    for (let iSubstance = 0; iSubstance < Substance.TYPES_COUNT; ++iSubstance)
    {
        if (myUnit.storage.content[iSubstance] < model.ProductionCost(warriorParams, iSubstance))
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
    }
}