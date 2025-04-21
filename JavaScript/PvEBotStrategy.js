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

export function tick()
{
    for (let iUnit = 0; iUnit < model.myUnits.count; ++iUnit)
    {
        let myUnit = model.myUnits[iUnit];

        if (myUnit.busy)
            continue;

        if (myUnit.storage.capacityPerSubstance > 0)
        {
            if (
                model.myUnits.count < 4 && iUnit < 2
                || model.myUnits.count < 5 && iUnit == 0
            )
            {
                let peasant = { miningTier: 1, productionTier: 1, movementTier: 1, storageTier: 1 };

                if (myUnit.produce(peasant) == Result.Ok)
                    continue;
            }
            else
            {
                let warrior = { attackTier: 1, movementTier: 1, hullTier: 1 };

                if (myUnit.produce(warrior) == Result.Ok)
                    continue;
            }

            let nearbyDeposit = findNearby(model.deposits, myUnit.pos, 100000);
            if (nearbyDeposit)
            {
                if (iUnit == 0)
                {
                    let requiredPos = nearbyDeposit.pos + new int2(-1, 0);
                    if (myUnit.pos !== requiredPos)
                    {
                        myUnit.moveTo(requiredPos);
                        continue;
                    }
                }
                else if (iUnit == 1)
                {
                    let requiredPos = nearbyDeposit.pos + new int2(0, -1);
                    if (myUnit.pos !== requiredPos)
                    {
                        myUnit.moveTo(requiredPos);
                        continue;
                    }
                }

                if (myUnit.mine(nearbyDeposit) == Result.Ok)
                    continue;
                else
                {
                    myUnit.moveTo(nearbyDeposit.pos);
                    continue;
                }
            }
        }
        else if (model.myUnits.count > 10 && iUnit > 9)
        {
            let nearbyEnemyUnit = findNearby(model.enemyUnits, myUnit.pos, 100000);
            if (nearbyEnemyUnit)
            {
                if (myUnit.attack(nearbyEnemyUnit) == Result.Ok)
                    continue;
                else
                    myUnit.moveTo(nearbyEnemyUnit.pos);
            }            
        }
    }
}
