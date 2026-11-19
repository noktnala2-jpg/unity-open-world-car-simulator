using UnityEngine;

namespace OpenRoadSim
{
    /// <summary>
    /// WheelCollider-based arcade-simulation hybrid. Configure four wheel colliders in the inspector;
    /// the bootstrapper creates a working example vehicle automatically.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(VehicleInput))]
    public sealed class VehicleController : MonoBehaviour
    {
        [SerializeField] private VehicleConfig config;
        [SerializeField] private WheelCollider frontLeft, frontRight, rearLeft, rearRight;
        [SerializeField] private Transform frontLeftVisual, frontRightVisual, rearLeftVisual, rearRightVisual;
        [SerializeField] private float fuelLitres = 60f;
        [SerializeField, Range(0f, 1f)] private float condition = 1f;

        private Rigidbody body;
        private VehicleInput input;
        private float engineRpm;
        private int gearIndex;
        private float currentSteer;
        private float lastImpactTime;
        private float impactDamageThreshold = 10f;

        public float SpeedKph => body == null ? 0f : body.velocity.magnitude * 3.6f;
        public float SignedSpeedKph => body == null ? 0f : Vector3.Dot(body.velocity, transform.forward) * 3.6f;
        public float EngineRpm => engineRpm;
        public int CurrentGear => SignedSpeedKph < -1f ? -1 : gearIndex + 1;
        public float FuelFraction => config == null || config.tankCapacityLitres <= 0f ? 0f : fuelLitres / config.tankCapacityLitres;
        public float Condition => condition;
        public float ThrottleInput => input == null ? 0f : input.Throttle;
        public VehicleConfig Config { get => config; set => config = value; }

        public void SetWheels(WheelCollider fl, WheelCollider fr, WheelCollider rl, WheelCollider rr,
            Transform flVisual, Transform frVisual, Transform rlVisual, Transform rrVisual)
        {
            frontLeft = fl; frontRight = fr; rearLeft = rl; rearRight = rr;
            frontLeftVisual = flVisual; frontRightVisual = frVisual;
            rearLeftVisual = rlVisual; rearRightVisual = rrVisual;
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            input = GetComponent<VehicleInput>();
            body.mass = config != null ? config.massKg : 1400f;
            body.centerOfMass = new Vector3(0f, -0.35f, 0f);
            if (config != null) fuelLitres = config.tankCapacityLitres;
        }

        private void FixedUpdate()
        {
            if (frontLeft == null || frontRight == null || rearLeft == null || rearRight == null) return;
            float throttle = fuelLitres > 0f && condition > 0.05f ? input.Throttle : 0f;
            float steer = input.Steering;
            float speed = SignedSpeedKph;
            float maxSteer = config != null ? config.maxSteerAngle : 30f;
            float steerFade = Mathf.Lerp(1f, 0.48f, Mathf.InverseLerp(30f, 180f, Mathf.Abs(speed)));
            currentSteer = Mathf.Lerp(currentSteer, steer * maxSteer * steerFade, Time.fixedDeltaTime * 5f);
            frontLeft.steerAngle = currentSteer;
            frontRight.steerAngle = currentSteer;

            UpdateAutomaticGear(Mathf.Abs(speed), throttle);
            float maxSpeed = config != null ? config.maxSpeedKph : 220f;
            float reverseSpeed = config != null ? config.reverseSpeedKph : 35f;
            float engineTorque = config != null ? config.engineTorqueNm : 420f;
            float gearRatio = throttle < -0.05f && speed < 2f ? (config != null ? config.reverseGearRatio : 3f) :
                (config != null ? config.GetGearRatio(gearIndex) : 1f);
            float speedLimitFactor = speed >= 0f
                ? Mathf.Clamp01((maxSpeed - speed) / 8f)
                : Mathf.Clamp01((reverseSpeed + speed) / 5f);
            float power = throttle * engineTorque * gearRatio * (config != null ? config.finalDrive : 3.4f) * condition * speedLimitFactor;
            float rearShare = config != null ? config.rearTorqueShare : 0.5f;
            float frontTorque = power * (1f - rearShare) * 0.5f;
            float rearTorque = power * rearShare * 0.5f;
            frontLeft.motorTorque = frontTorque;
            frontRight.motorTorque = frontTorque;
            rearLeft.motorTorque = rearTorque;
            rearRight.motorTorque = rearTorque;

            float brake = input.Handbrake ? 6500f : (Mathf.Abs(throttle) < 0.05f ? 100f : 0f);
            if (throttle < -0.05f && speed > 2f || throttle > 0.05f && speed < -2f)
                brake = config != null ? config.brakeTorque : 3000f;
            ApplyBrake(brake);
            ApplyAntiRoll(frontLeft, frontRight);
            ApplyAntiRoll(rearLeft, rearRight);
            UpdateRpm(throttle, gearRatio);
            ConsumeFuel(Mathf.Abs(throttle));
            UpdateWheelVisual(frontLeft, frontLeftVisual);
            UpdateWheelVisual(frontRight, frontRightVisual);
            UpdateWheelVisual(rearLeft, rearLeftVisual);
            UpdateWheelVisual(rearRight, rearRightVisual);
        }

