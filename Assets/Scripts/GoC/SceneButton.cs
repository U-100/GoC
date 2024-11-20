using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GoC
{
    public class SceneButton : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private UnityEvent<string> _onPointerClick;

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("OnPointerClick");
            _onPointerClick?.Invoke("Test");
        }
    }
}
