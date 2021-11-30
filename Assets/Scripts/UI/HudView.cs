using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace GoC
{
	public class Hud
	{
		private TaskCompletionSource<bool> _executeTcs;

		public string script { get; set; }

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
		public Hud viewModel { get; set; }

		void OnEnable()
		{
			VisualElement rootVisualElement = GetComponent<UIDocument>().rootVisualElement;

			Button button = rootVisualElement.Q<Button>("Go");
			button.clicked += () => this.viewModel?.Execute();

			TextField textField = rootVisualElement.Q<TextField>("ImmediateScript");
			textField.RegisterCallback<KeyDownEvent>(
				keyDownEvent =>
				{
					if(keyDownEvent.ctrlKey && keyDownEvent.keyCode == KeyCode.Return)
						this.viewModel?.Execute();
				}
			);
			textField.RegisterValueChangedCallback(changeEvent => this.viewModel?.SetScript(changeEvent.newValue));
		}
	}
}
