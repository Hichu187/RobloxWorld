using Cysharp.Threading.Tasks;
using DG.Tweening;
using Hichu;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Game
{
    public class Announcement_Coundown : MonoBehaviour
    {
        [Title("Reference")]
        [SerializeField] private AnimationSequence _animCountdown;
        [SerializeField] private AnimationSequence _animEnd;

        [SerializeField] private TextMeshProUGUI _txtCountdown;

        private bool _isDestroyed;

        private void OnDestroy()
        {
            _isDestroyed = true;
        }

        public async UniTask Countdown()
        {
            gameObject.SetActive(true);

            _animCountdown.sequence.Complete();

            for (int i = 3; i >= 1; i--)
            {
                _txtCountdown.text = i.ToString();

                _animCountdown.sequence.Restart();
                _animCountdown.sequence.Play();

                await UniTask.WaitForSeconds(1.0f);
            }

            if (!_isDestroyed)
            {
                _txtCountdown.text = "GO!";

                _animEnd.sequence.Restart();
                _animEnd.sequence.Play();
            }

            await UniTask.WaitForSeconds(0.5f);
        }
    }
}
