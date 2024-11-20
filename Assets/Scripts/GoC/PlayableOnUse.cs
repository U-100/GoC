using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace GoC
{
    public class PlayableOnUse : MonoBehaviour, IUsable
    {
        [SerializeField] private PlayableDirector _playableDirector;

        public Task Use(CancellationToken executionCt)
        {
            _playableDirector.Play();
            return Task.Delay(TimeSpan.FromSeconds(_playableDirector.playableAsset.duration), executionCt);
        }
    }
}
