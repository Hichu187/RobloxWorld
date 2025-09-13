using Hichu;
using UnityEngine;

namespace Game
{
    public class AIFollowWaypoint : MonoBehaviour
    {
        [SerializeField] private AI _ai;

        [SerializeField] private AIWaypoint _waypointPrevious;
        [SerializeField] private AIWaypoint _waypoint;
        [SerializeField] private AIWaypoint _waypointNext;

        private void Awake()
        {
            _ai = GetComponent<AI>();
        }

        private void Start()
        {
            _ai.eventIdleComplete += AI_EventIdleComplete;
            _ai.eventChaseComplete += AI_EventChaseComplete;

            _ai.character.eventRevive += Character_EventRevive;

            FollowNearestWaypoint();
        }

        private void AI_EventChaseComplete()
        {
            if (_waypoint.next.IsNullOrEmpty())
            {
                _ai.Idle();
                return;
            }

            // Get next waypoint
            _waypointNext = _waypoint.next.GetRandom();

            if (_waypoint.type == AIWaypointType.WaitForDistance)
                _ai.IdleWaitForDistance(_waypointNext.transformCached, _waypoint.radius);
            else
                _ai.Idle();
        }

        private void AI_EventIdleComplete()
        {
            // If AI not reached position
            if (!_waypoint.IsReached(_ai.character.transformCached.position))
            {
                FollowWaypoint();
                return;
            }

            /*
            // Waypoint to high
            if (_waypoint.transformCached.position.y - _ai.character.transformCached.position.y > 3f)
            {
                FollowNearestWaypoint();
                return;
            }
            */

            if (!_waypoint.next.IsNullOrEmpty())
            {
                FollowNextWaypoint();
            }
            else
            {
                FollowWaypoint();
            }
        }

        private void Character_EventRevive()
        {
            FollowNearestWaypoint();
        }

        private void FollowNearestWaypoint()
        {
            _waypointPrevious = null;
            _waypoint = AIWaypointManager.Instance.GetNearestWaypoint(_ai.character.transformCached.position);
            _waypointNext = null;

            _ai.Chase(_waypoint.GetRandomPosition());
        }

        private void FollowNextWaypoint()
        {
            _waypointPrevious = _waypoint;
            _waypoint = _waypointNext;
            _waypointNext = null;

            FollowWaypoint();
        }

        private void FollowWaypoint()
        {
            if (_waypointPrevious != null && _waypointPrevious.type == AIWaypointType.WaitForDistance)
                _ai.Chase(_waypoint.transformCached);
            else
                _ai.Chase(_waypoint.GetRandomPosition());
        }
    }
}
