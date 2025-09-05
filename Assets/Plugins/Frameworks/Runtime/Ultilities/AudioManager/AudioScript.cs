using DG.Tweening;
using UnityEngine;

namespace Hichu
{
    public class AudioScript : MonoCached
    {
        private AudioConfig _config;
        private AudioSource _audioSource;
        private Tween _tween;

        public AudioSource audioSource
        {
            get
            {
                if (_audioSource == null)
                    _audioSource = gameObjectCached.GetComponent<AudioSource>();
                return _audioSource;
            }
        }

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource != null)
            {
                _audioSource.playOnAwake = false;
                _audioSource.loop = false;
            }
        }

        private void OnEnable()
        {
            if (!AudioManager.IsDestroyed && AudioManager.Instance != null)
                transformCached.SetParent(AudioManager.Instance.TransformCached, false);

            _tween?.Kill();
            _tween = null;
            _config = null;

            AudioManager.volumeSound.eventValueChanged += VolumeSound_EventValueChanged;
            AudioManager.volumeMusic.eventValueChanged += VolumeMusic_EventValueChanged;
        }

        private void OnDisable()
        {
            _tween?.Kill();
            _tween = null;

            AudioManager.volumeSound.eventValueChanged -= VolumeSound_EventValueChanged;
            AudioManager.volumeMusic.eventValueChanged -= VolumeMusic_EventValueChanged;
        }

        private void OnDestroy()
        {
            _tween?.Kill();
            _tween = null;

            AudioManager.volumeSound.eventValueChanged -= VolumeSound_EventValueChanged;
            AudioManager.volumeMusic.eventValueChanged -= VolumeMusic_EventValueChanged;
        }

        public void Play(AudioConfig config, bool loop = false)
        {
            if (config == null || config.clip == null) return;

            Construct(config, loop);

            _tween?.Kill();
            _tween = null;

            if (!loop)
            {
                float pitch = Mathf.Approximately(audioSource.pitch, 0f) ? 1f : audioSource.pitch;
                float playSecs = config.clip.length / Mathf.Abs(pitch);
                _tween = DOVirtual.DelayedCall(playSecs, Stop, false);
            }
        }

        public void Stop()
        {
            if (AudioManager.IsDestroyed) return;

            _tween?.Kill();
            _tween = null;

            if (audioSource != null)
                audioSource.Stop();

            AudioManager.ReturnPool(this);
        }

        private float GetVolume()
        {
            if (_config == null) return 0f;
            float global = (_config.type == AudioType.Music) ? AudioManager.volumeMusic.value : AudioManager.volumeSound.value;
            return _config.volumeScale * global;
        }

        private void VolumeSound_EventValueChanged(float _)
        {
            UpdateVolume();
        }

        private void VolumeMusic_EventValueChanged(float _)
        {
            UpdateVolume();
        }

        private void Construct(AudioConfig config, bool loop = false)
        {
            _config = config;

            var src = audioSource;
            src.Stop();
            src.clip = config.clip;
            src.loop = loop;
            src.spatialBlend = config.is3D ? 1f : 0f;
            src.minDistance = config.distance.x;
            src.maxDistance = config.distance.y;

            UpdateVolume();
            src.Play();
        }

        private void UpdateVolume()
        {
            if (audioSource == null) return;

            float volumeFinal = GetVolume();
            audioSource.mute = volumeFinal <= 0f;
            audioSource.volume = volumeFinal;
        }
    }
}
