using UnityEngine;
using UnityEngine.UIElements;

namespace GoC.Arena
{
    public class ArenaMain : MonoBehaviour
    {
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

        [SerializeField] private ArenaView _arenaView;
        
        private readonly ArenaViewModel _arenaViewModel = new ArenaViewModel();

        void Start()
        {
            _arenaView.viewModel = _arenaViewModel;
        }
    }
}
