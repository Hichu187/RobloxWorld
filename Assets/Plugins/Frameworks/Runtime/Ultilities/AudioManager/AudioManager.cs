using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Hichu
{
    public class AudioManager : MonoSingleton<AudioManager>
    {
        public static LValue<float> volumeMusic = new LValue<float>(1.0f);
        public static LValue<float> volumeSound = new LValue<float>(1.0f);

        private IObjectPool<AudioScript> _pool;
        private static readonly Dictionary<int, AudioScript> _single = new Dictionary<int, AudioScript>(2);

        protected override void Awake()
        {
            base.Awake();
            InitPool();
        }
        protected override void OnDestroy()
        {
            if (_pool is ObjectPool<AudioScript> p) p.Dispose();
        }

        public static AudioScript Play(AudioConfig config, bool loop = false, Transform pos = null)
        {
            if (config == null || config.clip == null || Instance == null) return null;
            var audio = Instance._pool.Get();
            if (config.is3D)
            {
                var t = audio.transform;
                t.position = pos != null ? pos.position : Instance.transform.position;
            }
            audio.Play(config, loop);
            return audio;
        }

        public static AudioScript PlaySingle(AudioConfig config, bool loop = false, int channel = 0, Transform pos = null)
        {
            if (config == null || config.clip == null || Instance == null) return null;

            if (_single.TryGetValue(channel, out var existing) && existing != null)
            {
                existing.Stop();
                _single[channel] = null;
            }

            var a = Instance._pool.Get();

            if (config.is3D)
            {
                var t = a.transform;
                t.position = pos != null ? pos.position : Instance.transform.position;
            }

            a.Play(config, loop);
            _single[channel] = a;
            return a;
        }

        public static void StopSingleAudio() => StopSingleAudio(0);

        public static void StopSingleAudio(int channel)
        {
            if (_single.TryGetValue(channel, out var a) && a != null)
            {
                a.Stop();
                _single[channel] = null;
            }
        }

        public static void StopAllSingles()
        {
            if (_single.Count == 0) return;
            var keys = new List<int>(_single.Keys);
            foreach (var k in keys)
            {
                if (_single[k] != null) _single[k].Stop();
                _single[k] = null;
            }
        }

        public static void ReturnPool(AudioScript audioScript)
        {
            if (audioScript == null || Instance == null) return;

            int? found = null;
            foreach (var kv in _single)
            {
                if (ReferenceEquals(kv.Value, audioScript)) { found = kv.Key; break; }
            }
            if (found.HasValue) _single[found.Value] = null;

            Instance._pool.Release(audioScript);
        }

        private void InitPool()
        {
            const int defaultCapacity = 8;
            const int maxSize = 64;

            _pool = new ObjectPool<AudioScript>(
                () =>
                {
                    var go = new GameObject(nameof(AudioScript), typeof(AudioSource));
                    go.transform.SetParent(this.transform, false);
                    var a = go.AddComponent<AudioScript>();
                    return a;
                },
                audio => { audio.gameObject.SetActive(true); },
                audio => { audio.gameObject.SetActive(false); },
                audio => { if (audio != null) Destroy(audio.gameObject); },
                false,
                defaultCapacity,
                maxSize
            );
        }
    }
}
