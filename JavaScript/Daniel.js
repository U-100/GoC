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


function enemyCloudMiddle()
{
    if (model.enemyUnits.Count > 0)
    {
        let midPoint = new int2(0, 0);

        for (let enemyUnit of model.enemyUnits)
            midPoint = midPoint + enemyUnit.pos;

        return midPoint / model.enemyUnits.Count;
    }
    
    return [0, 0];
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

var nearbyDeposit = undefined;

let instructions =
    [
        /*0*/{path: [[-1, 0]]},
        /*1*/{path: [[0, -1]]},
        /*2*/{path: [[0, 0]]},
        /*3*/{path: [[1, 0]]},
        /*4*/{path: [[0, 1]]},
        /*5*/{path: [[1, 1]]},
        /*6*/{path: [[1, -2], [-1, -2], [-1, -1]]},
        /*7*/{path: [[1, -1]]},
        /*8*/{path: [[-1, 1]]}
    ];


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

        if (!nearbyDeposit)
            nearbyDeposit = findNearby(model.deposits, myUnit.pos, 100000);

        if (iUnit < instructions.length)
        {
            let instruction = instructions[iUnit];

            let requiredPos = [nearbyDeposit.pos.x + instruction.path[instruction.path.length - 1][0], nearbyDeposit.pos.y + instruction.path[instruction.path.length - 1][1]];
            if (myUnit.pos.x != requiredPos[0] || myUnit.pos.y != requiredPos[1])
            {
                let iPoint = instruction.path.length - 1;
                for (; iPoint >= 0; --iPoint)
                {
                    let requiredPos = [nearbyDeposit.pos.x + instruction.path[iPoint][0], nearbyDeposit.pos.y + instruction.path[iPoint][1]];


                    if (myUnit.MoveTo(requiredPos) === Result.Ok)
                        break;
                }
                if (iPoint < 0)
                    log("ohohohoh " + iUnit);
            }
        }

        if (myUnit.Produce() == Result.Ok)
            continue;

        if (myUnit.Mine(nearbyDeposit) == Result.Ok)
            continue;

        if (iUnit >= instructions.length)
        {
            let midPoint = enemyCloudMiddle();
            myUnit.MoveTo(midPoint);
        }
    }
}