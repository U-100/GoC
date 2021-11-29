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
				() => new MainMenu(navigator)
			);

			navigator.Start();
		}
	}

	//public class Main : MonoBehaviour
	//{
	//	[SerializeField] private UIDocument _uiDocument = null;
	//	[SerializeField] private Hero _hero = null;

	//	private Dictionary<string, object> _api;
	//	private ScriptRunner _scriptRunner;

	//	void Start()
	//	{
	//		this._api = new Dictionary<string, object>
	//		{
	//			["log"] = new Action<object>(Debug.Log),
	//			["hero"] = this._hero,
	//			["north"] = Vector3.forward,
	//			["south"] = Vector3.back,
	//			["west"] = Vector3.left,
	//			["east"] = Vector3.right,
	//		};

	//		this._scriptRunner = new ScriptRunner();

	//		TextField immediateScript = this._uiDocument.rootVisualElement.Q<TextField>("ImmediateScript");
	//		//immediateScript.RegisterCallback<NavigationSubmitEvent>(_ => OnSubmitImmediateScript(immediateScript));

	//		Button go = this._uiDocument.rootVisualElement.Q<Button>("Go");
	//		go.clicked += () => OnClickedGoButton(immediateScript.value);
	//	}

	//	private async void OnSubmitImmediateScript(TextField immediateScript)
	//	{
	//		immediateScript.Focus();
	//		await this._scriptRunner.Execute(this._api, immediateScript.value);
	//	}

	//	private async void OnClickedGoButton(string script)
	//	{
	//		await this._scriptRunner.Execute(this._api, script);
	//	}
	//}
}