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
    for (let iUnit = 0; iUnit < model.myUnits.Count; ++iUnit) {
        let myUnit = model.myUnits[iUnit];
        
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

        if (iUnit < 2) {
            let nearbyDeposit = findNearby(model.deposits, myUnit.pos, 100000);
            if (nearbyDeposit) {
                let miningResult = myUnit.control.Mine(nearbyDeposit);
                if (miningResult != Result.Ok)
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
