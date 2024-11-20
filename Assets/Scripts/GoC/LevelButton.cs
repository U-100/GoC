using UnityEngine;
using UnityEngine.EventSystems;

namespace GoC
{
    public class LevelButton : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private MenuMain _menuMain;
        [SerializeField] private string _levelSceneName;

        public void OnPointerClick(PointerEventData eventData)
        {
            _menuMain.LoadLevel(_levelSceneName);
        }
    }
}
