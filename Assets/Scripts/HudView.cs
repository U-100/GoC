using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace GoC
{
	public class Hud : IDisposable
	{
		private TaskCompletionSource<bool> _executeTcs;

		private string _script { get; set; } = "";

		public Hud()
		{
			Application.logMessageReceivedThreaded += OnLogMessageReceived;
		}

		public void Dispose()
		{
			Application.logMessageReceivedThreaded -= OnLogMessageReceived;
		}

		public string logText { get; private set; }

		public async IAsyncEnumerable<string> GetScriptsAsync([EnumeratorCancellation] CancellationToken ct)
		{
			while (!ct.IsCancellationRequested)
			{
				this._executeTcs = new TaskCompletionSource<bool>();
				using (ct.Register(this.StopGetScriptsAsync))
				{
					if (await this._executeTcs.Task)
						yield return this._script;
					else
						break;
				}
			}
		}

		public void SetScript(string s) => this._script = s;

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

		private void OnLogMessageReceived(string condition, string stacktrace, LogType type)
		{
			// TODO: need threading protection
			this.logText += condition + "\n";
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

			TextField immediateScript = rootVisualElement.Q<TextField>("ImmediateScript");
			immediateScript.SetVerticalScrollerVisibility(ScrollerVisibility.Auto);
			immediateScript.RegisterValueChangedCallback(changeEvent => this.viewModel?.SetScript(changeEvent.newValue));
			immediateScript.RegisterCallback<KeyDownEvent>(
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

			UIDocument uiDocument = GetComponent<UIDocument>();
			VisualElement rootVisualElement = uiDocument.rootVisualElement;
			TextField logTextField = rootVisualElement.Q<TextField>("Log");
			logTextField.SetValueWithoutNotify(this.viewModel.logText);
		}

		private void OnJustEnabled()
		{
			GetComponent<UIDocument>().rootVisualElement.Q(TextInputBaseField<string>.textInputUssName).Focus();
		}
	}
}
