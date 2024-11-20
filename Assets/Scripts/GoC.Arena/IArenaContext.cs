using System;
using GoC.ScriptingApi;

namespace GoC.Arena
{
    public interface IArenaContext
    {
        int executingTeamId { get; }
        
        IDisposable ForModel(IModel model);
    }
}
