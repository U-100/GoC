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
    for (let iUnit = 0; iUnit < model.myUnits.Count; ++iUnit)
    {
        let myUnit = model.myUnits[iUnit];

        if (myUnit.busy)
            continue;

        let nearbyEnemyUnit = findNearby(model.enemyUnits, myUnit.pos, 100000);
        if (nearbyEnemyUnit)
            if (myUnit.Attack(nearbyEnemyUnit) == Result.Ok)
                continue;

        if (myUnit.storage.content[Substance.Type.Energy] >= 2)
            if (myUnit.Produce() == Result.Ok)
                continue;

        let nearbyDeposit = findNearby(model.deposits, myUnit.pos, 100000);
        if (nearbyDeposit)
        {
            if (iUnit == 0 && myUnit.pos !== nearbyDeposit.pos)
            {
                myUnit.MoveTo(nearbyDeposit.pos);
                continue;
            }

            if (myUnit.Mine(nearbyDeposit) == Result.Ok)
                continue;
            else if (iUnit < 2)
            {
                myUnit.MoveTo(nearbyDeposit.pos);
                continue;
            }
        }

        if (nearbyEnemyUnit && iUnit > 10)
            myUnit.MoveTo(nearbyEnemyUnit.pos);
    }
}
