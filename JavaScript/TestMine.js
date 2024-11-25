export function tick()
{
    let myUnitsCount = 0;

    let deposit = undefined;
    for(d of model.deposits) { deposit = d; break; }        
    
    for(let myUnit of model.myUnits)
    {
        ++myUnitsCount;
     
        if(deposit)
        {
            if(myUnit.CanMine(deposit) == Result.Ok)
            {
                log("Have " + myUnit.storage[Substance.Type.Energy] + " Energy, mining");
                myUnit.control.Mine(deposit);
            }
            else
            {
                log("Have " + myUnit.storage[Substance.Type.Energy] + " Energy, moving to deposit");
                myUnit.control.MoveTo(deposit.pos);
            }
        }
        else
            log("See no deposits");
    }

    if(myUnitsCount < 1)
        for(let myFactory of model.myFactories)
            myFactory.control.Produce();
}

