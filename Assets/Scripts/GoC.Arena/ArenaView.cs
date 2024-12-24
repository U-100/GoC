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
            }
        }
    }
}
