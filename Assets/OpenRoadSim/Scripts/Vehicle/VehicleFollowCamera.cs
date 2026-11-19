using UnityEngine;

namespace OpenRoadSim
{
    /// <summary>Simple smooth chase camera with speed-responsive follow distance.</summary>
    public sealed class VehicleFollowCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 localOffset = new Vector3(0f, 3.2f, -7.5f);
        [SerializeField, Range(1f, 20f)] private float positionSharpness = 8f;
        [SerializeField, Range(1f, 20f)] private float rotationSharpness = 8f;
        private Rigidbody targetBody;

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            targetBody = target != null ? target.GetComponent<Rigidbody>() : null;
        }

        private void LateUpdate()
        {
            if (target == null) return;
            float speedFactor = targetBody == null ? 0f : Mathf.Clamp01(targetBody.velocity.magnitude / 45f);
            Vector3 desiredPosition = target.TransformPoint(localOffset + new Vector3(0f, speedFactor * 0.6f, -speedFactor * 2f));
            float p = 1f - Mathf.Exp(-positionSharpness * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, p);
            Vector3 lookPoint = target.position + Vector3.up * 1.1f + target.forward * 2f;
            Quaternion desiredRotation = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, 1f - Mathf.Exp(-rotationSharpness * Time.deltaTime));
        }
    }
}
