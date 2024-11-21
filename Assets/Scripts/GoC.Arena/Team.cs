using System;
using System.Collections.Generic;
using System.Linq;
using GoC.ScriptingApi;
using UnityEngine;

namespace GoC.Arena
{
    public class Team : MonoBehaviour, IModel
    {
        [SerializeField] private Arena _arena;
        [SerializeField] private string _scriptFilePath;

        public IArenaContext arena => _arena;
        public string scriptFilePath => _scriptFilePath;

        public int teamId => GetInstanceID();

        private Func<IFactory, bool> _factoryIsMine; // for delegate caching
        public IEnumerable<IFactory> myFactories => factories.Where(_factoryIsMine ??= f => f.teamId == teamId);

        private Func<IUnit, bool> _unitIsMine; // for delegate caching
        public IEnumerable<IUnit> myUnits => units.Where(_unitIsMine ??= u => u.teamId == teamId);

        public IEnumerable<IFactory> factories => FindObjectsByType<FactoryBehaviour>(FindObjectsSortMode.None);
        public IEnumerable<IUnit> units => FindObjectsByType<UnitBehaviour>(FindObjectsSortMode.None);

        private void OnEnable()
        {
            if(_arena != null)
                _arena.TeamEnabled(this);
            else
                Debug.LogWarning("Arena must be set for a team", this);
        }
    }
}
