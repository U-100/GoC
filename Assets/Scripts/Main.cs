using Jint;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace GoC
{
	public class Main : MonoBehaviour
	{
		[SerializeField] private HudView _hudView;

		async void Start()
		{
			try
			{
				Hud hud = new Hud();
				this._hudView.viewModel = hud;

				Engine jsEngine = new Engine();
				jsEngine.SetValue("log", new Action<object>(Debug.Log));

				foreach (RuntimeApiMarker runtimeApiMarker in FindObjectsOfType<RuntimeApiMarker>())
					runtimeApiMarker.SetupRuntimeApi(jsEngine);

				await foreach(string script in hud.GetScriptsAsync())
				{
					try
					{
						await Task.Run(() => jsEngine.Execute(script));
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
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
