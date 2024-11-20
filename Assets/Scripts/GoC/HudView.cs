using UnityEngine;
using UnityEngine.UIElements;

namespace GoC
{
    public class HudView : MonoBehaviour
    {
        private UIDocument _uiDocument;

        public Hud viewModel { get; set; }

        void OnEnable() => (_uiDocument = GetComponent<UIDocument>()).enabled = false;

        void Update()
        {
            if (viewModel != null)
            {
                if (!_uiDocument.enabled)
                {
                    _uiDocument.enabled = true;

                    VisualElement rootVisualElement = _uiDocument.rootVisualElement;
                    rootVisualElement.dataSource = viewModel;

                    Button button = rootVisualElement.Q<Button>("Go");
                    button.clicked += viewModel.Execute;

                    TextField immediateScript = rootVisualElement.Q<TextField>("ImmediateScript");

                    // make Ctrl+Enter in the "ImmediateScript" text field trigger the "Go" button
                    immediateScript.RegisterCallback<KeyDownEvent>(
                        keyDownEvent =>
                        {
                            if (keyDownEvent.ctrlKey && keyDownEvent.keyCode == KeyCode.Return)
                                using (var navigationSubmitEvent = new NavigationSubmitEvent() { target = button })
                                    button.SendEvent(navigationSubmitEvent);
                        }
                    );

                    immediateScript.Focus();
                }
            }
        }
    }
}
