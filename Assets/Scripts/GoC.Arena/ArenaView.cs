using UnityEngine;
using UnityEngine.UIElements;

namespace GoC.Arena
{
    [RequireComponent(typeof(UIDocument))]
    public class ArenaView : MonoBehaviour
    {
        private UIDocument _uiDocument;
        private ArenaViewModel _viewModel;

        public UIDocument uiDocument => _uiDocument ??= GetComponent<UIDocument>();

        public ArenaViewModel viewModel
        {
            get => _viewModel;
            set
            {
                _viewModel = value;
                
                uiDocument.rootVisualElement.dataSource = _viewModel;

                VisualElement arenaMainMenu = uiDocument.rootVisualElement.Q<VisualElement>("MainMenu");
                
                Button arenaMainMenuPlayButton = arenaMainMenu.Q<Button>("PrepareCodeVsAIButton");
                arenaMainMenuPlayButton.clicked += _viewModel.PrepareCodeVsAI;
                
                Button arenaMainMenuQuitButton = arenaMainMenu.Q<Button>("QuitButton");
                arenaMainMenuQuitButton.clicked += _viewModel.Quit;
                
                VisualElement arenaPlayMenu = uiDocument.rootVisualElement.Q<VisualElement>("CodeVsAIMenu");
                
                Button arenaPlayMenuPlayButton = arenaPlayMenu.Q<Button>("PlayButton");
                arenaPlayMenuPlayButton.clicked += _viewModel.PlayCodeVsAI;
                
                Button arenaPlayMenuBackButton = arenaPlayMenu.Q<Button>("BackButton");
                arenaPlayMenuBackButton.clicked += _viewModel.Back;

                VisualElement arenaHud = uiDocument.rootVisualElement.Q<VisualElement>("Hud");

                Button arenaHudFastButton = arenaHud.Q<Button>("FastButton");
                arenaHudFastButton.clicked += _viewModel.Fast;

                Button arenaHudPlayButton = arenaHud.Q<Button>("PlayButton");
                arenaHudPlayButton.clicked += _viewModel.Play;

                Button arenaHudPauseButton = arenaHud.Q<Button>("PauseButton");
                arenaHudPauseButton.clicked += _viewModel.Pause;

                Button arenaHudStopButton = arenaHud.Q<Button>("StopButton");
                arenaHudStopButton.clicked += _viewModel.Stop;
            }
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        public static void RegisterUIToolkitConverters()
        {
            ConverterGroups.RegisterGlobalConverter((ref bool b) => b ? new StyleEnum<DisplayStyle>(DisplayStyle.Flex) : new StyleEnum<DisplayStyle>(DisplayStyle.None));
            ConverterGroups.RegisterGlobalConverter((ref StyleEnum<DisplayStyle> seds) => seds.value != DisplayStyle.None);
        }
    }
}
