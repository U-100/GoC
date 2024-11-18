using System;
using UnityEngine;

namespace GoC
{
    [CreateAssetMenu]
    public class ExecutionContext : ScriptableObject
    {
        public struct Token : IDisposable
        {
            private readonly ExecutionContext _executionContext;
            private readonly IModel _oldModel;
            
            public Token(ExecutionContext executionContext, IModel oldModel)
            {
                _executionContext = executionContext;
                _oldModel = oldModel;
            }

            public void Dispose() => _executionContext._currentModel = _oldModel;
        }

        private IModel _currentModel;

        public int teamId => _currentModel.teamId;

        public Token ForModel(IModel model)
        {
            Token token = new Token(this, _currentModel);
            _currentModel = model;
            return token;
        }
    }
}
