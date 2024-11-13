const unitDataStore = {};

function tick()
{
    let myUnitsCount = 0;
    
    for(let unit of model.myUnits)
    {
        ++myUnitsCount;
        
        let unitData = (unitDataStore[unit.id] ??= {});

        if (unitData.moved)
        {
            unitData.moved = false;

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
            unitData.moved = true;
            unit.Move();
        }
    }

    if(myUnitsCount < 4)
        model.myFactory.Produce();
}

