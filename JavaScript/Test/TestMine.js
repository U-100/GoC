function tick()
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
                myUnit.control.Mine(deposit);
                log("Have " + myUnit.storage[Substance.Type.Energy] + " Energy");
            }
            else
                myUnit.control.MoveTo(deposit.pos);
        }
    }

    if(myUnitsCount < 1)
        for(let myFactory of model.myFactories)
            myFactory.control.Produce();
}

