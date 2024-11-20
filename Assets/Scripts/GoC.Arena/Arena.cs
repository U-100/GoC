using System;
using System.Collections.Generic;
using GoC.ScriptingApi;

namespace GoC.Arena
{
    public class Arena : IArenaContext
    {
        private readonly Dictionary<int, IFactory> _factories = new();
        private readonly Dictionary<int, IUnit> _units = new();

        private IModel _currentModel;

        public IModel AddTeamModel(int addedTeamId, Factory newTeamFactory)
        {
            newTeamFactory.Init(this, addedTeamId, unit => AddUnit(addedTeamId, unit));
            _factories.Add(newTeamFactory.id, newTeamFactory);

            return new TeamModel(addedTeamId, _factories, _units);
        }

        private void AddUnit(int unitTeamId, Unit unit)
        {
            unit.Init(this, unitTeamId);
            _units.Add(unit.id, unit);
        }

        public int executingTeamId => _currentModel.teamId;

        private readonly struct Token : IDisposable
        {
            private readonly Arena _arena;
            private readonly IModel _savedModel;
            
            public Token(Arena arena, IModel savedModel)
            {
                _arena = arena;
                _savedModel = savedModel;
            }

            public void Dispose() => _arena._currentModel = _savedModel;
        }

        public IDisposable ForModel(IModel model)
        {
            Token token = new Token(this, _currentModel);
            _currentModel = model;
            return token;
        }
    }
}
