using System;

namespace GoC
{
	public static class ActionFloatProgressExtensions
	{
		public static Action<float> Slice(this Action<float> progress, float from, float to) => x => progress(@from + Math.Clamp(x, 0, 1) * (to - @from));
	}
}
