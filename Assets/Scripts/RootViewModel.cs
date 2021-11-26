namespace GoC
{
	public interface ILoading
	{
		float progress { get; set; }
	}

	public interface IMainMenu
	{

	}

	public interface IRootViewModel
	{
		ILoading loading { get; set; }
		IMainMenu mainMenu { get; set; }
	}

	public class RootViewModel : IRootViewModel
	{
		public ILoading loading { get; set; }
		public IMainMenu mainMenu { get; set; }
	}
}
