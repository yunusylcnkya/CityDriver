using UnityEngine;

[RequireComponent(typeof(Light))]
public class DayNightCycle : MonoBehaviour
{
    [Tooltip("Bir tam oyun gününün kaç gerçek dakika süreceği.")]
    [SerializeField, Min(0.1f)] private float dayLengthMinutes = 30f;

    [Tooltip("Oyun başladığında şehirdeki saat.")]
    [SerializeField, Range(0f, 23.99f)] private float startHour = 8f;

    private Light sun;
    private float currentHour;
    private float sunHeading;
    private float daytimeIntensity;

    private void Awake()
    {
        sun = GetComponent<Light>();
        sunHeading = transform.eulerAngles.y;
        daytimeIntensity = sun.intensity;
        currentHour = startHour;
        UpdateSun();
    }

    private void Update()
    {
        float dayLengthSeconds = Mathf.Max(0.1f, dayLengthMinutes) * 60f;
        currentHour = Mathf.Repeat(
            currentHour + Time.deltaTime * 24f / dayLengthSeconds, 24f);

        UpdateSun();
    }

    private void UpdateSun()
    {
        // 06.00'da gün doğar, 12.00'de güneş tepededir, 18.00'de batar.
        float sunAngle = (currentHour - 6f) * 15f;
        transform.rotation = Quaternion.Euler(sunAngle, sunHeading, 0f);

        float sunHeight = Mathf.Sin(sunAngle * Mathf.Deg2Rad);
        sun.intensity = daytimeIntensity * Mathf.SmoothStep(0f, 1f, sunHeight);
    }
}