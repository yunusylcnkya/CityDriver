using UnityEngine;

namespace ArcadeDrift
{
    [CreateAssetMenu(menuName = "Arcade Drift/Car Settings")]
    public sealed class CarSettings : ScriptableObject
    {
        [Header("Garage")]
        public string displayName = "Sport";
        [Min(0)] public int price;
        public Sprite icon;
        public AudioClip engineLoop;
        public CarView view;

        [Header("Chassis")]
        [Min(1)] public float mass = 1150f;
        public Vector3 centerOfMassOffset = new Vector3(0, -0.2f, 0);

        [Header("Powertrain")]
        [Min(0)] public float motorTorque = 310f; // Nm
        public float[] gearRatios = { 3.2f, 2.1f, 1.55f, 1.2f, 0.95f };
        [Min(0.1f)] public float finalDrive = 3.7f;
        [Min(0.05f)] public float wheelRadius = 0.33f;
        [Min(2000)] public float redline = 6800;
        // X = devir / kesici devri, Y = tepe torkunun kullanılabilir oranı.
        public AnimationCurve torqueCurve = new AnimationCurve(
            new Keyframe(0, 0.6f), new Keyframe(0.55f, 1), new Keyframe(1, 0.65f));
        [Min(0)] public float brakeAcceleration = 11f;

        [Header("Lateral damping (1/s)")]
        [Min(0)] public float grip = 9;
        [Min(0)] public float driftMin = 1.2f;
        [Min(0)] public float driftMax = 4;
        [Range(2, 65)] public float enterAngle = 15;
        [Range(0, 60)] public float exitAngle = 7;

        [Header("Steering")]
        [Min(1)] public float yawSpeed = 95; // derece/sn
        [Min(1)] public float yawAcceleration = 350; // derece/sn²
        [Range(0.1f, 1)] public float fastSteerFraction = 0.52f;
        [Min(0)] public float recoveryGain = 2;

        void OnValidate()
        {
            driftMax = Mathf.Max(driftMax, driftMin);
            exitAngle = Mathf.Min(exitAngle, enterAngle - 1);
            if (gearRatios == null || gearRatios.Length == 0)
                gearRatios = new[] { 1f };
            for (int i = 0; i < gearRatios.Length; i++)
                gearRatios[i] = Mathf.Max(0.1f, gearRatios[i]);
        }
    }
}
