using UnityEngine;

namespace ArcadeDrift
{
    public sealed class CarBody : MonoBehaviour
    {
        Transform current;

        public void Apply(CarView view)
        {
            if (current)
            {
                current.gameObject.SetActive(false);
                Destroy(current.gameObject);
            }
            if (!view) return;
            current = new GameObject("Car Graphics").transform;
            current.SetParent(transform, false);
            Mount(view.chassisPrefab, view.chassisOffset, view.chassisEulerAngles);
            if (view.wheels == null) return;
            foreach (var wheel in view.wheels)
                Mount(wheel.prefab, wheel.position, wheel.eulerAngles);
        }

        void Mount(GameObject prefab, Vector3 position, Vector3 angles)
        {
            if (!prefab) return;
            Transform part = Instantiate(prefab, current, false).transform;
            part.SetLocalPositionAndRotation(position, Quaternion.Euler(angles));
        }
    }
}
