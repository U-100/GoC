using UnityEngine;

namespace GoC.Arena
{
    [CreateAssetMenu]
    public class TeamConfig : ScriptableObject
    {
        public static TeamConfig Create(string initScriptFile, TeamSkin initTeamSkin)
        {
            TeamConfig teamConfig = CreateInstance<TeamConfig>();
            teamConfig._scriptFile = initScriptFile;
            teamConfig._teamSkin = initTeamSkin;
            return teamConfig;
        }
        
        [SerializeField] private string _scriptFile;
        public string scriptFile => _scriptFile;

        [SerializeField] private TeamSkin _teamSkin;
        public TeamSkin teamSkin => _teamSkin;
    }
}
