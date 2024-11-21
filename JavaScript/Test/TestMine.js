let manhdist = (a, b) => Math.abs(b[0] - a[0]) + Math.abs(b[1] - a[1]);

let vecsum = (a, b) => [a[0] + b[0], a[1] + b[1]];

function tick()
{
    let myUnitsCount = 0;

    let deposit = model.deposits.find(_ => true);
    
    for(let myUnit of model.myUnits)
    {
        ++myUnitsCount;
     
        if(deposit)
        {
            let curDist = manhdist(myUnit.pos, deposit.pos);
            
            if(curDist > 1)
            {
                let posAfterMove = vecsum(myUnit.pos, myUnit.dir);
                let distAfterMove = manhdist(posAfterMove, deposit.pos);
                if(distAfterMove < curDist)
                    myUnit.control.Move();
                else
                    myUnit.control.TurnLeft();
            }
            else
            {
                myUnit.control.Mine(deposit);
                log("Have " + myUnit.storage[Substance.Type.Energy] + " Energy");
            }
        }
    }

    if(myUnitsCount < 1)
        for(let myFactory of model.myFactories)
            myFactory.control.Produce();
}

