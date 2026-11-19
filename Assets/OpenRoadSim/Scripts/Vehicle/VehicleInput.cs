using UnityEngine;

namespace OpenRoadSim
{
    /// <summary>Reads Unity's legacy Horizontal/Vertical axes plus Space for handbrake.</summary>
    public sealed class VehicleInput : MonoBehaviour
    {
        public float Steering { get; private set; }
        public float Throttle { get; private set; }
        public bool Handbrake { get; private set; }
        public bool EnabledInput { get; set; } = true;

        private void Update()
        {
            if (!EnabledInput)
            {
                Steering = 0f;
                Throttle = 0f;
                Handbrake = false;
                return;
            }

            Steering = Mathf.Clamp(Input.GetAxis("Horizontal"), -1f, 1f);
            Throttle = Mathf.Clamp(Input.GetAxis("Vertical"), -1f, 1f);
            Handbrake = Input.GetKey(KeyCode.Space);
        }
    }
}
