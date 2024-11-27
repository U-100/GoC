using GoC.ScriptingApi;
using UnityEngine;

namespace GoC.Arena
{
    public class Hull : MonoBehaviour, IHull
    {
        [SerializeField] private int _integrity;
        
        public int integrity => _integrity;

        public bool dead => _integrity <= 0;
        
        public void Absorb(int damage) => _integrity -= damage;
    }
}
