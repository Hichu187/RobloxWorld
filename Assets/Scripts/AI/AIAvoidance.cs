using Sirenix.OdinInspector;
using UnityEngine;
using Vertx.Debugging;

namespace Game
{
    public class AIAvoidance : MonoBehaviour
    {
        [Title("Config")]
        [SerializeField] private LayerMask _layerMaskCharacter;
        [SerializeField] private LayerMask _layerMaskObstacle;
        [SerializeField] private Vector3 _checkBlockStart = new Vector3(0f, 0.1f, 0f);
        [SerializeField] private Vector3 _checkBlockEnd = new Vector3(0f, 0.1f, 1f);
        [SerializeField] private Vector3 _checkCharacterStart = new Vector3(0f, 0.1f, 0f);
        [SerializeField] private Vector3 _checkCharacterEnd = new Vector3(0f, 0.1f, 1f);
        [SerializeField] private Vector3 _checkFallStart = new Vector3(0f, 0.5f, 0.4f);
        [SerializeField] private Vector3 _checkFallEnd = new Vector3(0f, -0.5f, 0.4f);

        private AI _ai;

        private void Awake()
        {
            _ai = GetComponent<AI>();
        }

        private void OnDrawGizmos()
        {
            _ai = GetComponent<AI>();

            D.raw(new Shape.Line(_ai.character.transformCached.TransformPoint(_checkBlockStart), _ai.character.transformCached.TransformPoint(_checkBlockEnd)), Color.green);
            D.raw(new Shape.Line(_ai.character.transformCached.TransformPoint(_checkFallStart), _ai.character.transformCached.TransformPoint(_checkFallEnd)), Color.blue);
            D.raw(new Shape.Line(_ai.character.transformCached.TransformPoint(_checkCharacterStart), _ai.character.transformCached.TransformPoint(_checkCharacterEnd)), Color.cyan);
        }

        private RaycastHit GetRaycastHit(Vector3 start, Vector3 end, Transform relativeTransform, LayerMask layerMask)
        {
            start = relativeTransform.TransformPoint(start);
            end = relativeTransform.TransformPoint(end);
            float distance = Vector3.Distance(start, end);

            RaycastHit hit;

            DrawPhysics.Raycast(start, (end - start).normalized, out hit, distance, layerMask);

            return hit;
        }

        public bool IsObstacleAhead()
        {
            RaycastHit hit = GetRaycastHit(_checkBlockStart, _checkBlockEnd, _ai.characterFoward, _layerMaskObstacle);

            if (hit.collider == null)
                return false;

            float hitAngle = Vector3.Angle(hit.normal, _ai.characterFoward.up);

            // Check if slope
            if (hitAngle < 45f)
                return false;

            return true;
        }

        public bool IsFallAhead()
        {
            RaycastHit hit = GetRaycastHit(_checkFallStart, _checkFallEnd, _ai.character.transformCached, _layerMaskObstacle);

            return hit.collider == null || hit.collider.CompareTag("Kill");
        }

        public bool IsCharacterAhead()
        {
            RaycastHit hit = GetRaycastHit(_checkCharacterStart, _checkCharacterEnd, _ai.character.transformCached, _layerMaskCharacter);

            return hit.collider != null;
        }

        public Collider GetCharacterAhead()
        {
            RaycastHit hit = GetRaycastHit(_checkCharacterStart, _checkCharacterEnd, _ai.character.transformCached, _layerMaskCharacter);

            return hit.collider;
        }
    }
}
