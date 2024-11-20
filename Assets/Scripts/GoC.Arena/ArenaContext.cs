using System;
using GoC.ScriptingApi;

namespace GoC.Arena
{
    public class ArenaContext : IArenaContext
    {
        private readonly struct Token : IDisposable
        {
            private readonly ArenaContext _arenaContext;
            private readonly IModel _oldModel;
            
            public Token(ArenaContext arenaContext, IModel oldModel)
            {
                _arenaContext = arenaContext;
                _oldModel = oldModel;
            }

            public void Dispose() => _arenaContext._currentModel = _oldModel;
        }

        private IModel _currentModel;

        public int teamId => _currentModel.teamId;

        public IDisposable ForModel(IModel model)
        {
            Token token = new Token(this, _currentModel);
            _currentModel = model;
            return token;
        }
    }
}
