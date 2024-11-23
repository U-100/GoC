function tick()
{
    let myUnitsCount = 0;

    for(let myUnit of model.myUnits)
        ++myUnitsCount;

    if (myUnitsCount < 2)
        for(let myFactory of model.myFactories)
            myFactory.control.Produce();

    if (myUnitsCount == 1)
        for(let myUnit of model.myUnits)
            myUnit.control.MoveTo(myUnit.pos.x, myUnit.pos.y + 1);

    if (myUnitsCount >= 2)
    {
        let source = null;
        let target = null;
        for(let myUnit of model.myUnits)
        {
            if (!source)
            {
                source = myUnit;
                continue;
            }

            target = myUnit;
            break;
        }

        let giveResult = source.control.Give(target, Substance.Type.Energy, 1)
        if(giveResult == Result.Ok)
            log("Source has " + source.storage[Substance.Type.Energy] + " and target has " + target.storage[Substance.Type.Energy]);
        else
            log("Give failed with error " + giveResult);
    }
}

