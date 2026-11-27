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
    private Material originalSkybox;
    private Material runtimeSkybox;
    private float daytimeExposure;

    [SerializeField] private Color dayAmbient = new Color(0.65f, 0.70f, 0.78f);
    [SerializeField] private Color nightAmbient = new Color(0.07f, 0.09f, 0.15f);
    private void Awake()
    {
        sun = GetComponent<Light>();
        sunHeading = transform.eulerAngles.y;
        daytimeIntensity = sun.intensity;
        currentHour = startHour;
        RenderSettings.sun = sun;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

        originalSkybox = RenderSettings.skybox;
        if (originalSkybox != null && originalSkybox.HasProperty("_Exposure"))
        {
            runtimeSkybox = new Material(originalSkybox);
            daytimeExposure = runtimeSkybox.GetFloat("_Exposure");
            RenderSettings.skybox = runtimeSkybox;
        }
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

        float daylight = Mathf.InverseLerp(-0.15f, 0.45f, sunHeight);
        daylight = Mathf.SmoothStep(0f, 1f, daylight);

        RenderSettings.ambientLight = Color.Lerp(nightAmbient, dayAmbient, daylight);

        if (runtimeSkybox != null)
        {
            float exposure = Mathf.Lerp(daytimeExposure * 0.08f, daytimeExposure, daylight);
            runtimeSkybox.SetFloat("_Exposure", exposure);
        }
    }

    private void OnDestroy()
    {
        if (runtimeSkybox != null)
        {
            if (RenderSettings.skybox == runtimeSkybox)
                RenderSettings.skybox = originalSkybox;

            Destroy(runtimeSkybox);
        }
    }
}