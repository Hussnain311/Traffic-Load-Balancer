using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class RoadUpgradeManager : MonoBehaviour
{
    [System.Serializable]
    public class RoadData
    {
        public string roadName;
        public GameObject[] levelObjects;
        public int[] purchaseCosts;
    }

    [Header("Roads")]
    public RoadData[] roads;

    [Header("Road Settings")]
    public float bumpScale = 1.2f;
    public float bumpDuration = 0.2f;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI detailsText;
    public GameObject confirmButton;
    public GameObject closeButton;

    [Header("Road Image Objects")]
    public GameObject RoadImage;
    public GameObject IntersectionImage;
    public GameObject RoadBlocks;

    [Header("References")]
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private UIAnimator uiAnimator;
    [SerializeField] private CameraControllerWithAutoMove cameraController;

    [Header("Intersection Level Activation")]
    [Tooltip("Objects that should activate when intersection reaches level 1")]
    [SerializeField] private List<GameObject> objectsToActivateOnIntersectionUpgrade;
    [Tooltip("Objects that should deactivate when intersection reaches max level")]
    [SerializeField] private List<GameObject> objectsToDeactivateOnMaxIntersectionLevel;

    private RoadData currentRoad;
    private int currentRoadIndex = -1;

    void Start()
    {
        if (moneyManager == null || uiAnimator == null)
        {
            Debug.LogError("[RoadUpgradeManager] MoneyManager or UIAnimator reference is missing!");
            enabled = false;
            return;
        }

        // Check if the object was disabled before
        if (PlayerPrefs.GetInt("Road1_Disabled", 0) == 1 && RoadBlocks != null)
        {
            if (RoadBlocks != null)
            {
                RoadBlocks.SetActive(false); // Keep it disabled
            }
            else
            {
                return;
            }
        }

        // Initialize all roads from saved data
        InitializeAllRoads();

        dialoguePanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                string tag = hit.collider.tag;

                for (int i = 0; i < roads.Length; i++)
                {
                    if (tag == roads[i].roadName)
                    {
                        HandleRoad(i, hit.collider.gameObject);
                        break;
                    }
                }
            }
        }
    }

    void HandleRoad(int roadIndex, GameObject roadObject)
    {
        currentRoadIndex = roadIndex;
        currentRoad = roads[roadIndex];
        LeanTween.scale(roadObject, Vector3.one * bumpScale, bumpDuration)
                 .setEase(LeanTweenType.easeOutBounce)
                 .setLoopPingPong(1);

        // Directly show upgrade dialog instead of buttons
        OnUpgradeButtonClicked();
    }

    public void OnUpgradeButtonClicked()
    {
        // DIsabling the camera movement for diaogueBox
        cameraController.DisableCamera();


        uiAnimator.crossingImage.SetActive(false);
        if (currentRoad == null) return;
        int level = PlayerPrefs.GetInt(currentRoad.roadName + "_Level", 1);
        int maxLevel = currentRoad.levelObjects.Length;

        dialoguePanel.SetActive(true);
        dialoguePanel.transform.localScale = Vector3.zero;
        LeanTween.scale(dialoguePanel, new Vector3(1.7f, 1.8f, 0f), 0.3f).setEase(LeanTweenType.easeOutBack);

        // Show current level image for maxed roads, next level for upgradable ones
        SetRoadImage(currentRoad.roadName, level >= maxLevel ? level : level + 1);

        if (level >= maxLevel)
        {
            // Show max level stats
            detailsText.text = $"{currentRoad.roadName} - MAX LEVEL\n" +
                             $"Type: Traffic Flow\n" +
                             $"Unit is Maxed Out!";
            confirmButton.SetActive(false);
            uiAnimator.crossingConfirmButton.SetActive(false);
        }
        else
        {
            int nextLevel = level + 1;
            detailsText.text = $"Upgrading To Level {nextLevel + 1}\n" +
                             $"Cost: ${currentRoad.purchaseCosts[level]}\n" +
                             $"Type: Traffic Flow";
            confirmButton.SetActive(true);
            uiAnimator.crossingConfirmButton.SetActive(false);
        }
    }

    public void OnConfirmButtonClicked()
    {
        if (currentRoad == null)
        {
            Debug.LogError("No road selected!");
            return;
        }

        // Get current level (default 0 if not purchased)
        int level = PlayerPrefs.GetInt(currentRoad.roadName + "_Level", 0);

        // Check if we can upgrade further
        if (level >= currentRoad.purchaseCosts.Length)
        {
            ToastMessageSystem.ShowToast("Max level reached!", ToastMessageSystem.ToastType.ERROR);
            return;
        }

        int cost = currentRoad.purchaseCosts[level];

        if (moneyManager.SpendCoins(cost))
        {
            level++;
            UpdateRoadState(currentRoad, level);

            // Special handling for Level 1 purchase
            if (level == 1)
            {
                // Disable the road block object if this is the correct road
                if (currentRoad.roadName == "Intersection") // road name is
                {
                    if (RoadBlocks != null)
                    {
                        RoadBlocks.SetActive(false);
                        PlayerPrefs.SetInt("Road1_Disabled", 1); // 1 = disabled
                    }
                    else
                    {
                        Debug.LogWarning("RoadBlocks reference is missing!");
                    }
                }

                ToastMessageSystem.ShowToast($"Purchased {currentRoad.roadName} for ${cost}!", ToastMessageSystem.ToastType.SUCCESS);
            }
            else // Upgrade
            {
                ToastMessageSystem.ShowToast($"Upgraded {currentRoad.roadName} to Level {level}!", ToastMessageSystem.ToastType.SUCCESS);
            }

            PlayerPrefs.Save();
            CloseDialoguePanel();
        }
        else
        {
            ToastMessageSystem.ShowToast("Not enough coins!", ToastMessageSystem.ToastType.WARNING);
            detailsText.text += "\nNot enough money!";
        }
    }

    void InitializeAllRoads()
    {
        foreach (RoadData road in roads)
        {
            int level = PlayerPrefs.GetInt(road.roadName + "_Level", 0);
            level = Mathf.Clamp(level, 0, road.levelObjects.Length);
            UpdateRoadState(road, level);

            // Special handling for intersection
            if (road.roadName == "Intersection")
            {
                ActivateIntersectionObjects(level);
            }
        }
    }

    void UpdateRoadState(RoadData road, int level)
    {
        // Existing road state update logic
        for (int i = 0; i < road.levelObjects.Length; i++)
        {
            if (road.levelObjects[i] != null)
            {
                road.levelObjects[i].SetActive(i < level);
            }
        }

        PlayerPrefs.SetInt(road.roadName + "_Level", level);

        // Special handling for intersection
        if (road.roadName == "Intersection")
        {
            ActivateIntersectionObjects(level);
        }
    }

    void ActivateIntersectionObjects(int intersectionLevel)
    {
        foreach (GameObject obj in objectsToActivateOnIntersectionUpgrade)
        {
            if (obj != null)
            {
                // Activate if intersection has reached at least level 1
                obj.SetActive(intersectionLevel >= 1);

                // Save activation state
                PlayerPrefs.SetInt("IntersectionObj_" + obj.name + "_Active",
                                 intersectionLevel >= 1 ? 1 : 0);
            }
        }

        // Deactivate different objects only when level reaches max (3)
        if (intersectionLevel == 2)
        {
            foreach (GameObject obj in objectsToDeactivateOnMaxIntersectionLevel)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }

    public void CloseDialoguePanel()
    {

        // Re-enable camera movement
        cameraController.EnableCamera();


        LeanTween.scale(dialoguePanel, Vector3.zero, 0.2f)
                 .setEase(LeanTweenType.easeInBack)
                 .setOnComplete(() =>
                 {
                     dialoguePanel.SetActive(false);
                     SetAllRoadImagesInactive();
                     currentRoad = null;
                     currentRoadIndex = -1;
                     Debug.Log("Road dialogue closed");
                 });
    }

    void SetRoadImage(string roadName, int level)
    {
        SetAllRoadImagesInactive();
        if (roadName.StartsWith("Road"))
        {
            RoadImage.SetActive(true);
        }
        else if (roadName.StartsWith("Intersection"))
        {
            IntersectionImage.SetActive(true);
        }
        else
        {
            Debug.LogError("Image nOt Assigned");
            return;
        }
    }

    void SetAllRoadImagesInactive()
    {
        RoadImage.SetActive(false);
        IntersectionImage.SetActive(false);
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) // Game is being paused (or put to background)
        {
            PlayerPrefs.Save();
        }
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}