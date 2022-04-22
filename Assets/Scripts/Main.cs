using UnityEngine;

namespace GoC
{
	public class Main : MonoBehaviour
	{
		[SerializeField] private RootView _rootView = null;

		void Start()
		{
			DontDestroyOnLoad(this.gameObject);

			RootViewModel rootViewModel = new RootViewModel();
			this._rootView.viewModel = rootViewModel;

			Navigator navigator = null;
			navigator = new Navigator(
				rootViewModel,
				() => new Loading(),
				() => new MainMenu(navigator),
				() => new Hud()
			);

			navigator.Start();
		}
	}
}