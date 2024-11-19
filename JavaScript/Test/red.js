function tick()
{
    let myUnitsCount = 0;
    
    for(let unit of model.myUnits)
    {
        ++myUnitsCount;
        
        if ((unit.data ??= {}).moved)
        {
            unit.data.moved = false;

            let r = Math.floor(Math.random() * 2);

            switch (r)
            {
                case 0:
                    unit.TurnLeft();
                    break;

                case 1:
                    unit.TurnRight();
                    break;
            }
        }
        else
        {
            unit.data.moved = true;
            unit.Move();
        }
    }

    if(myUnitsCount < 4)
        model.myFactory.Produce();
}

