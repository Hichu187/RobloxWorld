using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class BrainrotEvoEggCanvas : MonoBehaviour
    {
        [SerializeField] GameObject _root;
        [SerializeField] private List<GameObject> _eggs;
        [SerializeField] Button open;


        private int _openTime = 3;
        private int _currentClick = 0;
        private Animator _curEgg;
        private bool _isLocked = false;

        private void Start()
        {
            open.onClick.AddListener(ClickOpen);
        }

        [Button]
        public void InitEgg(int index)
        {
            _root.SetActive(true);

            foreach (var e in _eggs) e.SetActive(false);
            _eggs[index].SetActive(true);
            _curEgg = _eggs[index].GetComponent<Animator>();
            _currentClick = 0;
            _isLocked = false;
            open.interactable = true;
        }

        [Button]
        public void ClickOpen()
        {
            if (_curEgg == null || _isLocked) return;

            _isLocked = true;
            _currentClick++;

            _curEgg.SetTrigger("Shake");

            if (_currentClick >= _openTime)
            {
                _curEgg.SetTrigger("Shake");
                _curEgg.SetBool("Break", true);
                open.interactable = false;
            }
            else
            {
                StartCoroutine(UnlockAfter(0.8f));
            }
        }

        private IEnumerator UnlockAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            _isLocked = false;
        }
        public void Close()
        {
            _root.SetActive(false);
        }
    }
}
