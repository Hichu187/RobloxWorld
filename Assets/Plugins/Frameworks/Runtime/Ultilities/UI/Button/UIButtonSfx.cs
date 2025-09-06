using Hichu;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game
{
    public class UIButtonSfx : MonoBehaviour, IPointerDownHandler
    {
        [Title("Config")]
        [SerializeField] AudioConfig _sfxOverride;

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (_sfxOverride != null)
                AudioManager.Play(_sfxOverride);
/*            else
                AudioManager.Play(FactorySFX.click);*/
        }
    }
}
