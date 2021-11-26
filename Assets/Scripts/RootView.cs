using UnityEngine.UIElements;

namespace GoC
{
	public class RootView
	{
		private readonly RootViewModel _viewModel;
		private readonly VisualElement _visualElement;

		public RootView(RootViewModel viewModel, VisualElement visualElement)
		{
			this._viewModel = viewModel;
			this._visualElement = visualElement;
		}

		public void Update()
		{
			if (this._viewModel.loading == null)
			{

			}
		}
	}
}
