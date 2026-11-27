using System;
using UnityEngine;

namespace ArcadeDrift
{
    [CreateAssetMenu(menuName = "Arcade Drift/Car View")]
    public sealed class CarView : ScriptableObject
    {
        [Serializable]
        public struct Wheel
        {
            public GameObject prefab;
            public Vector3 position;
            public Vector3 eulerAngles;
        }

        public GameObject chassisPrefab;
        public Vector3 chassisOffset;
        public Vector3 chassisEulerAngles;
        // Konumlar aracın görsel köküne göre metredir.
        public Wheel[] wheels = new Wheel[4];
    }
}
