using UnityEngine;

namespace Game
{
    public class LookAtCamera : MonoBehaviour
    {
        private void LateUpdate()
        {
            if (Camera.main == null) return;

            Vector3 camPos = Camera.main.transform.position;
            Vector3 lookPos = new Vector3(camPos.x, transform.position.y, camPos.z);

            transform.LookAt(lookPos);
        }
    }
}
