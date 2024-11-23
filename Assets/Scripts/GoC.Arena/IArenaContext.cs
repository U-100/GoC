namespace GoC.Arena
{
    public interface IArenaContext
    {
        uint currentTick { get; }
        int executingTeamId { get; }
    }
}
