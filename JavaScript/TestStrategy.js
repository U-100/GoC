function alwaysTrue(_) { return true; }

function findNearby(set, pos, maxDistance, predicate = alwaysTrue) {
    let nearest = undefined;
    let distanceToNearest = maxDistance;
    for (let elem of set) {
        if(!predicate(elem))
            continue;
        
        let curDist = distance(elem.pos, pos);
        if (distanceToNearest > curDist) {
            nearest = elem;
            distanceToNearest = curDist;
        }
    }

    return nearest;
}

let isEnemy = (x) => x.teamId != model.teamId;

function wander(myUnit)
{
    let destination = [myUnit.pos.x - 1 + Math.floor(Math.random() * 3), myUnit.pos.y - 1 + Math.floor(Math.random() * 3)];
    myUnit.control.MoveTo(destination);
}

export function tick() {
    let myUnitsCount = 0;

    for (let myUnit of model.myUnits) {
        ++myUnitsCount;

        if(!myUnit.started || myUnit.started < 5)
        {
            wander(myUnit);
            myUnit.started = (myUnit.started ?? 0) + 1;
            continue;
        }

        let nearbyEnemyUnit = findNearby(model.units, myUnit.pos, 3, isEnemy);
        if (nearbyEnemyUnit) {
            if (myUnit.control.Attack(nearbyEnemyUnit) != Result.Ok)
                myUnit.control.MoveTo(nearbyEnemyUnit);
            continue;
        }

        if (myUnit.storage.content[Substance.Type.Energy] >= 4)
        {
            let nearbyMyFactory = findNearby(model.myFactories, myUnit.pos, 10000);
            if (nearbyMyFactory) {
                if (myUnit.control.Give(nearbyMyFactory, Substance.Type.Energy, myUnit.storage.content[Substance.Type.Energy]) != Result.Ok)
                    myUnit.control.MoveTo(nearbyMyFactory);
                continue;
            }
        }

        if (myUnitsCount <= 4) {
            let nearbyDeposit = findNearby(model.deposits, myUnit.pos, 100000);
            if (nearbyDeposit) {
                if (myUnit.control.Mine(nearbyDeposit) != Result.Ok)
                    myUnit.control.MoveTo(nearbyDeposit);
                continue;
            }
        }

        let nearbyEnemyFactory = findNearby(model.factories, myUnit.pos, 100000, isEnemy);
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
