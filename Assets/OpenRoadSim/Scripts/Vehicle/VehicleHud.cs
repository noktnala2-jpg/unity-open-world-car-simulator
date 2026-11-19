using UnityEngine;

namespace OpenRoadSim
{
    /// <summary>Prototype telemetry overlay; intentionally kept independent of any UI package.</summary>
    [RequireComponent(typeof(VehicleController))]
    public sealed class VehicleHud : MonoBehaviour
    {
        private VehicleController vehicle;
        private GUIStyle big, small;

        private void Awake() { vehicle = GetComponent<VehicleController>(); }

        private void OnGUI()
        {
            if (big == null)
            {
                big = new GUIStyle(GUI.skin.label) { fontSize = 30, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
                small = new GUIStyle(GUI.skin.label) { fontSize = 16, normal = { textColor = Color.white } };
            }
            float speed = vehicle.SpeedKph;
            GUI.Label(new Rect(22, Screen.height - 105, 250, 45), Mathf.RoundToInt(speed) + " km/h", big);
            string gear = vehicle.CurrentGear < 0 ? "R" : vehicle.CurrentGear == 0 ? "N" : vehicle.CurrentGear.ToString();
            GUI.Label(new Rect(25, Screen.height - 61, 300, 30), "Gear " + gear + "   RPM " + Mathf.RoundToInt(vehicle.EngineRpm), small);
            GUI.Label(new Rect(25, Screen.height - 38, 300, 26), "Fuel " + Mathf.RoundToInt(vehicle.FuelFraction * 100f) + "%   Condition " + Mathf.RoundToInt(vehicle.Condition * 100f) + "%", small);
            GUI.Label(new Rect(20, 20, 700, 28), "WASD / Arrow keys: drive   Space: handbrake   Esc: release cursor", small);
            if (Cursor.lockState != CursorLockMode.Locked && Event.current.type == EventType.MouseDown)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
