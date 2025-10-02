using UnityEngine;

namespace Game
{
    public class LookAtCamera : MonoBehaviour
    {
        private void LateUpdate()
        {
            transform.LookAt(Camera.main.transform);
        }
    }
}
