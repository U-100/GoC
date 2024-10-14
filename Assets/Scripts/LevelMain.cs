using System;
using System.IO;
using System.Threading.Tasks;
using Jint;
using UnityEngine;

namespace GoC
{
    public class LevelMain : ExclusiveMainBehaviour
    {
        [SerializeField] private GameObject _enableForTesting;
        [SerializeField] private HudView _hudView;
        [SerializeField] private string _scriptFilePath;

        protected override void Run()
        {
            if (_enableForTesting != null)
                _enableForTesting.SetActive(true);

            RunLevel();
        }

        public async void RunLevel()
        {
            try
            {
                using Hud hud = new Hud();
                _hudView.viewModel = hud;

                Engine jsEngine = new Engine();
                jsEngine.SetValue("log", new Action<object>(Debug.Log));

                foreach (RuntimeApiMarker runtimeApiMarker in FindObjectsByType<RuntimeApiMarker>(FindObjectsSortMode.None))
                    runtimeApiMarker.SetupRuntimeApi(jsEngine, destroyCancellationToken);

                hud.script = await File.ReadAllTextAsync(_scriptFilePath);
                
                await foreach (string script in hud.GetScriptsAsync(destroyCancellationToken))
                {
                    if (destroyCancellationToken.IsCancellationRequested)
                        break;

                    try
                    {
                        await Task.Run(() => jsEngine.Execute(script));
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex, this);
                    }
                }
            }
            finally
            {
                _hudView.viewModel = null;
            }
        }
    }
}
