using System;

namespace GoC
{
    public interface IExecutionContext
    {
        int teamId { get; }
        
        IDisposable ForModel(IModel model);
    }
    
    public class ExecutionContext : IExecutionContext
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

        public IDisposable ForModel(IModel model)
        {
            Token token = new Token(this, _currentModel);
            _currentModel = model;
            return token;
        }
    }
}
