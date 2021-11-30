using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GoC
{
	public interface INavigator
	{
		void Start();
		void Play();
	}

	public delegate Loading LoadingFactory();
	public delegate MainMenu MainMenuFactory();
	public delegate Hud HudFactory();

	public class Navigator : INavigator
	{
		private const string INTRO_SCENE_NAME = "Intro";
		private const string RUNTIME_API_TAG = "JsRuntimeApi";

		private readonly RootViewModel _rootViewModel;
		private readonly LoadingFactory _loadingFactory;
		private readonly MainMenuFactory _mainMenuFactory;
		private readonly HudFactory _hudFactory;

		public Navigator(RootViewModel rootViewModel, LoadingFactory loadingFactory, MainMenuFactory mainMenuFactory, HudFactory hudFactory)
		{
			this._rootViewModel = rootViewModel;
			this._loadingFactory = loadingFactory;
			this._mainMenuFactory = mainMenuFactory;
			this._hudFactory = hudFactory;
		}

		public async void Start()
		{
			this._rootViewModel.loading = this._loadingFactory();

			await LoadSceneAsync(INTRO_SCENE_NAME, this._rootViewModel.loading.SetProgress);

			this._rootViewModel.mainMenu = this._mainMenuFactory();
			this._rootViewModel.loading = null;
		}

		public async void Play()
		{
			this._rootViewModel.mainMenu = null;
			this._rootViewModel.loading = this._loadingFactory();

			await LoadSceneAsync("HeroMovement", this._rootViewModel.loading.SetProgress);

			Dictionary<string, object> runtimeApi = new Dictionary<string, object>
			{
				["log"] = new Action<object>(Debug.Log),
				["north"] = Vector3.forward,
				["south"] = Vector3.back,
				["west"] = Vector3.left,
				["east"] = Vector3.right,
				["Vector3"] = typeof(Vector3),
			};

			Scene activeScene = SceneManager.GetActiveScene();
			foreach (GameObject gameObject in activeScene.GetRootGameObjects())
			{
				IRuntimeApiMarker runtimeApiMarker = gameObject.GetComponent<IRuntimeApiMarker>();
				if (runtimeApiMarker != null)
					runtimeApi[gameObject.name] = runtimeApiMarker;
			}

			this._rootViewModel.hud = this._hudFactory();
			this._rootViewModel.loading = null;

			while (true)
			{
				await this._rootViewModel.hud.WaitExecuteAsync();

				ScriptRunner scriptRunner = new ScriptRunner();
				await scriptRunner.Execute(runtimeApi, this._rootViewModel.hud.script);
			}
		}

		private async Task LoadSceneAsync(string sceneName, Action<float> progress)
		{
			AsyncOperation loadSceneAsync = SceneManager.LoadSceneAsync(sceneName);
			while (!loadSceneAsync.isDone)
			{
				progress(loadSceneAsync.progress);
				await Task.Yield();
			}
		}
	}
}
