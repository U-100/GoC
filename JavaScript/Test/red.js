var model;

export function init(initModel)
{
    model = initModel;
}

export function tick()
{
    for(let unit of model.units)
    {
        let r = Math.floor(Math.random() * 4);

        switch (r)
        {
            case 0:
                log("stay");
                break;
                
            case 1:
                unit.Move();
                break;
                
            case 2:
                unit.TurnLeft();
                break;
                
            case 3:
                unit.TurnRight();
                break;
        }
    }
}

