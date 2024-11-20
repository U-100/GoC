using System;
using GoC.ScriptingApi;

namespace GoC.Arena
{
    public interface IArenaContext
    {
        int teamId { get; }
        
        IDisposable ForModel(IModel model);
    }
}
