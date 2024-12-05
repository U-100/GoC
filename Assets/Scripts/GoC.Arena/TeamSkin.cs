using UnityEngine;

namespace GoC.Arena
{
    [CreateAssetMenu]
    public class TeamSkin : ScriptableObject
    {
        [SerializeField] private Material _mainColorMaterial;
        public Material mainColorMaterial => _mainColorMaterial;
    }
}
