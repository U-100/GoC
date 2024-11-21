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

        public IModel AddTeamModel(int addedTeamId, FactoryBehaviour newTeamFactoryBehaviour)
        {
            newTeamFactoryBehaviour.Init(this, addedTeamId, unit => AddUnit(addedTeamId, unit));
            _factories.Add(newTeamFactoryBehaviour.id, newTeamFactoryBehaviour);

            return new TeamModel(addedTeamId, _factories, _units);
        }

        private void AddUnit(int unitTeamId, UnitBehaviour unitBehaviour)
        {
            unitBehaviour.Init(this, unitTeamId);
            _units.Add(unitBehaviour.id, unitBehaviour);
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
