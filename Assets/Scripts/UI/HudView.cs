using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace GoC
{
	public class Hud
	{
		private TaskCompletionSource<bool> _executeTcs;

		public string script { get; set; } = "";

		public async IAsyncEnumerable<string> GetScriptsAsync()
		{
			while (true)
			{
				Task t = WaitExecuteAsync();
				await t;
				if (t.IsCompletedSuccessfully)
					yield return this.script;
				else
					break;
			}
		}

		public Task WaitExecuteAsync()
		{
			return (this._executeTcs ??= new TaskCompletionSource<bool>()).Task;
		}

		public void SetScript(string s) => this.script = s;
		public void Execute()
		{
			this._executeTcs?.SetResult(true);
			this._executeTcs = null;
		}
	}

	public class HudView : MonoBehaviour
	{
		private bool _justEnabled;

		public Hud viewModel { get; set; }

		void OnEnable()
		{
			UIDocument uiDocument = GetComponent<UIDocument>();
			VisualElement rootVisualElement = uiDocument.rootVisualElement;

			Button button = rootVisualElement.Q<Button>("Go");
			button.clicked += () => this.viewModel?.Execute();

			TextField textField = rootVisualElement.Q<TextField>("ImmediateScript");
			textField.SetVerticalScrollerVisibility(ScrollerVisibility.Auto);
			textField.RegisterValueChangedCallback(changeEvent => this.viewModel?.SetScript(changeEvent.newValue));
			textField.RegisterCallback<KeyDownEvent>(
				keyDownEvent =>
				{
					if(keyDownEvent.ctrlKey && keyDownEvent.keyCode == KeyCode.Return)
						this.viewModel?.Execute();
				}
			);

			this._justEnabled = true;
		}

		void Update()
		{
			if (this._justEnabled)
				OnJustEnabled();
			this._justEnabled = false;
		}

		private void OnJustEnabled()
		{
			GetComponent<UIDocument>().rootVisualElement.Q(TextInputBaseField<string>.textInputUssName).Focus();
		}
	}
}
