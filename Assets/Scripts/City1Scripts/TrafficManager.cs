using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TrafficManager : MonoBehaviour
{
    [Header("Manual Control (Stop Tag)")]
    public Button[] controlButtons; // Buttons for manual control
    [SerializeField] private GameObject[] stopObjects; // Objects with "Stop" tag

    [Header("Automatic Control (Signal Tag)")]
    [SerializeField] private GameObject[] signalObjects; // Objects with "Signal" tag
    [SerializeField] private float signalSwitchTime = 10f; // Time per signal (10s)

    private Coroutine activeStopCoroutine; // For manual stops
    private Coroutine activeSignalCycle; // For automatic signals

    private void Start()
    {
        // Initialize manual control
        if (controlButtons.Length != stopObjects.Length)
            Debug.LogError("Button and Stop counts don't match!");

        // Start automatic signal cycling
        if (signalObjects.Length > 0)
            activeSignalCycle = StartCoroutine(CycleSignals());
    }

    // === MANUAL CONTROL (Stop Tag) ===
    public void OnButtonPressed(int index)
    {
        if (index < 0 || index >= stopObjects.Length)
        {
            Debug.LogError($"Invalid index: {index}");
            return;
        }

        // Reset manual control
        if (activeStopCoroutine != null)
            StopCoroutine(activeStopCoroutine);

        ResetAllStops();
        stopObjects[index].SetActive(false);
        activeStopCoroutine = StartCoroutine(ReenableStop(index));
    }

    private IEnumerator ReenableStop(int index)
    {
        yield return new WaitForSeconds(5f);
        stopObjects[index].SetActive(true);
        activeStopCoroutine = null;
    }

    private void ResetAllStops()
    {
        foreach (var stop in stopObjects)
            stop.SetActive(true);
    }

    // === AUTOMATIC CONTROL (Signal Tag) ===
    private IEnumerator CycleSignals()
    {
        int currentSignalIndex = 0;
        while (true)
        {
            // Disable current signal (allow traffic)
            signalObjects[currentSignalIndex].SetActive(false);
            Debug.Log($"🚦 Signal {currentSignalIndex} disabled (auto)");

            // Wait for signalSwitchTime (10s)
            yield return new WaitForSeconds(signalSwitchTime);

            // Re-enable current signal (block traffic)
            signalObjects[currentSignalIndex].SetActive(true);
            Debug.Log($"🚦 Signal {currentSignalIndex} re-enabled");

            // Move to next signal (loop back to 0 after 3)
            currentSignalIndex = (currentSignalIndex + 1) % signalObjects.Length;
        }
    }

    // Cleanup
    private void OnDestroy()
    {
        if (activeStopCoroutine != null) StopCoroutine(activeStopCoroutine);
        if (activeSignalCycle != null) StopCoroutine(activeSignalCycle);
    }

    // Button shortcuts (for Unity Events)
    public void OnButton0Pressed() => OnButtonPressed(0);
    public void OnButton1Pressed() => OnButtonPressed(1);
    public void OnButton2Pressed() => OnButtonPressed(2);
    public void OnButton3Pressed() => OnButtonPressed(3);
}