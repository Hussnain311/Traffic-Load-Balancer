using UnityEngine;

public class DayNightManager : MonoBehaviour
{
    [Header("Time Settings")]
    public float dayDuration = 300f;    // 5 minutes in seconds
    public float nightDuration = 60f;   // 1 minute in seconds
    private float fullCycleDuration;    // Total day+night cycle

    [Header("Light Settings")]
    public Light sunLight;
    [Range(0, 1)] public float daylightThreshold = 0.25f; // When night ends (25% of cycle)

    [Header("Night Lights")]
    public GameObject[] nightLights;

    private float cycleProgress = 0f;   // 0-1 value tracking cycle progress

    private void Start()
    {
        fullCycleDuration = dayDuration + nightDuration;
        cycleProgress = 0.1f; // Start at morning
        SetStreetLights(false);
    }

    void Update()
    {
        // Advance time
        cycleProgress += Time.deltaTime / fullCycleDuration;
        if (cycleProgress > 1f) cycleProgress = 0f;

        // Calculate sun position (day only)
        float dayProgress = Mathf.Clamp01(cycleProgress * (fullCycleDuration / dayDuration));
        float sunAngle = dayProgress * 180f; // 0°-180° for day

        // Apply rotation (keeping original y/z values)
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        // Night detection (first and last parts of cycle)
        bool isNight = cycleProgress > (1f - nightDuration / fullCycleDuration);
        SetStreetLights(isNight);
    }

    void SetStreetLights(bool status)
    {
        foreach (GameObject light in nightLights)
        {
            if (light != null && light.activeSelf != status)
                light.SetActive(status);
        }
    }

    // Helper property to check current phase
    public bool IsNightTime => cycleProgress > (1f - nightDuration / fullCycleDuration);
}