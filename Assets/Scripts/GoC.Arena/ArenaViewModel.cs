using Unity.Properties;
using UnityEngine;

namespace GoC.Arena
{
    public class ArenaViewModel
    {
        private bool _isInMainMenu = true;
        private bool _isInPlayMenu;
        
        [CreateProperty] public bool isInMainMenu => _isInMainMenu;
        [CreateProperty] public bool isInCodeVsAIMenu => _isInPlayMenu;

        [CreateProperty] public string codeVsAIScript { get; set; }

        public void PrepareCodeVsAI()
        {
            _isInMainMenu = false;
            _isInPlayMenu = true;
        }
        
        public void PlayCodeVsAI()
        {
            Debug.Log($"Play \"{codeVsAIScript}\" vs. AI");
        }

        public void Back()
        {
            _isInMainMenu = true;
            _isInPlayMenu = false;
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
