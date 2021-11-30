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
			GetComponent<UIDocument>().rootVisualElement.Q<Button>("Go").clicked += () => this.viewModel?.Execute();
		}

		void Update()
		{
			this.viewModel?.SetScript(GetComponent<UIDocument>().rootVisualElement.Q<TextField>("ImmediateScript").value);
		}
	}
}
