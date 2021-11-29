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

	public class Navigator : INavigator
	{
		private readonly RootViewModel _rootViewModel;
		private readonly LoadingFactory _loadingFactory;
		private readonly MainMenuFactory _mainMenuFactory;

		public Navigator(RootViewModel rootViewModel, LoadingFactory loadingFactory, MainMenuFactory mainMenuFactory)
		{
			this._rootViewModel = rootViewModel;
			this._loadingFactory = loadingFactory;
			this._mainMenuFactory = mainMenuFactory;
		}

		public async void Start()
		{
			this._rootViewModel.loading = this._loadingFactory();

			await LoadSceneAsync("Intro", this._rootViewModel.loading.SetProgress);

			this._rootViewModel.mainMenu = this._mainMenuFactory();
			this._rootViewModel.loading = null;
		}

		public async void Play()
		{
			this._rootViewModel.mainMenu = null;
			this._rootViewModel.loading = this._loadingFactory();

			await LoadSceneAsync("HeroMovement", this._rootViewModel.loading.SetProgress);

			this._rootViewModel.loading = null;
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
