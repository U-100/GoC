function tick()
{
    let myUnitsCount = 0;
    
    for(let myUnit of model.myUnits)
    {
        ++myUnitsCount;
        
        if ((myUnit.data ??= {}).moved)
        {
            myUnit.data.moved = false;

            let r = Math.floor(Math.random() * 2);
            if (r)
                myUnit.control.TurnLeft();
            else
                myUnit.control.TurnRight();
        }
        else
        {
            myUnit.data.moved = true;
            myUnit.control.Move();
        }
    }

    if(myUnitsCount < 4)
        for(let myFactory of model.myFactories)
            myFactory.control.Produce();
}

