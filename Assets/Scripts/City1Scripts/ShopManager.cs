using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [System.Serializable]
    public class ItemData
    {
        public string name;
        public GameObject[] objectLevels;
        public int[] price;
        public int[] income;
        public Button purchaseButton;
        public TextMeshProUGUI purchaseText;
        [HideInInspector] public int currentLevel = 0;
        public Vector3[] cameraPositions; // Camera positions for each level
    }

    [Header("Item Data")]
    [SerializeField] private ItemData[] items;

    [Header("UI Settings")]
    [SerializeField] private GameObject shopBox;
    [SerializeField] private GameObject confirmBox;
    [SerializeField] private TextMeshProUGUI detailsBoxText;
    [SerializeField] private GameObject confirmButton; 
   

    [Header("Button Colors")]
    [SerializeField] private Color normalButtonColor = Color.white;
    [SerializeField] private Color maxedOutButtonColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);


    [Header("References")]
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private CameraControllerWithAutoMove cameraController;


    [Header("Tree Placement Settings")]
    [SerializeField] private GameObject treePrefab;
    [SerializeField] private int maxTreesPerPurchase = 5;

    private bool isPlacingTree = false;
    private int placedTreeCount = 0;


    private const string LEVEL_KEY_PREFIX = "ShopItem_";
    private int currentSelectedItemIndex = -1;


    private void Start()
    {
        if (moneyManager == null)
        {
            Debug.LogError("[ShopManager] MoneyManager is not assigned!");
            return;
        }

        shopBox.SetActive(false);
        confirmBox.SetActive(false);
        LoadPurchasedLevels();

    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && isPlacingTree)
        {
            TryPlaceTree();
        }
    }
   

    public void EnableTreePlacement()
    {
        placedTreeCount = 0;
        isPlacingTree = true;
        ToastMessageSystem.ShowToast("Tap on the ground to place a tree.", ToastMessageSystem.ToastType.INFO);
        CloseShop();
    }

    public void DisableTreePlacement()
    {
        isPlacingTree = false;
    }


    private void TryPlaceTree()
    {
        if (!isPlacingTree) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Ground"))
            {
                const int treeCost = 100000;

                if (moneyManager.SpendCoins(treeCost))
                {
                    Instantiate(treePrefab, hit.point, Quaternion.identity);
                    placedTreeCount++;

                    ToastMessageSystem.ShowToast($"Tree placed! ({placedTreeCount}/5)", ToastMessageSystem.ToastType.SUCCESS);

                    if (placedTreeCount >= maxTreesPerPurchase)
                    {
                        isPlacingTree = false;
                        ToastMessageSystem.ShowToast("Maximum of 5 trees placed. Purchase again to plant more.", ToastMessageSystem.ToastType.INFO);
                    }
                }
                else
                {
                    ToastMessageSystem.ShowToast("Not enough coins to place a tree!", ToastMessageSystem.ToastType.ERROR);
                }
            }
            else
            {
                ToastMessageSystem.ShowToast("You cannot place a tree here!", ToastMessageSystem.ToastType.WARNING);
            }
        }
    }

    public void OpenShop()
    {
        cameraController.DisableCamera();
        shopBox.SetActive(true);
        
    }

    public void CloseShop()
    {
       
            shopBox.SetActive(false);
        confirmBox.SetActive(false);
        cameraController.EnableCamera();
    }



    private void LoadPurchasedLevels()
    {
        for (int i = 0; i < items.Length; i++)
        {
            string key = LEVEL_KEY_PREFIX + items[i].name;
            items[i].currentLevel = PlayerPrefs.GetInt(key, 0);
            UpdateItemVisuals(i);
            UpdatePurchaseButtonState(i);
        }
    }

    private void UpdateItemVisuals(int itemIndex)
    {
        var item = items[itemIndex];
        for (int i = 0; i < item.objectLevels.Length; i++)
        {
            if (item.objectLevels[i] != null)
                item.objectLevels[i].SetActive(i < item.currentLevel);
        }
    }

    private void UpdatePurchaseButtonState(int itemIndex)
    {
        var item = items[itemIndex];
        bool isMaxed = item.currentLevel >= item.price.Length;

        if (item.purchaseButton != null)
        {
            item.purchaseButton.interactable = !isMaxed;
            var colors = item.purchaseButton.colors;
            colors.disabledColor = maxedOutButtonColor;
            item.purchaseButton.colors = colors;
        }

        if (item.purchaseText != null)
        {
            item.purchaseText.text = isMaxed ? "Maxed Out!" : "Purchase";
        }
    }

    public void PurchaseFlyover() => ShowPurchaseConfirmation(GetItemIndex("Flyover"));
    public void PurchaseFarm() => ShowPurchaseConfirmation(GetItemIndex("Farm"));
    public void PurchaseTrees() => ShowPurchaseConfirmation(GetItemIndex("Trees"));

    private int GetItemIndex(string itemName)
    {
        for (int i = 0; i < items.Length; i++)
            if (items[i].name == itemName)
                return i;
        Debug.LogError($"Item {itemName} not found!");
        return -1;
    }

    private void ShowPurchaseConfirmation(int itemIndex)
    {
        if (itemIndex == -1) return;

        currentSelectedItemIndex = itemIndex;
        var item = items[itemIndex];

        if (item.currentLevel >= item.price.Length)
        {
            detailsBoxText.text = "You've already purchased all levels!";
            confirmBox.SetActive(true);
            return;
        }

        int nextLevel = item.currentLevel + 1;
        detailsBoxText.text = $"Purchase {item.name} Level {nextLevel} for {item.price[nextLevel - 1]} coins?";
        confirmBox.SetActive(true);

        if (item.name == "Trees")
        {
            EnvironmentManager.Instance.OnTreePurchased(item.currentLevel);
        }

    }

    public void ConfirmPurchase()
    {
        if (currentSelectedItemIndex == -1) return;

        var item = items[currentSelectedItemIndex];
        int nextLevel = item.currentLevel + 1;
        int cost = item.price[nextLevel - 1];

        if (moneyManager.SpendCoins(cost))
        {
            item.currentLevel = nextLevel;
            PlayerPrefs.SetInt(LEVEL_KEY_PREFIX + item.name, item.currentLevel);

            UpdateItemVisuals(currentSelectedItemIndex);
            UpdatePurchaseButtonState(currentSelectedItemIndex);

            // Move camera to appropriate position
            if (item.cameraPositions != null && nextLevel <= item.cameraPositions.Length)
            {
                cameraController.MoveToPosition(item.cameraPositions[nextLevel - 1]);
            }

            ToastMessageSystem.ShowToast($"Purchased {item.name} Level {nextLevel}!", ToastMessageSystem.ToastType.SUCCESS);
        }
        else
        {
            ToastMessageSystem.ShowToast("Not enough coins!", ToastMessageSystem.ToastType.ERROR);
        }

        confirmBox.SetActive(false);
        CloseShop();
    }

    public void CancelPurchase()
    {
        confirmBox.SetActive(false);
        currentSelectedItemIndex = -1;
    }
}