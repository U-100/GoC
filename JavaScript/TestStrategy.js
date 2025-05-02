function findNearby(set, pos, maxDistance) {
    let nearest = undefined;
    let distanceToNearest = maxDistance;
    for (let elem of set) {
        let curDist = distance(elem.pos, pos);
        if (distanceToNearest > curDist) {
            nearest = elem;
            distanceToNearest = curDist;
        }
    }

    return nearest;
}

function findNearbyNonAttackedEnemy(pos)
{
    let nearest = undefined;
    let distanceToNearest = 1000000;
    for (let enemyUnit of model.enemyUnits) 
    {
        if (enemyUnit.attackers >= enemyUnit.hull.integrity)
            continue;
        
        let curDist = distance(enemyUnit.pos, pos);
        if (distanceToNearest > curDist)
        {
            nearest = enemyUnit;
            distanceToNearest = curDist;
        }
    }

    return nearest;
}

function wander(myUnit)
{
    let destination = [myUnit.pos.x - 1 + Math.floor(Math.random() * 3), myUnit.pos.y - 1 + Math.floor(Math.random() * 3)];
    myUnit.MoveTo(destination);
}

let productionParams =
{
    attackTier: 1,
    miningTier: 1,
    productionTier: 1,
    movementTier: 1,
    hullTier: 1,
    storageTier: 1
}

log("Production ticks: " + model.ProductionTicks(productionParams));

export function tick()
{
    for (let enemyUnit of model.enemyUnits)
        enemyUnit.attackers = 0;
    
    for (let iUnit = 0; iUnit < model.myUnits.Count; ++iUnit)
    {
        let myUnit = model.myUnits[iUnit];

        if (myUnit.busy)
            continue;
        
        //let nearbyEnemyUnit = findNearby(model.enemyUnits, myUnit.pos, 100000);
        let nearbyEnemyUnit = findNearbyNonAttackedEnemy(myUnit.pos);
        if (nearbyEnemyUnit)
            if (myUnit.Attack(nearbyEnemyUnit) == Result.Ok)
            {
                nearbyEnemyUnit.attackers = nearbyEnemyUnit.attackers + 1;
                continue;
            }
        
        if (myUnit.storage.content[Substance.Type.Energy] >= 2)
            if (myUnit.Produce(productionParams) == Result.Ok)
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

        if (nearbyEnemyUnit)
        {
            myUnit.MoveTo(nearbyEnemyUnit.pos);
            continue;
        }

        wander(myUnit);
    }
}
