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
        private readonly TeamConfig _aiTeamConfig;
        
        private IArena _currentArena;

        public ArenaViewModel(Func<IArena> arenaFactory, ArenaConfig arenaConfig, TeamSkin playerTeamSkin, TeamConfig aiTeamConfig)
        {
            _arenaFactory = arenaFactory;
            _arenaConfig = arenaConfig;
            _playerTeamSkin = playerTeamSkin;
            _aiTeamConfig = aiTeamConfig;
            _codeVsAIScript = PlayerPrefs.GetString("USER_SCRIPT_PATH", "./TestStrategy.js");

            Application.logMessageReceived += OnLogMessageReceived;
        }

        [CreateProperty] public string version => Application.version;
        
        [CreateProperty] public bool isInMainMenu { get; private set; } = true;

        [CreateProperty] public bool isInCodeVsAIMenu { get; private set; }

        private string _codeVsAIScript;
        [CreateProperty] public string codeVsAIScript
        {
            get => _codeVsAIScript;
            set => PlayerPrefs.SetString("USER_SCRIPT_PATH", _codeVsAIScript = value);
        }
        
        [CreateProperty] public bool isInHud { get; private set; }

        [CreateProperty] public bool isPaused => Time.timeScale == 0;

        [CreateProperty] public string log { get; private set; }

        public void PrepareCodeVsAI()
        {
            if (_currentArena != null) throw new InvalidOperationException();
            
            isInMainMenu = false;
            isInCodeVsAIMenu = true;

            _currentArena = _arenaFactory();
            _currentArena.Setup(_arenaConfig);
        }

        public void Quit()
        {
#if !UNITY_EDITOR
            Application.Quit();
#else
            UnityEditor.EditorApplication.isPlaying = false; // Application.Quit() does not work in the editor so UnityEditor.EditorApplication.isPlaying need to be set to false instead
#endif
        }
        
        public void PlayCodeVsAI()
        {
            TeamConfig playerTeamConfig = TeamConfig.Create(codeVsAIScript, _playerTeamSkin);
            _currentArena.CreateTeams(playerTeamConfig, _aiTeamConfig);
            
            isInMainMenu = false;
            isInCodeVsAIMenu = false;
            isInHud = true;
        }

        public void Back()
        {
            isInMainMenu = true;
            isInCodeVsAIMenu = false;
            
            _currentArena?.Dispose();
            _currentArena = null;
        }

        public void Fast()
        {
            Time.timeScale = 4;
        }

        public void Play()
        {
            Time.timeScale = 1;
        }

        public void Pause()
        {
            Time.timeScale = 0;
        }

        public void Stop()
        {
            isInMainMenu = true;
            isInCodeVsAIMenu = false;
            isInHud = false;
            
            _currentArena?.Dispose();
            _currentArena = null;
        }

        private void OnLogMessageReceived(string message, string stacktrace, LogType logType)
        {
            if (logType == LogType.Warning)
                log += $"<color=#FFFF00>";
            else if (logType != LogType.Log)
                log += $"<color=#FF0000>";
            
            if (_currentArena != null)
                log += $"[{_currentArena.currentTick,4}] ";

            log += message + "\n";
            log += stacktrace.Substring(0, stacktrace.IndexOf('\n')) + "\n";

            if (logType != LogType.Log)
                log += $"</color>";
            
            if (log.Length > 2000)
                log = log.Substring(log.Length - 2000, 2000);
        }
    }
}
