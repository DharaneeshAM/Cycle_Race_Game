using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class WinSceneManager : MonoBehaviour
{
    #region ==================== PANELS ====================

    [Header("=== PANELS ===")]
    [Tooltip("Main panel showing the winner")]
    public GameObject panel_Winner;

    [Tooltip("Panel showing race history")]
    public GameObject panel_History;

    #endregion

    #region ==================== WINNER PANEL ====================

    [Header("=== WINNER PANEL UI ===")]
    public TextMeshProUGUI txt_WinnerName;
    public TextMeshProUGUI txt_WinnerDistance;

    [Header("=== WINNER PANEL BUTTONS ===")]
    public Button btn_History;
    public Button btn_PlayAgain;
    public Button btn_MainMenu;

    #endregion

    #region ==================== HISTORY PANEL ====================

    [Header("=== HISTORY PANEL UI ===")]
    [Tooltip("Content transform of the ScrollView (vertical layout)")]
    public Transform scrollContent;

    [Tooltip("Prefab for each history row")]
    public GameObject attemptRowPrefab;

    [Header("=== HISTORY PANEL BUTTONS ===")]
    public Button btn_BackFromHistory;

    [Header("=== AUDIO ===")]
    public AudioSource uiAudioSource;
    public AudioClip buttonClickSound;

    #endregion

    #region ==================== SCENE NAMES ====================

    [Header("=== SCENE NAVIGATION ===")]
    public string homeSceneName = "HomeScene";
    public string gameSceneName = "GameScene";

    #endregion

    #region ==================== UNITY LIFECYCLE ====================

    void Start()
    {
        SetupListeners();
        LoadWinnerData();
        ShowPanel(PanelType.Winner);

        Debug.Log("[WinScene] Initialized");
    }

    #endregion

    #region ==================== SETUP ====================

    void SetupListeners()
    {
        if (btn_History != null)
            btn_History.onClick.AddListener(OnClick_History);

        if (btn_PlayAgain != null)
            btn_PlayAgain.onClick.AddListener(OnClick_PlayAgain);

        if (btn_MainMenu != null)
            btn_MainMenu.onClick.AddListener(OnClick_MainMenu);

        if (btn_BackFromHistory != null)
            btn_BackFromHistory.onClick.AddListener(OnClick_BackFromHistory);
    }

    void LoadWinnerData()
    {
        string winnerName = PlayerPrefs.GetString("WinnerName", "Unknown");
        float winnerDistance = PlayerPrefs.GetFloat("WinnerDistance", 0f);

        if (txt_WinnerName != null)
            txt_WinnerName.text = winnerName;

        if (txt_WinnerDistance != null)
            txt_WinnerDistance.text = $"{winnerDistance:F1}m";

        Debug.Log($"[WinScene] Winner: {winnerName} - {winnerDistance:F1}m");
    }

    #endregion

    #region ==================== PANEL NAVIGATION ====================

    enum PanelType { Winner, History }

    void ShowPanel(PanelType panelType)
    {
        if (panel_Winner != null)
            panel_Winner.SetActive(panelType == PanelType.Winner);

        if (panel_History != null)
            panel_History.SetActive(panelType == PanelType.History);

        Debug.Log($"[WinScene] Showing panel: {panelType}");
    }

    #endregion

    #region ==================== BUTTON HANDLERS ====================

    void OnClick_History()
    {
        PopulateHistory();
        ShowPanel(PanelType.History);

        PlayClickSound();
    }

    void OnClick_PlayAgain()
    {
        Debug.Log("[WinScene] Play Again clicked");

        if (!string.IsNullOrEmpty(gameSceneName))
            SceneManager.LoadScene(gameSceneName);
        else
            Debug.LogError("[WinScene] Game scene name not set!");

        PlayClickSound();
    }

    void OnClick_MainMenu()
    {
        Debug.Log("[WinScene] Main Menu clicked");

        if (!string.IsNullOrEmpty(homeSceneName))
            SceneManager.LoadScene(homeSceneName);
        else
            Debug.LogError("[WinScene] Home scene name not set!");

        PlayClickSound();
    }

    void OnClick_BackFromHistory()
    {
        ShowPanel(PanelType.Winner);
        PlayClickSound();
    }

    #endregion

    #region ==================== HISTORY ====================

    void PopulateHistory()
    {
        if (scrollContent == null)
        {
            Debug.LogError("[WinScene] Scroll content not assigned!");
            return;
        }

        if (attemptRowPrefab == null)
        {
            Debug.LogError("[WinScene] Attempt row prefab not assigned!");
            return;
        }

        foreach (Transform child in scrollContent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 1; i <= 5; i++)
        {
            string playerName = PlayerPrefs.GetString($"HistoryName_{i}", "");
            float distance = PlayerPrefs.GetFloat($"HistoryDist_{i}", 0f);

            if (string.IsNullOrEmpty(playerName))
                continue;

            GameObject row = Instantiate(attemptRowPrefab, scrollContent);
            row.name = $"AttemptRow_{i}";

            SetRowData(row, i, playerName, distance);
        }

        Debug.Log("[WinScene] History populated");
    }

    void SetRowData(GameObject row, int number, string playerName, float distance)
    {
        TMP_Text[] texts = row.GetComponentsInChildren<TMP_Text>();

        if (texts.Length >= 3)
        {
            texts[0].text = number.ToString();
            texts[1].text = playerName;
            texts[2].text = $"{distance:F1}m";
        }
        else
        {
            Transform numberTransform = row.transform.Find("NumberText");
            Transform nameTransform = row.transform.Find("PlayerNameText");
            Transform distTransform = row.transform.Find("DistanceText");

            if (numberTransform != null)
            {
                TMP_Text numText = numberTransform.GetComponent<TMP_Text>();
                if (numText != null) numText.text = number.ToString();
            }

            if (nameTransform != null)
            {
                TMP_Text nameText = nameTransform.GetComponent<TMP_Text>();
                if (nameText != null) nameText.text = playerName;
            }

            if (distTransform != null)
            {
                TMP_Text distText = distTransform.GetComponent<TMP_Text>();
                if (distText != null) distText.text = $"{distance:F1}m";
            }
        }
    }

    #endregion

    #region ==================== PUBLIC API ====================

    public void ClearHistory()
    {
        for (int i = 1; i <= 5; i++)
        {
            PlayerPrefs.DeleteKey($"HistoryName_{i}");
            PlayerPrefs.DeleteKey($"HistoryDist_{i}");
        }
        PlayerPrefs.Save();

        Debug.Log("[WinScene] History cleared");
    }
    public int GetHistoryCount()
    {
        int count = 0;
        for (int i = 1; i <= 5; i++)
        {
            if (!string.IsNullOrEmpty(PlayerPrefs.GetString($"HistoryName_{i}", "")))
                count++;
        }
        return count;
    }
    void PlayClickSound()
    {
        if (uiAudioSource != null && buttonClickSound != null)
        {
            uiAudioSource.PlayOneShot(buttonClickSound);
        }
    }

    #endregion
}