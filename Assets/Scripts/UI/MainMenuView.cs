using UnityEngine;
using UnityEngine.UIElements;

namespace GoC
{
	public class MainMenu
	{
		private readonly INavigator _navigator;

		public MainMenu(INavigator navigator) => this._navigator = navigator;

		public void Play() => this._navigator.Play();
	}

	public class MainMenuView : MonoBehaviour
	{
		public MainMenu viewModel { get; set; }

		void OnEnable()
		{
			Button playButton = this.GetComponent<UIDocument>().rootVisualElement.Q<Button>("PlayButton");
			playButton.clicked += () => this.viewModel?.Play();
		}
	}
}
