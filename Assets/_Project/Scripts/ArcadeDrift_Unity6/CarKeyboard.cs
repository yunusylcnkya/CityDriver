using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ArcadeDrift
{
    [RequireComponent(typeof(CarController))]
    public sealed class CarKeyboard : MonoBehaviour
    {
        CarController car;
        void Awake() => car = GetComponent<CarController>();

        void Update()
        {
#if ENABLE_INPUT_SYSTEM
            var keys = Keyboard.current;
            if (keys == null) { ClearInput(); return; }
            car.SetInput((keys.dKey.isPressed ? 1 : 0) - (keys.aKey.isPressed ? 1 : 0),
                keys.wKey.isPressed ? 1 : 0, keys.sKey.isPressed ? 1 : 0, keys.spaceKey.isPressed);
#elif ENABLE_LEGACY_INPUT_MANAGER
            car.SetInput((Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0),
                Input.GetKey(KeyCode.W) ? 1 : 0, Input.GetKey(KeyCode.S) ? 1 : 0, Input.GetKey(KeyCode.Space));
#endif
        }

        void OnDisable() => ClearInput();
        void ClearInput()
        {
            if (car) car.SetInput(0, 0, 0, false);
        }
    }
}
