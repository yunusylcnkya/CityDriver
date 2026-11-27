using UnityEngine;

namespace ArcadeDrift
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class CarController : MonoBehaviour
    {
        [SerializeField] LayerMask roadLayers;
        [Tooltip("Kök pivotundan aşağı doğru zemin arama mesafesi (m).")]
        [SerializeField, Min(0.05f)] float probeLength = 0.65f;
        Rigidbody rb;
        CarSettings settings;
        float steering, throttle, brake;
        bool handbrake;
        int gear;
        float shiftDelay;

        public float SlipAngle { get; private set; }
        public bool Drifting { get; private set; }
        public float Rpm { get; private set; }
        public int Gear => gear + 1;

        void Awake() => rb = GetComponent<Rigidbody>();

        public void Configure(CarSettings profile)
        {
            settings = profile;
            gear = 0;
            shiftDelay = 0;
            Rpm = 900;
            SlipAngle = 0;
            Drifting = false;
        }

        public void SetInput(float steer, float gas, float footBrake, bool slide)
        {
            steering = Mathf.Clamp(steer, -1, 1);
            throttle = Mathf.Clamp01(gas);
            brake = Mathf.Clamp01(footBrake);
            handbrake = slide;
        }

        void FixedUpdate()
        {
            if (!settings) return;
            Vector3 up = rb.rotation * Vector3.up;
            if (!Physics.Raycast(rb.position, -up, out var hit, probeLength,
                    roadLayers, QueryTriggerInteraction.Ignore))
            {
                SlipAngle = 0;
                Drifting = false;
                return;
            }

            Vector3 forward = Vector3.ProjectOnPlane(rb.rotation * Vector3.forward, hit.normal).normalized;
            Vector3 right = Vector3.Cross(hit.normal, forward);
            Vector3 velocity = rb.linearVelocity;
            float longitudinal = Vector3.Dot(velocity, forward);
            float lateral = Vector3.Dot(velocity, right);
            float speed = Mathf.Sqrt(longitudinal * longitudinal + lateral * lateral);
            // Geri yuvarlanırken düz hareket de sıfır kayma olarak okunur.
            float signedSlip = Mathf.Atan2(lateral, Mathf.Abs(longitudinal)) * Mathf.Rad2Deg;
            SlipAngle = speed < 0.5f ? 0 : Mathf.Clamp(Mathf.Abs(signedSlip), 0, 90);

            // Ayrı giriş/çıkış eşikleri, sınırda art arda mod değişmesini önler.
            float threshold = Drifting ? settings.exitAngle : settings.enterAngle;
            Drifting = speed >= 3 && (handbrake || SlipAngle > threshold);
            float angleRatio = Mathf.InverseLerp(settings.enterAngle, 90, SlipAngle);
            float damping = Drifting ? Mathf.Lerp(settings.driftMin, settings.driftMax, angleRatio) : settings.grip;
            float dt = Time.fixedDeltaTime;
            // Bir fizik adımında yanal hızı tersine çevirmeyen üstel sönüm.
            float lateralAcceleration = -lateral * (1 - Mathf.Exp(-damping * dt)) / dt;
            rb.AddForce(right * lateralAcceleration, ForceMode.Acceleration);

            ApplyDrive(forward, longitudinal, dt);
            ApplySteering(up, speed, longitudinal, signedSlip, dt);
        }

        void ApplyDrive(Vector3 forward, float longitudinal, float dt)
        {
            float wheelRpm = Mathf.Max(0, longitudinal) * 60 / (2 * Mathf.PI * settings.wheelRadius);
            var ratios = settings.gearRatios;
            float axleRpm = wheelRpm * settings.finalDrive;
            float rawRpm = axleRpm * ratios[gear];
            shiftDelay -= dt;
            if (shiftDelay <= 0)
            {
                int previous = gear;
                if (rawRpm > settings.redline * 0.9f && gear < ratios.Length - 1)
                    gear++;
                else if (gear > 0 && rawRpm < settings.redline * 0.42f &&
                         axleRpm * ratios[gear - 1] < settings.redline * 0.86f)
                    gear--;
                if (gear != previous) shiftDelay = 0.22f;
            }
            rawRpm = axleRpm * ratios[gear];
            Rpm = Mathf.Clamp(rawRpm, 900, settings.redline);
            float torque = settings.motorTorque * Mathf.Max(0, settings.torqueCurve.Evaluate(Rpm / settings.redline));
            float drive = torque * ratios[gear] * settings.finalDrive / settings.wheelRadius;
            if (rawRpm < settings.redline)
                rb.AddForce(forward * (drive * throttle * (1 - brake)), ForceMode.Force);

            float resistance = 0.22f + 0.001f * longitudinal * longitudinal;
            float deceleration = resistance + settings.brakeAcceleration * brake + (handbrake ? 2f : 0f);
            float stop = Mathf.Min(Mathf.Abs(longitudinal) / dt, deceleration);
            rb.AddForce(-forward * (Mathf.Sign(longitudinal) * stop), ForceMode.Acceleration);
        }

        void ApplySteering(Vector3 up, float speed, float longitudinal, float slip, float dt)
        {
            float fast = Mathf.InverseLerp(8, 34, speed);
            float limit = settings.yawSpeed * Mathf.Lerp(1, settings.fastSteerFraction, fast);
            float target = steering * limit * Mathf.Clamp01(speed / 4);
            if (longitudinal < -0.5f) target = -target;
            // Büyük açıda burnu hız yönüne çevir. Küçük drift açısına müdahale etme.
            if (longitudinal >= 0 && speed > 3)
                target += Mathf.Sign(slip) * Mathf.Max(0, Mathf.Abs(slip) - 25) * settings.recoveryGain;
            target = Mathf.Clamp(target, -limit, limit) * Mathf.Deg2Rad;
            float yaw = Vector3.Dot(rb.angularVelocity, up);
            float next = Mathf.MoveTowards(yaw, target, settings.yawAcceleration * Mathf.Deg2Rad * dt);
            rb.angularVelocity += up * (next - yaw);
        }
    }
}
