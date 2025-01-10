using UnityEngine;

namespace GoC.Arena
{
    public class ArenaMain : MonoBehaviour
    {
        [SerializeField] private ArenaView _arenaView;
        [SerializeField] private Arena _arenaPrefab;
        [SerializeField] private ArenaConfig _arenaConfig;
        [SerializeField] private TeamSkin _playerTeamSkin;
        [SerializeField] private TeamConfig _aiTeamConfig;

        private ArenaViewModel _arenaViewModel;
        private ArenaViewModel arenaViewModel => _arenaViewModel ??= new ArenaViewModel(CreateArena, _arenaConfig, _playerTeamSkin, _aiTeamConfig);

        void Start()
        {
            _arenaView.viewModel = arenaViewModel;
        }

        private IArena CreateArena() => Instantiate(_arenaPrefab);
    }
}
