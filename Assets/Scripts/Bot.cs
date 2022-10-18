using System;
using System.Threading;
using System.Threading.Tasks;
using Jint;
using UnityEngine;

namespace GoC
{
	public class Bot : RuntimeApiMarker
	{
		[SerializeField] private float _speed = 1.0f;
		[SerializeField] private float _angularSpeed = 90.0f;
		[SerializeField] private LayerMask _walkableLayerMask;

		private SynchronizationContext _synchronizationContext;
		private Transform _transform;

		public override void SetupRuntimeApi(Engine engine, CancellationToken executionCt)
		{
			engine.SetValue("move", (Action<int>)(distance => this.Move(distance, executionCt)));
			engine.SetValue("turn", (Action<int>)(distance => this.Turn(distance, executionCt)));
		}

		void Awake()
		{
			this._synchronizationContext = SynchronizationContext.Current;
			this._transform = this.transform;
		}

		private void Move(int distance, CancellationToken executionCt) => Post(() => MoveImpl(distance, executionCt));
		private void Turn(int angle, CancellationToken executionCt) => Post(() => TurnImpl(angle, executionCt));

		private void Post(Func<Task> f)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			this._synchronizationContext.Post(
				async state =>
				{
					TaskCompletionSource<bool> stateTcs = (TaskCompletionSource<bool>)state;
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

		private async Task MoveImpl(int distance, CancellationToken executionCt)
		{
			int absDistance = Math.Abs(distance);
			int step = Math.Sign(distance);
			for (int i = 0; i < absDistance && !executionCt.IsCancellationRequested; ++i)
				await MoveBy(step);
		}

		private async Task MoveBy(int step)
		{
			Vector3 target = this._transform.position + step * (Vector3)(this._transform.localToWorldMatrix * Vector3.forward);

			if (!Physics.CheckSphere(target, 0, this._walkableLayerMask))
				throw new Exception("Can't move there");

			// round target coordinates so that position remains in whole numbers
			target.x = Mathf.Round(target.x);
			target.y = Mathf.Round(target.y);
			target.z = Mathf.Round(target.z);

			while (true)
			{
				if (this == null || !this.isActiveAndEnabled) // cancel movement when this Bot is disabled or destroyed
					throw new OperationCanceledException();

				float d = this._speed * Time.deltaTime;
				Vector3 dir = target - this._transform.position;
				if (dir.magnitude < d)
					break;

				this._transform.position += d * dir.normalized;
				await Task.Yield(); // skip a frame
			}

			this._transform.position = target;
		}

		private async Task TurnImpl(int angle, CancellationToken executionCt)
		{
			float startAngle = this.transform.rotation.eulerAngles.y;
			float endAngle = MathF.Round(startAngle) + (angle / 90) * 90;
			Quaternion targetRotation = Quaternion.Euler(this.transform.rotation.eulerAngles.x, endAngle, this.transform.rotation.eulerAngles.z);

			while (true)
			{
				float dAngle = this._angularSpeed * Time.deltaTime;
				float currentAngle = Quaternion.Angle(this.transform.rotation, targetRotation);
				if(currentAngle < dAngle)
					break;

				this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, targetRotation, this._angularSpeed * Time.deltaTime);
				await Task.Yield(); // skip a frame
			}

			this.transform.rotation = targetRotation;
		}
	}
}
