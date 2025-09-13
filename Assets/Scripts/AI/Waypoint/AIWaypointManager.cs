using Hichu;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class AIWaypointManager : MonoSingleton<AIWaypointManager>
    {
        protected override bool _dontDestroyOnLoad { get { return false; } }

        [Title("Reference")]
        [SerializeField] private AIWaypoint[] _waypoints;

        public AIWaypoint GetNearestWaypoint(Vector3 position)
        {
            AIWaypoint nearestWaypoint = _waypoints[0];
            float nearestDistance = Vector3.Distance(position, _waypoints[0].transformCached.position);

            for (int i = 1; i < _waypoints.Length; i++)
            {
                float d = Vector3.Distance(position, _waypoints[i].transformCached.position);

                if (d < nearestDistance)
                {
                    nearestDistance = d;
                    nearestWaypoint = _waypoints[i];
                }
            }

            return nearestWaypoint;
        }

        public void Construct()
        {
            _waypoints = GetComponentsInChildren<AIWaypoint>();
        }

#if UNITY_EDITOR
        [Button]
        public void ConstructInEditMode()
        {
            List<AIWaypoint> waypoints = new List<AIWaypoint>();

            transform.RecursiveAction<AIWaypoint>((waypoint) =>
            {
                if (waypoint == null)
                    return;

                waypoints.Add(waypoint);
            });

            for (int i = 0; i < waypoints.Count; i++)
            {
                List<AIWaypoint> next = new List<AIWaypoint>(waypoints[i].next);

                for (int j = 0; j < next.Count; j++)
                {
                    if (next[j] == null)
                    {
                        next.RemoveAt(j);
                        j--;
                    }
                }

                if (next.Count != waypoints[i].next.Length)
                {
                    waypoints[i].next = next.ToArray();

                    UnityEditor.EditorUtility.SetDirty(waypoints[i]);
                }

                if (waypoints[i].next.IsNullOrEmpty() && i < waypoints.Count - 1)
                {
                    waypoints[i].SetNext(waypoints[i + 1]);
                    UnityEditor.EditorUtility.SetDirty(waypoints[i]);
                }
            }

            _waypoints = waypoints.ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
