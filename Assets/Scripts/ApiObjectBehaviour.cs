using UnityEngine;

namespace GoC
{
    public abstract class ApiObjectBehaviour : MonoBehaviour, IApiObject
    {
        private IExecutionContext _executionContext;

        public int id => GetInstanceID();
        public int teamId { get; private set; }
        public object data { get; set; }

        protected void Init(int initTeamId, IExecutionContext executionContext)
        {
            teamId = initTeamId;
            _executionContext = executionContext;
        }
        
        protected bool CheckExecutionRestrictions() => _executionContext.teamId == teamId;
    }
}
