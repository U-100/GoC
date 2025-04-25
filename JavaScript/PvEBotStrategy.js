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
    let nearbyDeposit = undefined;
    if (model.myUnits.count > 0)
        nearbyDeposit = findNearby(model.deposits, model.myUnits[0].pos, 100000);

    for (let iUnit = 0; iUnit < model.myUnits.count; ++iUnit)
    {
        let myUnit = model.myUnits[iUnit];

        if (myUnit.busy)
            continue;

        if (iUnit === 0)
        {
            if (nearbyDeposit && myUnit.pos !== nearbyDeposit.pos)
            {
                myUnit.moveTo(nearbyDeposit.pos);
                continue;
            }

            let pp =
            [
                nearbyDeposit.pos + new int2(-1, 0),
                nearbyDeposit.pos + new int2(1, 0),
                nearbyDeposit.pos + new int2(0, -1),
                nearbyDeposit.pos + new int2(0, 1)
            ];
            
            if (model.myUnits.count < 5)
            {
                let miner = { miningTier: 1, productionTier: 1, storageTier: 1 };

                myUnit.produce(miner, pp[model.myUnits.count - 1]) == Result.Ok || myUnit.mine(nearbyDeposit);
                continue;
            }
        }
        else if (model.myUnits.count < 5)
        {
            myUnit.mine(nearbyDeposit);
        }
        else if (myUnit.storage.capacityPerSubstance > 0)
        {
            let warrior = { attackTier: 1, movementTier: 1, hullTier: 1 };

            myUnit.produce(warrior) == Result.Ok || myUnit.mine(nearbyDeposit);
            continue;
        }
        else
        {
            let nearbyEnemyUnit = findNearby(model.enemyUnits, myUnit.pos, 100000);

            if (nearbyEnemyUnit)
            {
                myUnit.attack(nearbyEnemyUnit) == Result.Ok || myUnit.moveTo(nearbyEnemyUnit.pos);
                continue;
            }
        }
    }
}
