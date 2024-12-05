using UnityEngine;

namespace GoC.Arena
{
    [CreateAssetMenu]
    public class TeamConfig : ScriptableObject
    {
        [SerializeField] private string _scriptFile;
        public string scriptFile => _scriptFile;

        [SerializeField] private TeamSkin _teamSkin;
        public TeamSkin teamSkin => _teamSkin;
    }
}
