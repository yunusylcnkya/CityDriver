using UnityEngine;

namespace ArcadeDrift
{
    [RequireComponent(typeof(Rigidbody), typeof(CarController), typeof(CarBody))]
    public sealed class CarSwitcher : MonoBehaviour
    {
        public CarSettings[] profiles;
        [Min(0)] public int initialIndex;
        public AudioSource engine;
        public CarSettings Current { get; private set; }
        CarController controller;

        void Start() => Select(initialIndex);

        // UI Button.OnClick(int) üzerinden de çağrılabilir.
        public void Select(int index)
        {
            if (profiles == null || index < 0 || index >= profiles.Length || !profiles[index])
            {
                Debug.LogWarning("Araç profili atanmamış.", this);
                return;
            }
            Current = profiles[index];
            var rb = GetComponent<Rigidbody>();
            rb.mass = Current.mass;
            rb.ResetCenterOfMass();
            rb.centerOfMass += Current.centerOfMassOffset;
            controller = GetComponent<CarController>();
            controller.Configure(Current);
            GetComponent<CarBody>().Apply(Current.view);
            if (!engine) return;
            engine.Stop();
            engine.clip = Current.engineLoop;
            engine.loop = true;
            engine.pitch = 0.75f;
            if (engine.clip) engine.Play();
        }

        void Update()
        {
            if (engine && Current && controller)
                engine.pitch = Mathf.Lerp(0.7f, 1.95f, controller.Rpm / Current.redline);
        }
    }
}
