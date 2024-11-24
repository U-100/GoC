function findEnemyNearby(p) {
    let enemy = undefined;
    let manhattanDistanceToEnemy = 100000;
    for (let unit of model.units) {
        if (unit.teamId != model.teamId) {
            if (!enemy) {
                enemy = unit;
                manhattanDistanceToEnemy = Math.abs(unit.pos.x - p.x) + Math.abs(unit.pos.y - p.y);
            } else {
                let manhattanDistanceToUnit = Math.abs(unit.pos.x - p.x) + Math.abs(unit.pos.y - p.y);
                if (manhattanDistanceToEnemy > manhattanDistanceToUnit) {
                    enemy = unit;
                    manhattanDistanceToEnemy = manhattanDistanceToUnit;
                }
            }
        }
    }

    return enemy;
}

function tick() {
    let myUnitsCount = 0;

    for (let myUnit of model.myUnits) {
        ++myUnitsCount;

        let nearbyEnemy = findEnemyNearby(myUnit.pos);
        if (nearbyEnemy) {
            if (myUnit.control.Attack(nearbyEnemy) != Result.Ok)
                myUnit.control.MoveTo(nearbyEnemy);
        } else {
            let destination = [myUnit.pos.x - 1 + Math.floor(Math.random() * 3), myUnit.pos.y - 1 + Math.floor(Math.random() * 3)];
            myUnit.control.MoveTo(destination);
        }
    }

    if (myUnitsCount < 4)
        for (let myFactory of model.myFactories)
            myFactory.control.Produce();
}

