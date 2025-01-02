using System;
using Unity.Properties;
using UnityEngine;

namespace GoC.Arena
{
    public class ArenaViewModel
    {
        private readonly Func<IArena> _arenaFactory;
        private readonly ArenaConfig _arenaConfig;
        private readonly TeamSkin _playerTeamSkin;
        private readonly TeamSkin _aiTeamSkin;
        
        private IArena _currentArena;

        public ArenaViewModel(Func<IArena> arenaFactory, ArenaConfig arenaConfig, TeamSkin playerTeamSkin, TeamSkin aiTeamSkin)
        {
            _arenaFactory = arenaFactory;
            _arenaConfig = arenaConfig;
            _playerTeamSkin = playerTeamSkin;
            _aiTeamSkin = aiTeamSkin;
        }
        
        [CreateProperty] public bool isInMainMenu { get; private set; } = true;
        [CreateProperty] public bool isInCodeVsAIMenu { get; private set; }

        [CreateProperty] public string codeVsAIScript { get; set; } = "./TestStrategy.js";

        public void PrepareCodeVsAI()
        {
            if (_currentArena != null) throw new InvalidOperationException();
            
            isInMainMenu = false;
            isInCodeVsAIMenu = true;

            _currentArena = _arenaFactory();
            _currentArena.Setup(_arenaConfig);
        }
        
        public void PlayCodeVsAI()
        {
            Debug.Log($"Play \"{codeVsAIScript}\" vs. AI");
            TeamConfig playerTeamConfig = TeamConfig.Create(codeVsAIScript, _playerTeamSkin);
            TeamConfig aiTeamConfig = TeamConfig.Create("./PvEBotStrategy.js", _aiTeamSkin);
            
            _currentArena.CreateTeams(playerTeamConfig, aiTeamConfig);
            
            isInMainMenu = false;
            isInCodeVsAIMenu = false;
        }

        public void Back()
        {
            isInMainMenu = true;
            isInCodeVsAIMenu = false;
            
            _currentArena?.Dispose();
            _currentArena = null;
        }

        public void Quit()
        {
#if !UNITY_EDITOR
            Application.Quit();
#else
            UnityEditor.EditorApplication.isPlaying = false; // Application.Quit() does not work in the editor so UnityEditor.EditorApplication.isPlaying need to be set to false instead
#endif
        }
    }
}
