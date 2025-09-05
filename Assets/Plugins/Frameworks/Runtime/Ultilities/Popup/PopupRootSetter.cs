using UnityEngine;

namespace Hichu
{
    public class PopupRootSetter : MonoBehaviour
    {
        private void Awake()
        {
            PopupManager.SetRoot(transform);
        }
    }
}
