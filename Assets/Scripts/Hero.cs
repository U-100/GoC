using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace GoC
{
	public class Hero : MonoBehaviour, IRuntimeApiMarker
	{
		[SerializeField] private float _walkSpeed = 1;
		[SerializeField] private float _runSpeed = 1;

		private SynchronizationContext _synchronizationContext;
		private Transform _transform;
		private Animator _animator;

		private readonly int ANIMATOR_PARAM_WALKING = Animator.StringToHash("walking");
		private readonly int ANIMATOR_PARAM_RUNNING = Animator.StringToHash("running");

		public void walk(Vector3 delta) => MoveThreadSafe(delta, this._walkSpeed);
		public void run(Vector3 delta) => MoveThreadSafe(4 * delta, this._runSpeed);

		public Vector2 position => new Vector2(0, 0);

		void Start()
		{
			this._synchronizationContext = SynchronizationContext.Current;
			this._transform = this.transform;
			this._animator = this.GetComponent<Animator>();
		}

		private void MoveThreadSafe(Vector3 delta, float speed)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			this._synchronizationContext.Post(
				async _ =>
				{
					try
					{
						await MoveOnUnityMainThread(delta, speed);
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

		private async Task MoveOnUnityMainThread(Vector3 delta, float speed)
		{
			Vector3 target = this._transform.position + delta;

			this._transform.LookAt(target);

			this._animator.SetBool(
				speed <= this._walkSpeed ? this.ANIMATOR_PARAM_WALKING : this.ANIMATOR_PARAM_RUNNING,
				true
			);

			while (true)
			{
				float d = speed * Time.deltaTime;
				Vector3 dir = target - this._transform.position;
				if (dir.magnitude < d)
					break;

				this._transform.position += dir.normalized * speed * Time.deltaTime;

				await Task.Yield(); // skip a frame
			}

			this._transform.position = target;
			this._animator.SetBool(this.ANIMATOR_PARAM_WALKING, false);
			this._animator.SetBool(this.ANIMATOR_PARAM_RUNNING, false);
		}
	}
}
