using Hichu;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class BGMStarter : MonoBehaviour
    {
        [SerializeField] private List<AudioConfig> _bgms;

        private void Start()
        {
            int bgmIndex = Random.Range(0, _bgms.Count);
            BGMHelper.Play(_bgms[bgmIndex]);
        }
    }
}
