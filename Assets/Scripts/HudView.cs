using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace GoC
{
	public class Hud : IDisposable
	{
		private const int MAX_LOG_LINES = 11;

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
			this.logText += string.IsNullOrEmpty(this.logText) ? condition : "\n" + condition;
			int linesCount = CountLines(this.logText);
			if (linesCount > MAX_LOG_LINES)
				this.logText = TrimLeadingLines(this.logText, linesCount - MAX_LOG_LINES);
		}

		private static int CountLines(string text)
		{
			if (string.IsNullOrEmpty(text))
				return 0;

			int linesCount = 1;
			foreach (char c in text)
				if (c == '\n')
					++linesCount;

			return linesCount;
		}

		private static string TrimLeadingLines(string text, int trimCount) => string.Join('\n', text.Split('\n').Skip(trimCount));
	}

	public class HudView : MonoBehaviour
	{
		private UIDocument _uiDocument;
		private TextField _logTextField;

		public Hud viewModel { get; set; }

		void OnEnable() => (this._uiDocument = GetComponent<UIDocument>()).enabled = false;

		void Update()
		{
			if (this.viewModel != null)
			{
				if (!this._uiDocument.enabled)
				{
					this._uiDocument.enabled = true;

					VisualElement rootVisualElement = this._uiDocument.rootVisualElement;

					Button button = rootVisualElement.Q<Button>("Go");
					button.clicked += () => this.viewModel?.Execute();

					TextField immediateScript = rootVisualElement.Q<TextField>("ImmediateScript");
					immediateScript.verticalScrollerVisibility = ScrollerVisibility.Auto;
					immediateScript.RegisterValueChangedCallback(changeEvent => this.viewModel?.SetScript(changeEvent.newValue));
					immediateScript.RegisterCallback<KeyDownEvent>(
						keyDownEvent =>
						{
							if (keyDownEvent.ctrlKey && keyDownEvent.keyCode == KeyCode.Return)
								this.viewModel?.Execute();
						}
					);

					this._logTextField = rootVisualElement.Q<TextField>("Log");
					immediateScript.Focus();
				}

				if(this._logTextField.value != this.viewModel.logText)
					this._logTextField.value = this.viewModel.logText;
			}
		}
	}
}
