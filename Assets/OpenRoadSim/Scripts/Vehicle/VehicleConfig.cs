using UnityEngine;

namespace OpenRoadSim
{
    [CreateAssetMenu(menuName = "Open Road Sim/Vehicle Configuration", fileName = "VehicleConfig")]
    public sealed class VehicleConfig : ScriptableObject
    {
        [Header("Powertrain")]
        [Min(100f)] public float engineTorqueNm = 420f;
        [Min(1f)] public float maxSpeedKph = 220f;
        [Min(1f)] public float reverseSpeedKph = 35f;
        [Min(1f)] public float idleRpm = 850f;
        [Min(1000f)] public float redlineRpm = 6500f;
        [Min(0.1f)] public float finalDrive = 3.4f;
        [Min(0.1f)] public float[] gearRatios = { 3.2f, 2.1f, 1.5f, 1.15f, 0.9f };
        [Min(0.1f)] public float reverseGearRatio = 3f;
        [Min(0f)] public float engineBraking = 120f;

        [Header("Chassis")]
        [Min(100f)] public float massKg = 1400f;
        [Min(0.1f)] public float maxSteerAngle = 30f;
        [Min(0f)] public float brakeTorque = 3000f;
        [Range(0f, 1f)] public float rearTorqueShare = 0.5f;
        [Range(0f, 2f)] public float antiRollStiffness = 0.8f;

        [Header("Fuel and durability")]
        [Min(1f)] public float tankCapacityLitres = 60f;
        [Min(0f)] public float litresPerHourAtFullLoad = 32f;

        public float GetGearRatio(int gearIndex)
        {
            if (gearRatios == null || gearRatios.Length == 0) return 1f;
            return gearRatios[Mathf.Clamp(gearIndex, 0, gearRatios.Length - 1)];
        }
    }
}
