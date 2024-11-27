using GoC.ScriptingApi;
using UnityEngine;

namespace GoC.Arena
{
    public class Hull : MonoBehaviour, IHull
    {
        [SerializeField] private int _integrity;
        
        public int integrity => _integrity;
    }
}
