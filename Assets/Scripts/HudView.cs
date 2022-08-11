using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace GoC
{
	public class Hud
	{
		private TaskCompletionSource<bool> _executeTcs;

		public string script { get; set; } = "";

		public async IAsyncEnumerable<string> GetScriptsAsync([EnumeratorCancellation] CancellationToken ct)
		{
			while (!ct.IsCancellationRequested)
			{
				this._executeTcs = new TaskCompletionSource<bool>();
				using (ct.Register(this.StopGetScriptsAsync))
				{
					if (await this._executeTcs.Task)
						yield return this.script;
					else
						break;
				}
			}
		}

		public void SetScript(string s) => this.script = s;

		public void Execute()
		{
			TaskCompletionSource<bool> tempExecuteTcs = this._executeTcs;
			this._executeTcs = null;
			tempExecuteTcs?.SetResult(true);
		}

		private void StopGetScriptsAsync()
		{
			TaskCompletionSource<bool> tempExecuteTcs = this._executeTcs;
			this._executeTcs = null;
			tempExecuteTcs?.SetResult(false);
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
					if (keyDownEvent.ctrlKey && keyDownEvent.keyCode == KeyCode.Return)
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
