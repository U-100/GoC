using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Properties;
using UnityEngine;

namespace GoC.Arena
{
    public class ArenaViewModel
    {
        private readonly Func<IArena> _arenaFactory;
        private readonly TeamConfig _aiTeamConfig;
        
        private readonly TeamConfig _playerTeamConfig;
        
        private IArena _currentArena;
        private int _codeVsAIScriptIndex;

        public ArenaViewModel(Func<IArena> arenaFactory, TeamConfig playerTeamConfig, TeamConfig aiTeamConfig)
        {
            _arenaFactory = arenaFactory;
            _playerTeamConfig = playerTeamConfig;
            _aiTeamConfig = aiTeamConfig;
            
            codeVsAIScripts = new List<string>();
            
            Application.logMessageReceived += OnLogMessageReceived;
        }

        [CreateProperty] public string version => Application.version;
        
        [CreateProperty] public bool isInMainMenu { get; private set; } = true;

        [CreateProperty] public bool isInCodeVsAIMenu { get; private set; }

        [CreateProperty] public List<string> codeVsAIScripts { get; private set; }

        [CreateProperty]
        public int codeVsAIScriptIndex
        {
            get => _codeVsAIScriptIndex;
            set
            {
                _codeVsAIScriptIndex = value;
                PlayerPrefs.SetString("USER_SCRIPT_PATH", codeVsAIScripts[_codeVsAIScriptIndex]);
            }
        }

        [CreateProperty] public bool isInHud { get; private set; }

        [CreateProperty] public bool isPaused => Time.timeScale == 0;

        [CreateProperty] public string log { get; private set; }

        public void PrepareCodeVsAI()
        {
            if (_currentArena != null) throw new InvalidOperationException();
            if (codeVsAIScriptIndex < 0) throw new InvalidOperationException();
            
            isInMainMenu = false;
            isInCodeVsAIMenu = true;

            _currentArena = _arenaFactory();
            
            codeVsAIScripts.Clear();
            codeVsAIScripts.AddRange(
                Directory.GetFiles(EngineFactory.JAVASCRIPT_MODULES_BASE_PATH, "*.js", SearchOption.AllDirectories)
                    .Select(path => Path.GetRelativePath(EngineFactory.JAVASCRIPT_MODULES_BASE_PATH, path))
                    .Select(path => Path.Combine("./", path)));
            
            string lastScriptPath = PlayerPrefs.GetString("USER_SCRIPT_PATH", "./TestStrategy.js");
            codeVsAIScriptIndex = codeVsAIScripts.IndexOf(lastScriptPath);
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
            TeamConfig playerTeamConfig =
                TeamConfig
                    .Clone(_playerTeamConfig)
                    .WithScript(codeVsAIScripts[codeVsAIScriptIndex]);
            
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
            int indexOfNewLine = stacktrace.IndexOf('\n');
            if (indexOfNewLine >= 0)
                log += stacktrace.Substring(0, indexOfNewLine) + "\n";
            else
                log += stacktrace + "\n";

            if (logType != LogType.Log)
                log += $"</color>";
            
            if (log.Length > 2000)
                log = log.Substring(log.Length - 2000, 2000);
        }
    }
}
