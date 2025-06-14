using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class BuildingUpgradeManager : MonoBehaviour
{
    [System.Serializable]
    public class BuildingData
    {
        public string buildingName;
        public GameObject[] levelObjects;
        public GameObject lockObject;
        public int[] purchaseCosts;
        public int[] revenuePerInterval;
        public float revenueInterval = 5f;
    }

    [Header("Buildings")]
    public BuildingData[] buildings;

    [Header("Building Settings")]
    public float bumpScale = 1.2f;
    public float bumpDuration = 0.2f;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI detailsText;
    public GameObject confirmButton;
    public GameObject closeButton;

    [Header("Building Image Objects")]
    public GameObject HotelImage;
    public GameObject StadiumImage;
    public GameObject FactoryImage;
    public GameObject SocietyImage;
    public GameObject MarketImage;
    public GameObject GasStationImage;
    public GameObject PoliceImage;
    public GameObject SchoolImage;
    public GameObject WhiteHouseImage;
    public GameObject City2RoadsImage;

    [Header("References")]
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private UIAnimator uiAnimator;
    [SerializeField] private CameraControllerWithAutoMove cameraController;

    private BuildingData currentBuilding;
    private int currentBuildingIndex = -1;

    void Start()
    {
        if (moneyManager == null || uiAnimator == null)
        {
            Debug.LogError("[BuildingUpgradeManager] MoneyManager or UIAnimator reference is missing!");
            enabled = false;
            return;
        }

        // Initialize all buildings from saved data
        InitializeAllBuildings();

        dialoguePanel.SetActive(false);
    }

    void Update()
    {

        if (cameraController != null && cameraController.IsCameraInteracting)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                string tag = hit.collider.tag;
                if (tag.StartsWith("Locked"))
                {
                    string buildingName = tag.Substring(6);
                    HandleLockedBuilding(buildingName, hit.collider.gameObject);
                }
                else
                {
                    for (int i = 0; i < buildings.Length; i++)
                    {
                        if (tag == buildings[i].buildingName)
                        {
                            HandleExistingBuilding(i, hit.collider.gameObject);
                            break;
                        }
                    }
                }
            }
        }
    }

    void HandleLockedBuilding(string buildingName, GameObject buildingObject)
    {
        for (int i = 0; i < buildings.Length; i++)
        {
            if (buildings[i].buildingName == buildingName)
            {
                currentBuildingIndex = i;
                currentBuilding = buildings[i];
                LeanTween.scale(buildingObject, Vector3.one * bumpScale, bumpDuration)
                         .setEase(LeanTweenType.easeOutBounce)
                         .setLoopPingPong(1);
                ShowPurchaseDialogue(currentBuilding);
                break;
            }
        }
    }

    void HandleExistingBuilding(int buildingIndex, GameObject buildingObject)
    {
        // Add minimum movement threshold
        if (Input.GetTouch(0).deltaPosition.magnitude > 15f)
            return;

        currentBuildingIndex = buildingIndex;
        currentBuilding = buildings[buildingIndex];
        LeanTween.scale(buildingObject, Vector3.one * bumpScale, bumpDuration)
                 .setEase(LeanTweenType.easeOutBounce)
                 .setLoopPingPong(1);

        // Directly show upgrade dialog instead of buttons
        OnUpgradeButtonClicked();
    }

    void ShowPurchaseDialogue(BuildingData building)
    {
        uiAnimator.crossingConfirmButton.SetActive(false);
        dialoguePanel.SetActive(true);
        dialoguePanel.transform.localScale = Vector3.zero;
        LeanTween.scale(dialoguePanel, new Vector3(1f, 1f, 1f), 0.3f).setEase(LeanTweenType.easeOutBack);

        SetBuildingImage(building.buildingName, 1);
        detailsText.text = $"Purchase {building.buildingName}\n" +
                          $"Cost: ${building.purchaseCosts[0]}\n" +
                          $"Revenue: ${building.revenuePerInterval[0]}/{building.revenueInterval}sec\n" +
                          $"Type: Earning";

        confirmButton.SetActive(true);
    }

    public void OnInfoButtonClicked()
    {
        uiAnimator.crossingImage.SetActive(false);
        if (currentBuilding == null) return;
        int level = PlayerPrefs.GetInt(currentBuilding.buildingName + "_Level", 1);
        int maxLevel = currentBuilding.levelObjects.Length;

        dialoguePanel.SetActive(true);
        dialoguePanel.transform.localScale = Vector3.zero;
        LeanTween.scale(dialoguePanel, new Vector3(1f, 1f, 1f), 0.3f).setEase(LeanTweenType.easeOutBack);

        SetBuildingImage(currentBuilding.buildingName, level);

        string infoText = $"{currentBuilding.buildingName} - Level {level}\n" +
                          $"Revenue: ${currentBuilding.revenuePerInterval[level - 1]}/{currentBuilding.revenueInterval}sec\n" +
                          $"Type: Earning";
        if (level >= maxLevel)
        {
            infoText += "\nBuilding is Maxed";
        }
        detailsText.text = infoText;

        confirmButton.SetActive(false);
    }

    public void OnUpgradeButtonClicked()
    {
        // Disabling the camera movement for dialogueBox
        cameraController.DisableCamera();

        uiAnimator.crossingImage.SetActive(false);
        if (currentBuilding == null) return;

        int level = PlayerPrefs.GetInt(currentBuilding.buildingName + "_Level", 1);
        int maxLevel = currentBuilding.levelObjects.Length;

        // Safety checks
        if (currentBuilding.purchaseCosts == null || currentBuilding.revenuePerInterval == null)
        {
            Debug.LogError("Building data arrays not initialized!");
            return;
        }

        dialoguePanel.SetActive(true);
        dialoguePanel.transform.localScale = Vector3.zero;
        LeanTween.scale(dialoguePanel, new Vector3(1f, 1f, 1f), 0.3f).setEase(LeanTweenType.easeOutBack);

        // Show appropriate level image
        int displayLevel = Mathf.Clamp(level >= maxLevel ? level : level + 1, 1, maxLevel);
        SetBuildingImage(currentBuilding.buildingName, displayLevel);

        // Trigger pollution phase for maxed Factory
        if (level >= maxLevel && currentBuilding.buildingName == "Factory")
        {
            EnvironmentManager.Instance?.StartPollutionPhase();
        }

        if (level >= maxLevel)
        {
            // Safe revenue access (use last valid level)
            int safeLevel = Mathf.Clamp(level - 1, 0, currentBuilding.revenuePerInterval.Length - 1);
            detailsText.text = $"{currentBuilding.buildingName} - MAX LEVEL\n" +
                             $"Current Revenue: ${currentBuilding.revenuePerInterval[safeLevel]}/{currentBuilding.revenueInterval}sec\n" +
                             $"Type: Earning\n" +
                             $"Building is Maxed Out!";
            confirmButton.SetActive(false);
            uiAnimator.crossingConfirmButton.SetActive(false);
        }
        else
        {
            // Safe array access
            int safeLevel = Mathf.Clamp(level, 0, currentBuilding.purchaseCosts.Length - 1);
            int safeRevenueLevel = Mathf.Clamp(level, 0, currentBuilding.revenuePerInterval.Length - 1);

            detailsText.text = $"Upgrade {currentBuilding.buildingName} to Level {level + 1}\n" +
                             $"Cost: ${currentBuilding.purchaseCosts[safeLevel]}\n" +
                             $"New Revenue: ${currentBuilding.revenuePerInterval[safeRevenueLevel]}/{currentBuilding.revenueInterval}sec\n" +
                             $"Type: Earning";
            confirmButton.SetActive(true);
        }
    }

    public void OnConfirmButtonClicked()
    {
        if (currentBuilding == null)
        {
            Debug.LogError("No building selected!");
            ToastMessageSystem.ShowToast("No building selected!", ToastMessageSystem.ToastType.ERROR);
            return;
        }

        int level = PlayerPrefs.GetInt(currentBuilding.buildingName + "_Level", 0);

        // Check if we can upgrade further
        if (level >= currentBuilding.purchaseCosts.Length)
        {
            Debug.LogError($"Level {level + 1} exceeds purchaseCosts array length ({currentBuilding.purchaseCosts.Length}) for {currentBuilding.buildingName}");
            ToastMessageSystem.ShowToast("Max level reached!", ToastMessageSystem.ToastType.ERROR);
            return;
        }

        int cost = currentBuilding.purchaseCosts[level];

        if (moneyManager.SpendCoins(cost))
        {
            level++;
            UpdateBuildingState(currentBuilding, level);

            // Force save after important changes
            PlayerPrefs.Save();

            if (level == 1) // First purchase
            {
                StartCoroutine(GenerateRevenue(currentBuilding));
                ToastMessageSystem.ShowToast($"Purchased {currentBuilding.buildingName} for ${cost}!", ToastMessageSystem.ToastType.SUCCESS);
            }
            else // Upgrade
            {
                ToastMessageSystem.ShowToast($"Upgraded {currentBuilding.buildingName} to Level {level}!", ToastMessageSystem.ToastType.SUCCESS);
            }

            CloseDialoguePanel();
        }
        else
        {
            ToastMessageSystem.ShowToast("Not enough coins!", ToastMessageSystem.ToastType.WARNING);
            detailsText.text += "\nNot enough money!";
        }
    }

    void InitializeAllBuildings()
    {
        foreach (BuildingData building in buildings)
        {
            // Load level (default to 0 for locked buildings)
            int level = PlayerPrefs.GetInt(building.buildingName + "_Level", 0);

            // Ensure level is within valid range
            level = Mathf.Clamp(level, 0, building.levelObjects.Length);

            // Update visual state
            UpdateBuildingState(building, level);

            // Start revenue generation if building is unlocked
            if (level > 0)
            {
                StartCoroutine(GenerateRevenue(building));
            }
        }

        // No need to PlayerPrefs.Save() here - we're only loading data
    }

    // Updated building state function
    void UpdateBuildingState(BuildingData building, int level)
    {
        // Validate level
        if (level < 0 || level > building.levelObjects.Length)
        {
            Debug.LogError($"Invalid level {level} for {building.buildingName}");
            return;
        }

        // Set active state for each level object
        for (int i = 0; i < building.levelObjects.Length; i++)
        {
            if (building.levelObjects[i] != null)
            {
                building.levelObjects[i].SetActive(i < level);
            }
        }

        // PROPERLY HANDLE LOCK STATE
        if (building.lockObject != null)
        {
            // Always set lock state based on level
            building.lockObject.SetActive(level == 0);

            // Debug to verify lock state changes
            Debug.Log($"Set {building.buildingName} lock state to: {building.lockObject.activeSelf} (level: {level})");
        }

        // Save the level
        PlayerPrefs.SetInt(building.buildingName + "_Level", level);
    }

    IEnumerator GenerateRevenue(BuildingData building)
    {
        while (true)
        {
            yield return new WaitForSeconds(building.revenueInterval);
            int level = PlayerPrefs.GetInt(building.buildingName + "_Level", 0);
            if (level > 0)
            {
                int revenue = building.revenuePerInterval[level - 1];
                moneyManager.AddCoins(revenue);
                TollPopup.Instance?.ShowBuildingRevenue($"+{revenue}$");
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
                     SetAllBuildingImagesInactive();
                     currentBuilding = null;
                     currentBuildingIndex = -1;
                     Debug.Log("Building dialogue closed");
                 });
    }

    void SetBuildingImage(string buildingName, int level)
    {
        SetAllBuildingImagesInactive();
        switch (buildingName)
        {
            case "Hotel":
                HotelImage.SetActive(true);
                break;
            case "Stadium":
                StadiumImage.SetActive(true);
                break;
            case "Factory":
                FactoryImage.SetActive(true);
                break;
            case "Society":
                SocietyImage.SetActive(true);
                break;
            case "Market":
                MarketImage.SetActive(true);
                break;
            case "GasStation":
                GasStationImage.SetActive(true);
                break;
            case "Police":
                PoliceImage.SetActive(true);
                break;
            case "School":
                SchoolImage.SetActive(true);
                break;
            case "WhiteHouse":
                WhiteHouseImage.SetActive(true);
                break;
            case "City2Roads":
                City2RoadsImage.SetActive(true);
                break;

            default:
                Debug.LogWarning($"No image defined for building: {buildingName}");
                break;
        }
    }

    void SetAllBuildingImagesInactive()
    {
        HotelImage.SetActive(false);
        StadiumImage.SetActive(false);
        FactoryImage.SetActive(false);
        SocietyImage.SetActive(false);
        MarketImage.SetActive(false);
        GasStationImage.SetActive(false);
        PoliceImage.SetActive(false);
        SchoolImage.SetActive(false);
        WhiteHouseImage.SetActive(false);
        City2RoadsImage.SetActive(false);
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