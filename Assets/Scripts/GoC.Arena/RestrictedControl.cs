namespace GoC.Arena
{
    public class RestrictedControl<T>
    {
        private readonly IArenaContext _arenaContext;
        private readonly T _control;

        public RestrictedControl(IArenaContext arenaContext, int teamId, T control)
        {
            _arenaContext = arenaContext;
            this.teamId = teamId;
            _control = control;
        }
        
        public int teamId { get; }
        
        public T control => _arenaContext.executingTeamId == teamId ? _control : default;
    }
}
