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
		[SerializeField] private LayerMask _usableLayerMask;

		private SynchronizationContext _synchronizationContext;
		private Transform _transform;

		private readonly Collider[] _tempSpatialQueryResult = new Collider[1];

		public override void SetupRuntimeApi(Engine engine, CancellationToken executionCt)
		{
			engine.SetValue("move", (Action<int>)(distance => Move(distance, executionCt)));
			engine.SetValue("turn", (Action<int>)(angle => Turn(angle, executionCt)));
			engine.SetValue("use", (Action<int>)(distance => Use(executionCt)));
		}

		void Awake()
		{
			_synchronizationContext = SynchronizationContext.Current;
			_transform = transform;
		}

		private void Move(int distance, CancellationToken executionCt) => Post(() => MoveImpl(distance, executionCt));
		private void Turn(int angle, CancellationToken executionCt) => Post(() => TurnImpl(angle, executionCt));
		private void Use(CancellationToken executionCt) => Post(() => UseImpl(executionCt));

		private void Post(Func<Task> f)
		{
			TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
			_synchronizationContext.Post(
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
			Vector3 target = _transform.position + step * (Vector3)(_transform.localToWorldMatrix * Vector3.forward);

			if (!Physics.CheckSphere(target, 0, _walkableLayerMask))
				throw new Exception("Can't move there");

			// round target coordinates so that position remains in whole numbers
			target.x = Mathf.Round(target.x);
			target.y = Mathf.Round(target.y);
			target.z = Mathf.Round(target.z);

			while (true)
			{
				if (this == null || !isActiveAndEnabled) // cancel movement when this Bot is disabled or destroyed
					throw new OperationCanceledException();

				float d = _speed * Time.deltaTime;
				Vector3 dir = target - _transform.position;
				if (dir.magnitude < d)
					break;

				_transform.position += d * dir.normalized;
				await Task.Yield(); // skip a frame
			}

			_transform.position = target;
		}

		private async Task TurnImpl(int angle, CancellationToken executionCt)
		{
			float startAngle = transform.rotation.eulerAngles.y;
			float endAngle = MathF.Round(startAngle) + (angle / 90) * 90;
			Quaternion targetRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, endAngle, transform.rotation.eulerAngles.z);

			while (true)
			{
				float dAngle = _angularSpeed * Time.deltaTime;
				float currentAngle = Quaternion.Angle(transform.rotation, targetRotation);
				if(currentAngle < dAngle)
					break;

				transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _angularSpeed * Time.deltaTime);
				await Task.Yield(); // skip a frame
			}

			transform.rotation = targetRotation;
		}

		private async Task UseImpl(CancellationToken executionCt)
		{
			Vector3 target = _transform.position + (Vector3)(_transform.localToWorldMatrix * Vector3.forward);

			if (Physics.OverlapSphereNonAlloc(target, 0, _tempSpatialQueryResult, _usableLayerMask) == 0)
				throw new Exception("Nothing usable here");

			IUsable usable = _tempSpatialQueryResult[0].GetComponent<IUsable>();
			if(usable == null)
				throw new Exception("That's unusable");

			await usable.Use(executionCt);
		}
	}
}
