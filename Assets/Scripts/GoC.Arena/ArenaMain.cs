using UnityEngine;

namespace GoC.Arena
{
    public class ArenaMain : MonoBehaviour
    {
        [SerializeField] private ArenaView _arenaView;
        [SerializeField] private ArenaConfig _arenaConfig;
        [SerializeField] private TeamConfig _playerTeamConfig;
        [SerializeField] private TeamConfig _aiTeamConfig;

        private ArenaViewModel _arenaViewModel;
        private ArenaViewModel arenaViewModel =>
            _arenaViewModel ??=
                new ArenaViewModel(
                    () => Arena.Create(_arenaConfig),
                    _playerTeamConfig,
                    _aiTeamConfig
                );

        void Start()
        {
            _arenaView.viewModel = arenaViewModel;
        }
    }
}
