using UnityEngine;

namespace GoC
{
	public class RootViewModel
	{
		public Loading loading { get; set; }
		public MainMenu mainMenu { get; set; }
	}

	public class RootView : MonoBehaviour
	{
		[SerializeField] private LoadingView _loadingView = null;
		[SerializeField] private MainMenuView _mainMenuView = null;

		public RootViewModel viewModel { get; set; }

		public void Update()
		{
			this._loadingView.gameObject.SetActive(this.viewModel?.loading != null);
			this._loadingView.viewModel = this.viewModel?.loading;

			this._mainMenuView.gameObject.SetActive(this.viewModel?.mainMenu != null);
			this._mainMenuView.viewModel = this.viewModel?.mainMenu;
		}
	}
}
