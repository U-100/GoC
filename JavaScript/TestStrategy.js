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

function wander(myUnit)
{
    let destination = [myUnit.pos.x - 1 + Math.floor(Math.random() * 3), myUnit.pos.y - 1 + Math.floor(Math.random() * 3)];
    myUnit.control.MoveTo(destination);
}

export function tick() {
    let myFactory = model.myFactories.Count > 0 ? model.myFactories[0] : undefined;
    let nearbyEnemyFactory = myFactory ? findNearby(model.enemyFactories, myFactory.pos, 100000) : undefined;
    
    for (let iUnit = 0; iUnit < model.myUnits.Count; ++iUnit) {
        let myUnit = model.myUnits[iUnit];
        
        if (myUnit.busy)
            continue;
        
        let nearbyEnemyUnit = findNearby(model.enemyUnits, myUnit.pos, 3);
        if (nearbyEnemyUnit) {
            if (myUnit.control.Attack(nearbyEnemyUnit) != Result.Ok)
                myUnit.control.MoveTo(nearbyEnemyUnit);
            continue;
        }
        
        if (myUnit.storage.content[Substance.Type.Energy] >= 2)
            if (myUnit.control.Produce() == Result.Ok)
                continue;

        let nearbyDeposit = findNearby(model.deposits, myUnit.pos, 100000);
        if (nearbyDeposit)
        {
            if (myUnit.control.Mine(nearbyDeposit) == Result.Ok)
                continue;
            else if (iUnit < 2) {
                myUnit.control.MoveTo(nearbyDeposit);
                continue;
            }
        }

        if (nearbyEnemyFactory) {
            if (myUnit.control.Attack(nearbyEnemyFactory) != Result.Ok)
                myUnit.control.MoveTo(nearbyEnemyFactory);
            continue;
        }

        wander(myUnit);
    }

    for (let myFactory of model.myFactories)
        myFactory.control.Produce();
}
