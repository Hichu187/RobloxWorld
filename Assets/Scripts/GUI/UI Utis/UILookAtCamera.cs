using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class UILookAtCamera : MonoBehaviour
    {
        Camera cam;
        void Start()
        {
            cam = Camera.main;

        }

        void Update()
        {
            if (cam != null)
            {
                Vector3 directionToCamera = cam.transform.position - transform.position;

                directionToCamera.y = 0f;

                if (directionToCamera.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(-directionToCamera);
                    transform.rotation = targetRotation;
                }
            }
        }
    }
}
