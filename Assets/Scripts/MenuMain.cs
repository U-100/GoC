using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GoC
{
	public class MenuMain : ExclusiveMainBehaviour
	{
		protected override void Run()
		{
		}

		public async void LoadLevel(string levelName)
		{
			Debug.Log($"LoadLevel(\"{levelName}\")");
			
			this.gameObject.SetActive(false);

			AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("Test", LoadSceneMode.Additive);

			while (!asyncOperation.isDone)
				await Task.Yield();

			FindObjectOfType<LevelMain>().RunLevel();
		}
	}
}
