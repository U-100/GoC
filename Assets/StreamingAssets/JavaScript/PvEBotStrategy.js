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

let enemiesMidPos;
let energyDeposit;
let matterDeposit;

let defencePos;

let workerCount;

export function tick()
{
    if (model.myUnits.count == 0 || model.enemyUnits.count == 0)
        return;

    energyDeposit = findNearbyDeposit(model.myUnits[0].pos, Substance.Type.Energy);
    matterDeposit = findNearbyDeposit(model.myUnits[0].pos, Substance.Type.Matter);

    enemiesMidPos = new int2(0, 0);
    for (let enemyUnit of model.enemyUnits)
        enemiesMidPos += enemyUnit.pos;
    enemiesMidPos /= model.enemyUnits.count;

    let defencePosX = enemiesMidPos.x * 0.1 + (energyDeposit.pos.x * 0.9 + matterDeposit.pos.x * 0.9) / 2;
    let defencePosY = enemiesMidPos.y * 0.1 + (energyDeposit.pos.y * 0.9 + matterDeposit.pos.y * 0.9) / 2;
    defencePos = new int2(defencePosX, defencePosY);

    workerCount = 0;
    for (let myUnit of model.myUnits)
        if (myUnit.config.canMine)
            ++workerCount;

    for (let myUnit of model.myUnits)
    {
        if (myUnit.config.canMine)
            tickWorker(myUnit);
        else
            tickWarrior(myUnit);
    }
}

function tickWorker(myUnit)
{
    if (myUnit.busy)
        return;

    let requredProduct = workerCount < 5 ? "BotWorker" : "BotFighter";

    if (myUnit.Produce(requredProduct) == Result.Ok)
        return;

    let unitConfig = model.myUnitConfigs[requredProduct];
    
    if (!unitConfig)
    {
        log("Couldn't get unit config for " + requredProduct);
        return;
    }
    
    for (let iSubstance = 0; iSubstance < Substance.TYPES_COUNT; ++iSubstance)
    {
        if (myUnit.storage[iSubstance] < unitConfig.productionCost[iSubstance])
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
        return;
    }
}