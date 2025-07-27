using UnityEngine;

namespace GoC.Arena
{
    [CreateAssetMenu]
    public class TeamConfig : ScriptableObject
    {
        public static TeamConfig Clone(TeamConfig rhs)
        {
            TeamConfig teamConfig = CreateInstance<TeamConfig>();
            teamConfig._starterUnitParams = rhs._starterUnitParams;
            teamConfig._scriptFile = rhs._scriptFile;
            teamConfig._teamSkin = rhs._teamSkin;
            return teamConfig;
        }

        public TeamConfig WithScript(string scriptFile)
        {
            _scriptFile = scriptFile;
            return this;
        }
        
        [SerializeField] private UnitParams _starterUnitParams;
        public UnitParams starterUnitParams => _starterUnitParams;
        
        [SerializeField] private string _scriptFile;
        public string scriptFile => _scriptFile;

        [SerializeField] private TeamSkin _teamSkin;
        public TeamSkin teamSkin => _teamSkin;
    }
}
