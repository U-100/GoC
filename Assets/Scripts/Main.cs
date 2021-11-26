using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace GoC
{
	public class Main : MonoBehaviour
	{
		[SerializeField] private UIDocument _uiDocument = null;
		[SerializeField] private Hero _hero = null;

		private Dictionary<string, object> _api;
		private ScriptRunner _scriptRunner;

		void Start()
		{
			this._api = new Dictionary<string, object>
			{
				["log"] = new Action<object>(Debug.Log),
				["hero"] = this._hero,
				["north"] = Vector3.forward,
				["south"] = Vector3.back,
				["west"] = Vector3.left,
				["east"] = Vector3.right,
			};

			this._scriptRunner = new ScriptRunner();

			TextField immediateScript = this._uiDocument.rootVisualElement.Q<TextField>("ImmediateScript");
			Button go = this._uiDocument.rootVisualElement.Q<Button>("Go");
			go.clicked += () => ExecuteScript(immediateScript.value);
		}

		public async void ExecuteScript(string script)
		{
			await this._scriptRunner.Execute(this._api, script);
		}



		//[SerializeField] private Transform _box = null;

		//private SynchronizationContext _synchronizationContext;

		//void Start()
		//{
		//	this._synchronizationContext = SynchronizationContext.Current;

		//	TextAsset textAsset = Resources.Load<TextAsset>("main.js");
		//	string script = textAsset.text;
		//	RuntimeWrapper runtimeWrapper = new RuntimeWrapper(SynchronizationContext.Current, this);
		//	Task.Run(
		//		() =>
		//		{
		//			Engine engine = new Engine();
		//			engine.SetValue("Runtime", runtimeWrapper);
		//			engine.SetValue("log", new Action<object>(m => Debug.Log(m)));
		//			engine.Execute(script);
		//		}
		//	);
		//}

		//async Task IRuntimeAsync.MoveBox()
		//{
		//	while (this._box.position.x < 2)
		//	{
		//		this._box.Translate(Time.deltaTime * Vector3.right);
		//		await Task.Yield();
		//	}
		//}
	}

	public interface IRuntime
	{
		void MoveBox();
	}

	public interface IRuntimeAsync
	{
		Task MoveBox();
	}

	public class RuntimeWrapper : IRuntime
	{
		private readonly SynchronizationContext _synchronizationContext;
		private readonly IRuntimeAsync _wrapped;

		public RuntimeWrapper(SynchronizationContext synchronizationContext, IRuntimeAsync wrapped)
		{
			this._synchronizationContext = synchronizationContext;
			this._wrapped = wrapped;
		}

		public void MoveBox()
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			this._synchronizationContext.Post(
				async _ =>
				{
					try
					{
						await this._wrapped.MoveBox();
						tcs.SetResult(true);
					}
					catch (Exception ex)
					{
						tcs.SetException(ex);
					}
				},
				null
			);
			tcs.Task.Wait();
		}
	}
}