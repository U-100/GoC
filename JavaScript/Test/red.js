var model;

export function init(initModel)
{
    model = initModel;
}

var unitDataStore = {}

export function tick()
{
    for(let unit of model.units)
    {
        let unitData = (unitDataStore[unit] ??= {});

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
}

