using Jint;
using System;
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

			await LoadSceneAsync("MoveBot", this._rootViewModel.loading.SetProgress);

			this._rootViewModel.hud = this._hudFactory();
			this._rootViewModel.loading = null;

			Engine engine = new Engine();
			
			engine.SetValue("log", new Action<object>(Debug.Log));

			foreach (GameObject gameObject in SceneManager.GetActiveScene().GetRootGameObjects())
				gameObject.GetComponent<IRuntimeApiMarker>()?.SetupRuntimeApi(engine);

			//foreach (KeyValuePair<JsValue, PropertyDescriptor> kvp in engine.Realm.GlobalObject.GetOwnProperties())
			//	Debug.Log($"{kvp.Key.AsString()}");

			await foreach(string script in this._rootViewModel.hud.GetScriptsAsync())
			{
				try
				{
					await Task.Run(() => engine.Execute(script));
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
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
