using UnityEngine;

namespace GoC
{
    public class LevelButton : MonoBehaviour
    {
        [SerializeField] private MenuMain _menuMain;
        [SerializeField] private string _levelSceneName;

        void OnClick()
        {
            _menuMain.LoadLevel(_levelSceneName);
        }
    }
}
