function findNearby(set, pos, maxDistance)
{
    let nearest = undefined;
    let distanceToNearest = maxDistance;
    for (let elem of set)
    {
        let curDist = distance(elem.pos, pos);
        if (distanceToNearest > curDist)
        {
            nearest = elem;
            distanceToNearest = curDist;
        }
    }

    return nearest;
}

let productionParams =
{
    movementTier: 1,
    hullTier: 0,
    storageTier: 1
}

export function tick()
{
    for (let iUnit = 0; iUnit < model.myUnits.count; ++iUnit)
    {
        let myUnit = model.myUnits[iUnit];

        if (myUnit.busy)
            continue;

        let nearbyEnemyUnit = findNearby(model.enemyUnits, myUnit.pos, 100000);
        if (nearbyEnemyUnit)
            if (myUnit.attack(nearbyEnemyUnit) == Result.Ok)
                continue;

        if (myUnit.storage.content[Substance.Type.Energy] >= 2)
            if (myUnit.produce(productionParams) == Result.Ok)
                continue;

        let nearbyDeposit = findNearby(model.deposits, myUnit.pos, 100000);
        if (nearbyDeposit)
        {
            if (iUnit == 0 && myUnit.pos !== nearbyDeposit.pos)
            {
                myUnit.moveTo(nearbyDeposit.pos);
                continue;
            }

            if (myUnit.mine(nearbyDeposit) == Result.Ok)
                continue;
            else if (iUnit < 2)
            {
                myUnit.moveTo(nearbyDeposit.pos);
                continue;
            }
        }

        if (nearbyEnemyUnit && iUnit > 10)
            myUnit.moveTo(nearbyEnemyUnit.pos);
    }
}
