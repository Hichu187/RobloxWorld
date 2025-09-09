using Hichu;
using UnityEngine;

namespace Game
{
    public class TowerWall : MonoBehaviour
    {
        public void SetHeight(float height)
        {
            Transform target = transform.GetChild(0);

            MeshFilter renderer = target.GetComponent<MeshFilter>();

            target.SetScaleY(height / renderer.sharedMesh.bounds.size.y);
        }
    }
}
