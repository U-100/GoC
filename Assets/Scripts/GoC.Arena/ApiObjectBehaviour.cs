using GoC.ScriptingApi;
using UnityEngine;

namespace GoC.Arena
{
    public abstract class ApiObjectBehaviour : MonoBehaviour, IApiObject
    {
        private IArenaContext _arenaContext;

        public int id => GetInstanceID();
        public int teamId { get; private set; }

        protected void Init(IArenaContext arenaContext, int initTeamId)
        {
            teamId = initTeamId;
            _arenaContext = arenaContext;
        }
        
        protected bool CheckExecutionRestrictions() => _arenaContext.teamId == teamId;
    }
}
