using UnityEngine;

public class VehicleMover : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float minSpeed = 3f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private LayerMask vehicleLayer = default;

    [Header("Sensor Settings")]
    [SerializeField] private float detectionDistance = 0.5f; // Reduced by half
    [SerializeField] private GameObject tollPopupPrefab;

    [Header("Collision Settings")]
    [SerializeField] private float minFollowDistance = 0.5f;

    private Transform currentWaypoint;
    private float speed;
    private float originalSpeed;
    private Transform frontSensor;
    private MoneyManager moneyManager;
    private VehicleMover frontVehicle;

    private void Start()
    {
        moneyManager = Object.FindFirstObjectByType<MoneyManager>();
        if (moneyManager == null)
            Debug.LogError("MoneyManager not found!");

        if (vehicleLayer == 0)
            vehicleLayer = LayerMask.GetMask("Vehicle");
    }

    public void Initialize(Transform startingWaypoint, Transform sensor)
    {
        frontSensor = sensor;
        currentWaypoint = startingWaypoint;
        originalSpeed = Random.Range(minSpeed, maxSpeed);
        speed = originalSpeed;
    }

    private void Update()
    {
        if (currentWaypoint == null)
        {
            Destroy(gameObject);
            return;
        }

        CheckForSignalStop();
        DetectVehicleAhead();
        MoveTowardsWaypoint();
    }

    private void CheckForSignalStop()
    {
        if (frontSensor == null) return;

        if (Physics.Raycast(frontSensor.position, transform.forward, out RaycastHit hit, detectionDistance))
        {
            if (hit.collider.CompareTag("Stop"))
            {
                speed = 0f;
                return;
            }
        }
    }

    private void DetectVehicleAhead()
    {
        if (frontSensor == null) return;

        if (Physics.Raycast(frontSensor.position, transform.forward, out var hit, detectionDistance))
        {
            // Vehicle detection
            if (((1 << hit.collider.gameObject.layer) & vehicleLayer) != 0)
            {
                frontVehicle = hit.collider.GetComponentInParent<VehicleMover>();

                if (frontVehicle != null)
                {
                    // Immediately match front vehicle's speed
                    speed = frontVehicle.speed;

                    // If too close and front vehicle is stopped, stop completely
                    if (hit.distance <= minFollowDistance && frontVehicle.speed < 0.1f)
                    {
                        speed = 0f;
                    }
                }
            }
            // Stop object detection
            else if (hit.collider.CompareTag("Stop") || hit.collider.CompareTag("Signal"))
            {
                speed = 0f;
            }
            else
            {
                // No vehicle detected - return to normal speed
                speed = Mathf.Lerp(speed, originalSpeed, Time.deltaTime * 2f);
                frontVehicle = null;
            }
        }
        else
        {
            // No obstacles - return to normal speed
            speed = Mathf.Lerp(speed, originalSpeed, Time.deltaTime * 2f);
            frontVehicle = null;
        }
    }

    private void MoveTowardsWaypoint()
    {
        transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.position, speed * Time.deltaTime);

        Vector3 direction = (currentWaypoint.position - transform.position).normalized;
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, currentWaypoint.position) < 0.1f)
        {
            AdvanceToNextWaypoint();
        }
    }

    private void AdvanceToNextWaypoint()
    {
        WaypointNode waypointNode = currentWaypoint.GetComponent<WaypointNode>();
        if (waypointNode == null || waypointNode.nextWaypoints.Length == 0)
        {
            Destroy(gameObject);
            return;
        }

        var activeWaypoints = System.Array.FindAll(waypointNode.nextWaypoints,
            waypoint => waypoint != null && waypoint.gameObject.activeInHierarchy);

        if (activeWaypoints.Length == 0)
        {
            Destroy(gameObject);
            return;
        }

        int randomIndex = Random.Range(0, activeWaypoints.Length);
        currentWaypoint = activeWaypoints[randomIndex];
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ToolTax"))
        {
            moneyManager?.AddCoins(10);
            TollPopup.Instance?.ShowToll("+10$");
        }
    }

    private void OnDrawGizmos()
    {
        if (frontSensor == null) return;

        Gizmos.color = (frontVehicle != null) ? Color.red : Color.green;
        Gizmos.DrawRay(frontSensor.position, transform.forward * detectionDistance);
    }
}