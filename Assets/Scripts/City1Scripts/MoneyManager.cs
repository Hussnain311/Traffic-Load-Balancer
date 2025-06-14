using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI coinText; // Assign in Inspector
    private int coins = 0;
    internal static MoneyManager Instance;
    private const string COINS_KEY = "PlayerCoins";
    private const int DEFAULT_COINS = 150000000;

    private void Start()
    {
        coins = PlayerPrefs.GetInt(COINS_KEY, DEFAULT_COINS);
        UpdateCoinText();

        if (coinText == null)
        {
            Debug.LogError("[MoneyManager] ❌ Coin Text (TextMeshPro) is not assigned in the Inspector!");
        }
    }

    // Format coins with K/M suffixes (e.g., 1000 → 1K, 1,500,000 → 1.5M)
    private string FormatCoins(int amount)
    {
        if (amount >= 1000000000) // Billions (1B+)
            return $"{(amount / 1000000000f):0.##}B";
        else if (amount >= 1000000) // Millions (1M+)
            return $"{(amount / 1000000f):0.##}M";
        else if (amount >= 1000) // Thousands (1K+)
            return $"{(amount / 1000f):0.##}K";
        else
            return amount.ToString(); // Small numbers (0-999)
    }

    public void AddCoins(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("[MoneyManager] Tried to add negative coins!");
            return;
        }

        coins += amount;
        SaveCoins();
        UpdateCoinText();
    }

    public bool SpendCoins(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("[MoneyManager] ❌ SpendCoins called with negative amount!");
            return false;
        }

        if (coins >= amount)
        {
            coins -= amount;
            SaveCoins();
            UpdateCoinText();
            return true;
        }
        return false;
    }

    private void UpdateCoinText()
    {
        if (coinText != null)
        {
            coinText.text = $"{FormatCoins(coins)}"; // Apply formatting
        }
    }

    public int GetCoins()
    {
        return coins;
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt(COINS_KEY, coins);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }

    public void ResetCoins()
    {
        coins = DEFAULT_COINS;
        SaveCoins();
        UpdateCoinText();
    }
}