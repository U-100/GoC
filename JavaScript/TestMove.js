export function tick()
{
    let myUnitsCount = 0;
    
    for(let myUnit of model.myUnits)
    {
        ++myUnitsCount;

        let destination = [myUnit.pos.x - 1 + Math.floor(Math.random() * 3), myUnit.pos.y - 1 + Math.floor(Math.random() * 3)];
        myUnit.control.MoveTo(destination);
    }

    if(myUnitsCount < 4)
        for(let myFactory of model.myFactories)
            myFactory.control.Produce();
}