        private void UpdateAutomaticGear(float speed, float throttle)
        {
            int count = config == null || config.gearRatios == null ? 5 : config.gearRatios.Length;
            if (count < 1) count = 1;
            if (throttle < -0.05f && speed < 2f) return;
            float rpm = engineRpm;
            if (rpm > (config != null ? config.redlineRpm * 0.88f : 5700f) && gearIndex < count - 1) gearIndex++;
            else if (rpm < (config != null ? config.redlineRpm * 0.38f : 2470f) && gearIndex > 0) gearIndex--;
        }

        private void UpdateRpm(float throttle, float ratio)
        {
            float idle = config != null ? config.idleRpm : 850f;
            float redline = config != null ? config.redlineRpm : 6500f;
            float drivenRpm = Mathf.Abs(SignedSpeedKph) * 32f * Mathf.Max(0.5f, ratio);
            float target = Mathf.Max(idle, drivenRpm + Mathf.Abs(throttle) * 900f);
            engineRpm = Mathf.Lerp(engineRpm, Mathf.Min(redline, target), Time.fixedDeltaTime * 4f);
        }

        private void ConsumeFuel(float load)
        {
            if (config == null || fuelLitres <= 0f) return;
            fuelLitres = Mathf.Max(0f, fuelLitres - config.litresPerHourAtFullLoad * load * Time.fixedDeltaTime / 3600f);
        }

        private void ApplyBrake(float torque)
        {
            frontLeft.brakeTorque = torque; frontRight.brakeTorque = torque;
            rearLeft.brakeTorque = torque; rearRight.brakeTorque = torque;
        }

        private void ApplyAntiRoll(WheelCollider left, WheelCollider right)
        {
            if (config == null || config.antiRollStiffness <= 0f) return;
            WheelHit hit;
            float travelL = 1f, travelR = 1f;
            if (left.GetGroundHit(out hit)) travelL = (-left.transform.InverseTransformPoint(hit.point).y - left.radius) / left.suspensionDistance;
            if (right.GetGroundHit(out hit)) travelR = (-right.transform.InverseTransformPoint(hit.point).y - right.radius) / right.suspensionDistance;
            float force = (travelL - travelR) * config.antiRollStiffness * body.mass * 9.81f;
            if (left.isGrounded) body.AddForceAtPosition(left.transform.up * -force, left.transform.position);
            if (right.isGrounded) body.AddForceAtPosition(right.transform.up * force, right.transform.position);
        }

        private static void UpdateWheelVisual(WheelCollider wheel, Transform visual)
        {
            if (visual == null) return;
            Vector3 position; Quaternion rotation;
            wheel.GetWorldPose(out position, out rotation);
            visual.SetPositionAndRotation(position, rotation * Quaternion.Euler(0f, 0f, 90f));
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (Time.time - lastImpactTime < 0.25f || collision.relativeVelocity.magnitude < impactDamageThreshold) return;
            lastImpactTime = Time.time;
            condition = Mathf.Clamp01(condition - (collision.relativeVelocity.magnitude - impactDamageThreshold) * 0.0025f);
        }

        public bool Refuel(float litres)
        {
            if (config == null || litres <= 0f) return false;
            float before = fuelLitres;
            fuelLitres = Mathf.Min(config.tankCapacityLitres, fuelLitres + litres);
            return fuelLitres > before;
        }

        public void Repair(float amount01) => condition = Mathf.Clamp01(condition + Mathf.Max(0f, amount01));
    }
}
