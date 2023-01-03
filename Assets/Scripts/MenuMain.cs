using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GoC
{
	public class MenuMain : ExclusiveMainBehaviour
	{
		protected override async void Run()
		{
			AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Test", LoadSceneMode.Additive);

			while (!asyncOperation.isDone)
				await Task.Yield();

			FindObjectOfType<LevelMain>().RunLevel();
		}
	}
}
