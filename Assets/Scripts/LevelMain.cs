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
			if (this._enableForTesting != null)
				this._enableForTesting.SetActive(true);

			RunLevel();
		}

		public async void RunLevel()
		{
			this._cts = new CancellationTokenSource();

			try
			{
				using Hud hud = new Hud();
				this._hudView.viewModel = hud;

				Engine jsEngine = new Engine();
				jsEngine.SetValue("log", new Action<object>(Debug.Log));

				foreach (RuntimeApiMarker runtimeApiMarker in FindObjectsOfType<RuntimeApiMarker>())
					runtimeApiMarker.SetupRuntimeApi(jsEngine, this._cts.Token);

				await foreach (string script in hud.GetScriptsAsync(this._cts.Token))
				{
					if (this._cts.IsCancellationRequested)
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
				this._hudView.viewModel = null;
			}
		}
	}
}
