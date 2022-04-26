using System;
using System.Threading;
using System.Threading.Tasks;
using Jint;
using UnityEngine;

namespace GoC
{
	public class Bot : MonoBehaviour, IRuntimeApiMarker
	{
		[SerializeField]
		private float _speed = 1.0f;

		private SynchronizationContext _synchronizationContext;
		private Transform _transform;

		public void Move(int distance) => Post(() => MoveOnUnityMainThread(distance));

		void IRuntimeApiMarker.SetupRuntimeApi(Engine engine)
		{
			engine.SetValue("move", (Action<int>)this.Move);
		}

		void Start()
		{
			this._synchronizationContext = SynchronizationContext.Current;
			this._transform = this.transform;
		}

		private void Post(Func<Task> f)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			this._synchronizationContext.Post(
				async state =>
				{
					TaskCompletionSource<bool> stateTcs = (TaskCompletionSource<bool>) state;
					try
					{
						await f();
						stateTcs.SetResult(true);
					}
					catch (Exception ex)
					{
						stateTcs.SetException(ex);
					}
				},
				tcs
			);
			tcs.Task.Wait();
		}

		private async Task MoveOnUnityMainThread(float distance)
		{
			Vector3 target = this._transform.position + distance * (Vector3)(this._transform.localToWorldMatrix * Vector3.forward);

			while (true)
			{
				float d = this._speed * Time.deltaTime;
				Vector3 dir = target - this._transform.position;
				if (dir.magnitude < d)
					break;

				this._transform.position += d * dir.normalized;

				await Task.Yield(); // skip a frame
			}

			this._transform.position = target;
		}
	}
}
