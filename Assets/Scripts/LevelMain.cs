using System;
using System.Threading;
using System.Threading.Tasks;
using Jint;
using UnityEngine;

namespace GoC
{
	public class LevelMain : ExclusiveMainBehaviour
	{
		[SerializeField] private GameObject _enableForTesting;
		[SerializeField] private HudView _hudView;

		private CancellationTokenSource _cts;

		protected override void Run()
		{
			if (_enableForTesting != null)
				_enableForTesting.SetActive(true);

			RunLevel();
		}

		public async void RunLevel()
		{
			_cts = new CancellationTokenSource();

			try
			{
				using Hud hud = new Hud();
				_hudView.viewModel = hud;

				Engine jsEngine = new Engine();
				jsEngine.SetValue("log", new Action<object>(Debug.Log));

				foreach (RuntimeApiMarker runtimeApiMarker in FindObjectsByType<RuntimeApiMarker>(FindObjectsSortMode.None))
					runtimeApiMarker.SetupRuntimeApi(jsEngine, _cts.Token);

				await foreach (string script in hud.GetScriptsAsync(_cts.Token))
				{
					if (_cts.IsCancellationRequested)
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
