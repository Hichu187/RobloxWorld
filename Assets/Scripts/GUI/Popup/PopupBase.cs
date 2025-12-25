using Hichu;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public abstract class PopupBase : MonoBehaviour
    {
        public Button btn_Get;
        public Button btn_nothnanks;

        private Coroutine nothankCoroutine;
        public View view;

        protected virtual void Start()
        {
            StartCoroutine(NothankCoroutine());

            view = GetComponent<View>();

        }
        private IEnumerator NothankCoroutine()
        {
            btn_nothnanks.gameObject.SetActive(false);
            yield return new WaitForSeconds(2f);
            btn_nothnanks.gameObject.SetActive(true);
        }

    }
}
