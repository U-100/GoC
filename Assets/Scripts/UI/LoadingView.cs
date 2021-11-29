using UnityEngine;
using UnityEngine.UIElements;

namespace GoC
{
	public class Loading
	{
		public float progress { get; set; }

		public void SetProgress(float p) => this.progress = p;
	}

	public class LoadingView : MonoBehaviour
	{
		public Loading viewModel { get; set; }

		void Update()
		{
			GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("ProgressBar").style.width = new Length(this.viewModel?.progress * 100 ?? 0, LengthUnit.Percent);
		}
	}
}
