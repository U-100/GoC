using Jint;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GoC
{
	public class Main : MonoBehaviour
	{
		[SerializeField] private HudView _hudView;
		[SerializeField] private Bot _bot;

		private CancellationTokenSource _cts;

		async void OnEnable()
		{
			this._cts = new CancellationTokenSource();

			try
			{
				Hud hud = new Hud();
				this._hudView.viewModel = hud;

				Engine jsEngine = new Engine();
				jsEngine.SetValue("log", new Action<object>(Debug.Log));

				foreach (RuntimeApiMarker runtimeApiMarker in FindObjectsOfType<RuntimeApiMarker>())
					runtimeApiMarker.SetupRuntimeApi(jsEngine, this._cts.Token);

				await foreach (string script in hud.GetScriptsAsync(this._cts.Token))
				{
					if (this._cts.IsCancellationRequested)
						break;

					await Task.Run(() => jsEngine.Execute(script));
				}
			}
			finally
			{
				this._hudView.viewModel = null;
			}
		}

		void OnDisable()
		{
			this._cts.Cancel();
			this._cts.Dispose();
			this._cts = null;
		}
	}
}
