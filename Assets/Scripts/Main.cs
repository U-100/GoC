using Jint;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

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

public class Main : MonoBehaviour, IRuntimeAsync
{
	[SerializeField] private Transform _box = null;

	private SynchronizationContext _synchronizationContext;

	void Start()
	{
		this._synchronizationContext = SynchronizationContext.Current;

		TextAsset textAsset = Resources.Load<TextAsset>("main.js");
		string script = textAsset.text;
		RuntimeWrapper runtimeWrapper = new RuntimeWrapper(SynchronizationContext.Current, this);
		Task.Run(
			() =>
			{
				Engine engine = new Engine();
				engine.SetValue("Runtime", runtimeWrapper);
				engine.SetValue("log", new Action<object>(m => Debug.Log(m)));
				engine.Execute(script);
			}
		);
	}

	async Task IRuntimeAsync.MoveBox()
	{
		while (this._box.position.x < 2)
		{
			this._box.Translate(Time.deltaTime * Vector3.right);
			await Task.Yield();
		}
	}
}
