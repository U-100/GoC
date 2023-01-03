using UnityEngine;

namespace GoC
{
	public abstract class ExclusiveMainBehaviour : MonoBehaviour
	{
		private static bool _exclusiveSlot;
		private bool _shouldStart;

		protected virtual void Awake()
		{
			// this is considered to always happen on Unity's main thread
			if (!_exclusiveSlot)
			{
				_exclusiveSlot = true;
				this._shouldStart = true;
				Application.quitting += () => _exclusiveSlot = false;
			}
		}

		protected virtual void Start()
		{
			if (this._shouldStart)
				Run();
		}

		protected abstract void Run();
	}
}
