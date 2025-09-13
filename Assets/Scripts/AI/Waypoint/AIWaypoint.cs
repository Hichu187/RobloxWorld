using Hichu;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using Vertx.Debugging;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game
{
    [SelectionBase]
    public class AIWaypoint : MonoCached
    {
        [Title("Reference")]
        [SerializeField] private AIWaypoint[] _next;

        [Title("Config")]
        [SerializeField] private AIWaypointType _type;
        [SerializeField] private float _x = 1f;
        [SerializeField] private float _z = 1f;
        [ShowIf("@_type == AIWaypointType.WaitForDistance")]
        [SerializeField] private float _radius;

        public AIWaypoint[] next { get { return _next; } set { _next = value; } }

        public AIWaypointType type { get { return _type; } }
        public float radius { get { return _radius; } }

        public Vector3 GetRandomPosition()
        {
            Vector3 randomPos = Vector3.zero;

            randomPos.x = Random.Range(-0.5f, 0.5f) * _x;
            randomPos.z = Random.Range(-0.5f, 0.5f) * _z;

            return transformCached.TransformPoint(randomPos);
        }

        public bool IsReached(Vector3 position)
        {
            Vector3 localPos = transformCached.InverseTransformPoint(position);

            if (Mathf.Abs(localPos.x) <= _x * 0.5f && Mathf.Abs(localPos.z) <= _z * 0.5f)
                return true;

            return false;
        }

        public void SetSize(float x, float z)
        {
            _x = x;
            _z = z;
        }

        public void SetNext(params AIWaypoint[] next)
        {
            _next = next;
        }

#if UNITY_EDITOR

        [Button]
        private void AddNext()
        {
            List<AIWaypoint> waypoints = GetNextList();

            AIWaypoint waypoint = SpawnNew();

            waypoint.transformCached.position = transformCached.position + Vector3.forward;
            waypoint.transformCached.SetSiblingIndex(transformCached.GetSiblingIndex() + 1);
            waypoint.SetSize(_x, _z);

            waypoints.Add(waypoint);

            _next = waypoints.ToArray();

            Selection.activeGameObject = waypoint.gameObject;
        }

        [Button]
        private void InsertNext()
        {
            List<AIWaypoint> waypoints = GetNextList();

            AIWaypoint waypoint = SpawnNew();

            waypoint.transformCached.position = transformCached.position + Vector3.forward;
            waypoint.transformCached.SetSiblingIndex(transformCached.GetSiblingIndex() + 1);
            waypoint.SetSize(_x, _z);
            waypoint._next = _next;

            waypoints.Add(waypoint);

            _next = waypoints.ToArray();

            Selection.activeGameObject = waypoint.gameObject;
        }

        private List<AIWaypoint> GetNextList()
        {
            List<AIWaypoint> waypoints = new List<AIWaypoint>(_next);

            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] == null)
                {
                    waypoints.RemoveAt(i);
                    i--;
                }
            }

            return waypoints;
        }

        private void ValidateNext()
        {
            List<AIWaypoint> list = GetNextList();

            if (list.Count != _next.Length)
            {
                _next = list.ToArray();

                EditorUtility.SetDirty(this);
            }
        }

        private AIWaypoint SpawnNew()
        {
            // Get the Path to Prefab
            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);

            // Load Prefab Asset as Object from path
            Object newObject = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(Object));

            //Instantiate the Prefab in the scene, as a child of the GO this script runs on
            return (PrefabUtility.InstantiatePrefab(newObject, transformCached.parent) as GameObject).GetComponent<AIWaypoint>();
        }

        [Button]
        private void SnapToGround()
        {
            RaycastHit hit;

            D.raw(new Shape.Line(transform.position + Vector3.up * 5f, transform.position + Vector3.down * 5f), Color.red, 5f);

            if (Physics.Raycast(transform.position + Vector3.up * 5f, Vector3.down, out hit, 10f))
                transform.position = hit.point;

            EditorUtility.SetDirty(transform);
        }

        private void OnDrawGizmos()
        {
            D.raw(new Shape.Box(transformCached.position, new Vector3(_x, 0f, _z) * 0.5f, transformCached.rotation), Color.yellow);

            Color lineColor = Color.HSVToRGB((0.1f * transformCached.GetSiblingIndex()) % 1f, 1f, 1f);

            for (int i = 0; i < _next.Length; i++)
            {
                if (_next[i] == null)
                {
                    ValidateNext();
                    return;
                }

                D.raw(new Shape.Line(transformCached.position, _next[i].transformCached.position), lineColor);
            }

            if (_type == AIWaypointType.WaitForDistance)
                D.raw(new Shape.Circle(transformCached.position, transformCached.up, _radius), Color.cyan);
        }

#endif
    }
}
