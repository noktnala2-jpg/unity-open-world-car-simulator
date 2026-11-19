using UnityEngine;

namespace OpenRoadSim
{
    /// <summary>Quick-start scene generator: creates a test pad, road, vehicle, camera and basic lighting.</summary>
    public sealed class PrototypeBootstrap : MonoBehaviour
    {
        [SerializeField] private VehicleConfig vehicleConfig;
        [SerializeField] private bool createOnStart = true;
        [SerializeField] private float testPadSize = 300f;

        private void Start()
        {
            if (createOnStart) BuildPrototype();
        }

        [ContextMenu("Build Prototype Scene")]
        public void BuildPrototype()
        {
            if (GameObject.Find("Prototype Vehicle") != null) return;
            CreateGroundAndRoads();
            GameObject car = CreateVehicle();
            CreateCamera(car.transform);
            CreateLighting();
            CreateEnvironmentMarkers();
            VehicleHud hud = car.AddComponent<VehicleHud>();
        }

        private void CreateGroundAndRoads()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Test Pad (300m x 300m)";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = Vector3.one * (testPadSize / 10f);
            SetColor(ground, new Color(0.22f, 0.31f, 0.19f));
            CreateRoad("Main Test Road", new Vector3(0f, 0.025f, 0f), new Vector3(12f, 0.05f, 220f), new Color(0.12f, 0.12f, 0.13f));
            CreateRoad("Cross Test Road", new Vector3(0f, 0.03f, 0f), new Vector3(220f, 0.05f, 12f), new Color(0.12f, 0.12f, 0.13f));
        }

        private static void CreateRoad(string name, Vector3 position, Vector3 scale, Color color)
        {
            GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
            road.name = name;
            road.transform.position = position;
            road.transform.localScale = scale;
            SetColor(road, color);
        }

        private GameObject CreateVehicle()
        {
            GameObject root = new GameObject("Prototype Vehicle");
            root.transform.position = new Vector3(0f, 0.85f, -8f);
            Rigidbody rb = root.AddComponent<Rigidbody>();
            rb.mass = vehicleConfig != null ? vehicleConfig.massKg : 1400f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            BoxCollider chassis = root.AddComponent<BoxCollider>();
            chassis.size = new Vector3(1.75f, 0.65f, 3.7f);
            chassis.center = new Vector3(0f, 0.05f, 0f);
            GameObject bodyMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bodyMesh.name = "Body Visual";
            bodyMesh.transform.SetParent(root.transform, false);
            bodyMesh.transform.localPosition = new Vector3(0f, 0.17f, 0f);
            bodyMesh.transform.localScale = new Vector3(1.72f, 0.58f, 3.65f);
            Destroy(bodyMesh.GetComponent<Collider>());
            SetColor(bodyMesh, new Color(0.08f, 0.32f, 0.85f));
            GameObject cabin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabin.name = "Cabin Visual";
            cabin.transform.SetParent(root.transform, false);
            cabin.transform.localPosition = new Vector3(0f, 0.57f, -0.1f);
            cabin.transform.localScale = new Vector3(1.35f, 0.55f, 1.8f);
            Destroy(cabin.GetComponent<Collider>());
            SetColor(cabin, new Color(0.12f, 0.17f, 0.2f));

            WheelCollider fl = CreateWheelCollider(root.transform, "Front Left Wheel Collider", new Vector3(-0.88f, -0.18f, 1.25f));
            WheelCollider fr = CreateWheelCollider(root.transform, "Front Right Wheel Collider", new Vector3(0.88f, -0.18f, 1.25f));
            WheelCollider rl = CreateWheelCollider(root.transform, "Rear Left Wheel Collider", new Vector3(-0.88f, -0.18f, -1.25f));
            WheelCollider rr = CreateWheelCollider(root.transform, "Rear Right Wheel Collider", new Vector3(0.88f, -0.18f, -1.25f));
            Transform flv = CreateWheelVisual(root.transform, "Front Left Wheel", new Vector3(-0.88f, 0.35f, 1.25f));
            Transform frv = CreateWheelVisual(root.transform, "Front Right Wheel", new Vector3(0.88f, 0.35f, 1.25f));
            Transform rlv = CreateWheelVisual(root.transform, "Rear Left Wheel", new Vector3(-0.88f, 0.35f, -1.25f));
            Transform rrv = CreateWheelVisual(root.transform, "Rear Right Wheel", new Vector3(0.88f, 0.35f, -1.25f));
            VehicleController controller = root.AddComponent<VehicleController>();
            controller.Config = vehicleConfig;
            controller.SetWheels(fl, fr, rl, rr, flv, frv, rlv, rrv);
            return root;
        }

        private static WheelCollider CreateWheelCollider(Transform parent, string name, Vector3 localPosition)
        {
            GameObject wheelObject = new GameObject(name);
            wheelObject.transform.SetParent(parent, false);
            wheelObject.transform.localPosition = localPosition;
            WheelCollider wheel = wheelObject.AddComponent<WheelCollider>();
            wheel.radius = 0.36f;
            wheel.mass = 20f;
            wheel.suspensionDistance = 0.22f;
            JointSpring spring = wheel.suspensionSpring;
            spring.spring = 35000f;
            spring.damper = 4500f;
            spring.targetPosition = 0.5f;
            wheel.suspensionSpring = spring;
            WheelFrictionCurve forward = wheel.forwardFriction;
            forward.stiffness = 1.35f;
            wheel.forwardFriction = forward;
            WheelFrictionCurve sideways = wheel.sidewaysFriction;
            sideways.stiffness = 1.45f;
            wheel.sidewaysFriction = sideways;
            return wheel;
        }

        private static Transform CreateWheelVisual(Transform parent, string name, Vector3 localPosition)
        {
            GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.name = name;
            wheel.transform.SetParent(parent, false);
            wheel.transform.localPosition = localPosition;
            wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            wheel.transform.localScale = new Vector3(0.26f, 0.13f, 0.36f);
            Destroy(wheel.GetComponent<Collider>());
            SetColor(wheel, new Color(0.035f, 0.035f, 0.04f));
            return wheel.transform;
        }

        private void CreateCamera(Transform target)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                camera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
                cameraObject.AddComponent<AudioListener>();
            }
            camera.transform.position = target.position + new Vector3(0f, 3.2f, -7.5f);
            VehicleFollowCamera follow = camera.GetComponent<VehicleFollowCamera>();
            if (follow == null) follow = camera.gameObject.AddComponent<VehicleFollowCamera>();
            follow.SetTarget(target);
        }

        private static void CreateLighting()
        {
            if (FindObjectOfType<Light>() != null) return;
            GameObject lightObject = new GameObject("Prototype Sun");
            Light sun = lightObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.15f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.58f, 0.62f, 0.68f);
        }

        private static void CreateEnvironmentMarkers()
        {
            for (int i = 0; i < 10; i++)
            {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                marker.name = "Driving Practice Marker " + (i + 1);
                marker.transform.position = new Vector3((i % 2 == 0 ? 9f : -9f), 0.6f, -75f + i * 16f);
                marker.transform.localScale = new Vector3(1f, 1.2f, 1f);
                SetColor(marker, new Color(0.95f, 0.62f, 0.08f));
            }
        }

        private static void SetColor(GameObject target, Color color)
        {
            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer == null) return;
            Material material = new Material(Shader.Find("Standard"));
            material.color = color;
            renderer.material = material;
        }
    }
}
