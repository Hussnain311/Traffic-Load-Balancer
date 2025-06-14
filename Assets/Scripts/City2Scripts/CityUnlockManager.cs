using UnityEngine;
using TMPro;

public class CityUnlockManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI detailsText;

    [Header("City Settings")]
    public GameObject lockedCityObject;  // Object with tag "LockedCity"
    public string cityKey = "CityUnlocked_Sandies";
    public int unlockPrice = 50000000;
    public string cityName = "Sandies";

    [Header("Objects to Activate After Unlock")]
    public GameObject[] objectsToActivate;

    [Header("Building Image Objects")]
    public GameObject City2RoadsImage; // Sandies city image

    [Header("References")]
    [SerializeField] private MoneyManager moneyManager;

    private void Start()
    {
        dialoguePanel.SetActive(false);

        if (PlayerPrefs.GetInt(cityKey, 0) == 1)
        {
            if (lockedCityObject != null)
                lockedCityObject.SetActive(false);

            ActivateAdditionalObjects();
            ActivateCityImage();
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DetectLockedCityTap();
        }
    }

    private void DetectLockedCityTap()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("LockedCity"))
            {
                ShowUnlockDialogue();
            }
        }
    }

    private void ShowUnlockDialogue()
    {
        detailsText.text = $"<b>New City Unlocked!</b>\n\n" +
                           $"<b>Name:</b> {cityName}\n" +
                           $"<b>Price:</b> ${unlockPrice:N0}";

       
        dialoguePanel.SetActive(true);

        ActivateCityImage();
    }



    public void TryUnlockCity()
    {
        if (!moneyManager.SpendCoins(unlockPrice))
        {
            Debug.Log("[CityUnlockManager] ❌ Not enough coins to unlock.");
            return;
        }

        PlayerPrefs.SetInt(cityKey, 1);
        PlayerPrefs.Save();

        if (lockedCityObject != null)
            lockedCityObject.SetActive(false);

        ActivateAdditionalObjects();
        ActivateCityImage();
        dialoguePanel.SetActive(false);
    }

    public void CloseDialogue()
    {
      
            dialoguePanel.SetActive(false);
            City2RoadsImage.SetActive(false);
            dialoguePanel.transform.localScale = Vector3.one; // Reset scale for next time
    }



    private void ActivateAdditionalObjects()
    {
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }

    private void ActivateCityImage()
    {
        if (City2RoadsImage != null)
            City2RoadsImage.SetActive(true);
    }
}
