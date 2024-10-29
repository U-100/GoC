using System.Collections.Generic;

namespace GoC
{
    public class Model : IModel
    {
        private readonly List<IUnit> _units;

        public Model(IUnit unit)
        {
            _units = new List<IUnit> { unit };
        }
        
        public string team { get; set; }
        public IReadOnlyList<IUnit> units => _units;
    }
}
