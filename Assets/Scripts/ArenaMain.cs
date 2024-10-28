using System.IO;
using Jint;
using UnityEngine;

namespace GoC
{
    public class ArenaMain : MonoBehaviour
    {
        [SerializeField] private string _redScriptFilePath;

        private Engine _redEngine;

        private void OnEnable()
        {
            if (!string.IsNullOrEmpty(_redScriptFilePath))
            {
                _redEngine = EngineFactory.Create();
                string redScript = File.ReadAllText(_redScriptFilePath);
                _redEngine.Execute(redScript);
            }
        }

        private void OnDisable()
        {
            _redEngine?.Dispose();
        }

        private void Update()
        {
            _redEngine?.Invoke("tick");
        }
    }
}
