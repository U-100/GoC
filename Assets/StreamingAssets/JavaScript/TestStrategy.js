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
let deposits = {};
let workerCount;
let defencePos;

export function tick()
{
    if (model.myUnits.count == 0 || model.enemyUnits.count == 0)
        return;
    
    deposits[Substance.Type.Energy] ??= findNearbyDeposit(model.myUnits[0].pos, Substance.Type.Energy);
    deposits[Substance.Type.Matter] ??= findNearbyDeposit(model.myUnits[0].pos, Substance.Type.Matter);

    enemiesMidPos = new int2(0, 0);
    for (let enemyUnit of model.enemyUnits)
        enemiesMidPos += enemyUnit.pos;
    enemiesMidPos /= model.enemyUnits.count;

    workerCount = 0;
    for (let myUnit of model.myUnits)
        if (myUnit.config.storageCapacityPerSubstance > 0)
            ++workerCount;
    
    for (let iUnit = 0; iUnit < model.myUnits.Count; ++iUnit)
    {
        let myUnit = model.myUnits[iUnit];
        if (iUnit == 0)
            tickPrimalWorker(myUnit);
        else if (myUnit.config.storageCapacityPerSubstance > 0)
            tickWorker(myUnit);
        else
            tickTurret(myUnit);
    }
}

function tickPrimalWorker(myUnit)
{
    if (myUnit.busy)
        return;
    
    if (workerCount >= 10)
        return;

    if (myUnit.Produce("BotWorker") == Result.Ok)
        return;
    else
        tickFarmResourcesFor(myUnit, "BotWorker");
}

function tickWorker(myUnit)
{
    if (myUnit.busy)
        return;

    if (myUnit.CanProduce("BotTurret") == Result.Ok)
    {
        let enemy = findNearbyEnemy(myUnit.pos);
        
        if (
            enemy.config.damage > 0 && distance(enemy.pos, myUnit.pos) < 6
            || distance(enemy.pos, myUnit.pos) < 3
        )
            myUnit.Produce("BotTurret");
        else
            myUnit.MoveTo(enemiesMidPos);
    }
    else
        tickFarmResourcesFor(myUnit, "BotTurret");
}

function tickFarmResourcesFor(myUnit, productName)
{
    let unitConfig = model.myUnitConfigs[productName];
    
    if (unitConfig)
    {
        for (let iSubstance = 0; iSubstance < Substance.TYPES_COUNT; ++iSubstance)
        {
            if (myUnit.storage[iSubstance] < unitConfig.productionCost[iSubstance])
            {
                let nearbyDeposit = deposits[iSubstance];

                if (myUnit.Mine(nearbyDeposit) == Result.Ok)
                    return;

                myUnit.MoveTo(nearbyDeposit.pos);
                return;
            }
        }
    }
}

function tickTurret(myUnit)
{
    let nearbyEnemy = findNearbyEnemy(myUnit.pos);
    myUnit.Attack(nearbyEnemy);
}

// function tickWarrior(myUnit)
// {
//     let nearbyEnemy = findNearbyEnemy(myUnit.pos);
//    
//     if (nearbyEnemy && myUnit.Attack(nearbyEnemy) == Result.Ok)
//         return;
//    
//     let radius = model.myUnits.count * 2 + 8 - model.units.count;
//     if (radius < 3)
//         radius = 3;
//    
//     if (nearbyEnemy && distance(nearbyEnemy.pos, defencePos) < 2 * radius)
//         myUnit.MoveTo(nearbyEnemy.pos);
//     if (nearbyEnemy && distance(myUnit.pos, defencePos) < radius)
//         myUnit.MoveTo(nearbyEnemy.pos);
//     else
//         myUnit.MoveTo(defencePos);
// }