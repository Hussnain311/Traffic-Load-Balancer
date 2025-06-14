using UnityEngine;
using System.Collections;

public class VehicleManager : MonoBehaviour
{
    [Header("Vehicle Settings")]
    [SerializeField] private GameObject[] vehiclePrefabs;
    [SerializeField] private float minSpawnInterval = 3f;
    [SerializeField] private float maxSpawnInterval = 7f;
    [SerializeField] private float spawnRateIncreaseStep = 0.5f; // How much faster spawning gets per unlock
    [SerializeField] private float timeBetweenUnlocks = 20f; // Time before next vehicle is unlocked
   

    [Header("Starting Waypoints")]
    [SerializeField] private Transform[] startingPoints;

    private int currentMaxVehicleIndex = 0; // Only spawn up to this index
    private float currentMinSpawnInterval;
    private float currentMaxSpawnInterval;
    private Coroutine unlockRoutine;

    private void Start()
    {
       

        currentMinSpawnInterval = minSpawnInterval;
        currentMaxSpawnInterval = maxSpawnInterval;

        StartCoroutine(SpawnVehicles());
        unlockRoutine = StartCoroutine(UnlockNewVehiclesOverTime());
    }


    private IEnumerator SpawnVehicles()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(currentMinSpawnInterval, currentMaxSpawnInterval));
            SpawnVehicle();
        }
    }

    private IEnumerator UnlockNewVehiclesOverTime()
    {
        while (currentMaxVehicleIndex < vehiclePrefabs.Length - 1)
        {
            yield return new WaitForSeconds(timeBetweenUnlocks);

            currentMaxVehicleIndex++; // Unlock next vehicle
            Debug.Log($"Unlocked vehicle: {vehiclePrefabs[currentMaxVehicleIndex].name}");

            // Increase spawn rate (reduce intervals)
            currentMinSpawnInterval = Mathf.Max(0.5f, currentMinSpawnInterval - spawnRateIncreaseStep);
            currentMaxSpawnInterval = Mathf.Max(1f, currentMaxSpawnInterval - spawnRateIncreaseStep);
            Debug.Log($"New spawn rate: {currentMinSpawnInterval}-{currentMaxSpawnInterval}s");
        }
    }

    private void SpawnVehicle()
    {
        Transform spawnPoint = startingPoints[Random.Range(0, startingPoints.Length)];
        int vehicleIndex = Random.Range(0, currentMaxVehicleIndex + 1); // Only pick from unlocked vehicles

        GameObject vehicle = Instantiate(
            vehiclePrefabs[vehicleIndex],
            spawnPoint.position,
            spawnPoint.rotation
        );

        Transform frontSensor = vehicle.transform.Find("FrontSensor");
        if (frontSensor == null)
        {
            Debug.LogError("Missing FrontSensor!", vehicle);
            Destroy(vehicle);
            return;
        }

        VehicleMover mover = vehicle.GetComponent<VehicleMover>();
        if (mover == null) mover = vehicle.AddComponent<VehicleMover>();

        mover.Initialize(spawnPoint, frontSensor);
        Debug.Log($"Spawned {vehicle.name} at {spawnPoint.name}", vehicle);
    }

    private void OnDestroy()
    {
        if (unlockRoutine != null)
            StopCoroutine(unlockRoutine);
    }
}