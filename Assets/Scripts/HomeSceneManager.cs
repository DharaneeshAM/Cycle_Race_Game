using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class HomeScreenManager : MonoBehaviour
{
    #region ==================== PANEL REFERENCES ====================

    [Header("=== PANELS ===")]
    public GameObject panel_Home;
    public GameObject panel_CycleSelection;
    public GameObject panel_Settings;

    #endregion

    #region ==================== PANEL 1: HOME ====================

    [Header("=== PANEL 1: HOME - Input Fields ===")]
    public TMP_InputField player1NameInput;
    public TMP_InputField player2NameInput;

    [Header("=== PANEL 1: HOME - Buttons ===")]
    public Button btn_Start;
    public Button btn_OpenSettings;
    public Button QuiteButton;

    #endregion

    #region ==================== PANEL 2: CYCLE SELECTION ====================

    [Header("=== PANEL 2: Player 1 Cycle Buttons (Assign 4) ===")]
    public Button[] player1CycleButtons;

    [Header("=== PANEL 2: Player 2 Cycle Buttons (Assign 4) ===")]
    public Button[] player2CycleButtons;

    [Header("=== PANEL 2: Buttons ===")]
    public Button btn_Player1Confirm;
    public Button btn_ReadyToRace;

    #endregion

    #region ==================== PANEL 3: SETTINGS ====================

    [Header("=== PANEL 3: Winning Meter ===")]
    public Slider slider_WinningMeter;
    public TextMeshProUGUI txt_WinningMeterValue;
    public float minWinningMeter = 50f;
    public float maxWinningMeter = 1000f;

    [Header("=== PANEL 3: Theme Buttons ===")]
    public Button btn_ThemeA;
    public Button btn_ThemeB;
    public Button btn_ThemeC;

    [Header("=== PANEL 3: Theme Sprites ===")]
    public Sprite themeA_Sprite;
    public Sprite themeB_Sprite;
    public Sprite themeC_Sprite;

    [Header("=== PANEL 3: Panel Background Images ===")]
    public Image img_HomeBackground;
    public Image img_CycleSelectionBackground;
    public Image img_SettingsBackground;

    [Header("=== PANEL 3: Logo Position Buttons ===")]
    public Button btn_LogoLeft;
    public Button btn_LogoCenter;
    public Button btn_LogoRight;

    [Header("=== PANEL 3: Name Entry Toggle ===")]
    public Toggle toggle_NameEntry;

    [Header("=== PANEL 3: Save & Back Buttons ===")]
    public Button btn_SaveSettings;
    public Button btn_BackFromSettings;

    #endregion

    #region ==================== SCENE SETTINGS ====================

    [Header("=== SCENE SETTINGS ===")]
    public string raceSceneName = "GameScene";

    [Header("=== SELECTION COLORS ===")]
    public Color selectedColor = Color.green;
    public Color unselectedColor = Color.white;

    #endregion

    #region ==================== AUDIO ==================

    [Header("=== AUDIO ===")]
    public AudioSource uiAudioSource;
    public AudioClip buttonClickSound;


    #endregion

    #region ==================== PRIVATE VARIABLES ====================

    private int player1SelectedCycle = -1;
    private int player2SelectedCycle = -1;
    private bool isPlayer1Confirmed = false;

    private float tempWinningMeter = 100f;
    private int tempTheme = 0;
    private int tempLogoPosition = 1;
    private bool tempNameEntryEnabled = true;

    private const string KEY_WINNING_METER = "WinningMeter";
    private const string KEY_THEME = "UITheme";
    private const string KEY_LOGO_POSITION = "LogoPosition";
    private const string KEY_NAME_ENTRY = "NameEntryEnabled";
    private const string KEY_P1_NAME = "Player1Name";
    private const string KEY_P2_NAME = "Player2Name";
    private const string KEY_P1_CYCLE = "Player1Cycle";
    private const string KEY_P2_CYCLE = "Player2Cycle";

    #endregion

    #region ==================== UNITY LIFECYCLE ====================

    void Start()
    {
        LoadSettings();
        ShowPanel(1);
        ApplySettings();
        SetupListeners();
        InitializeUI();

        Debug.Log("[HomeScreen] Initialized");
    }

    #endregion

    #region ==================== SETUP ====================

    void SetupListeners()
    {
        if (player1NameInput != null)
            player1NameInput.onValueChanged.AddListener(delegate { CheckStartButton(); });
        if (player2NameInput != null)
            player2NameInput.onValueChanged.AddListener(delegate { CheckStartButton(); });

        if (btn_Start != null)
            btn_Start.onClick.AddListener(OnClick_Start);
        if (btn_OpenSettings != null)
            btn_OpenSettings.onClick.AddListener(OnClick_OpenSettings);

        for (int i = 0; i < player1CycleButtons.Length; i++)
        {
            int index = i;
            if (player1CycleButtons[i] != null)
                player1CycleButtons[i].onClick.AddListener(() => SelectPlayer1Cycle(index));
        }

        for (int i = 0; i < player2CycleButtons.Length; i++)
        {
            int index = i;
            if (player2CycleButtons[i] != null)
                player2CycleButtons[i].onClick.AddListener(() => SelectPlayer2Cycle(index));
        }

        if (btn_Player1Confirm != null)
            btn_Player1Confirm.onClick.AddListener(OnClick_Player1Confirm);
        if (btn_ReadyToRace != null)
            btn_ReadyToRace.onClick.AddListener(OnClick_ReadyToRace);

        if (slider_WinningMeter != null)
        {
            slider_WinningMeter.minValue = minWinningMeter;
            slider_WinningMeter.maxValue = maxWinningMeter;
            slider_WinningMeter.onValueChanged.AddListener(OnWinningMeterChanged);
        }

        if (btn_ThemeA != null)
            btn_ThemeA.onClick.AddListener(() => OnThemeSelected(0));
        if (btn_ThemeB != null)
            btn_ThemeB.onClick.AddListener(() => OnThemeSelected(1));
        if (btn_ThemeC != null)
            btn_ThemeC.onClick.AddListener(() => OnThemeSelected(2));

        if (btn_LogoLeft != null)
            btn_LogoLeft.onClick.AddListener(() => OnLogoPositionSelected(0));
        if (btn_LogoCenter != null)
            btn_LogoCenter.onClick.AddListener(() => OnLogoPositionSelected(1));
        if (btn_LogoRight != null)
            btn_LogoRight.onClick.AddListener(() => OnLogoPositionSelected(2));

        if (toggle_NameEntry != null)
            toggle_NameEntry.onValueChanged.AddListener(OnNameEntryToggleChanged);

        if (btn_SaveSettings != null)
            btn_SaveSettings.onClick.AddListener(OnClick_SaveSettings);
        if (btn_BackFromSettings != null)
            btn_BackFromSettings.onClick.AddListener(OnClick_BackFromSettings);

         QuiteButton.onClick.AddListener(QuiteGame);
    }

    void InitializeUI()
    {
        CheckStartButton();
        ResetCycleSelection();
        UpdateNameInputVisibility();
    }

    #endregion

    #region ==================== SETTINGS ====================

    void LoadSettings()
    {
        tempWinningMeter = PlayerPrefs.GetFloat(KEY_WINNING_METER, 100f);
        tempTheme = PlayerPrefs.GetInt(KEY_THEME, 0);
        tempLogoPosition = PlayerPrefs.GetInt(KEY_LOGO_POSITION, 1);
        tempNameEntryEnabled = PlayerPrefs.GetInt(KEY_NAME_ENTRY, 1) == 1;

        Debug.Log($"[HomeScreen] Loaded: Meter={tempWinningMeter}, Theme={tempTheme}, Logo={tempLogoPosition}, NameEntry={tempNameEntryEnabled}");
    }

    void SaveSettings()
    {
        PlayerPrefs.SetFloat(KEY_WINNING_METER, tempWinningMeter);
        PlayerPrefs.SetInt(KEY_THEME, tempTheme);
        PlayerPrefs.SetInt(KEY_LOGO_POSITION, tempLogoPosition);
        PlayerPrefs.SetInt(KEY_NAME_ENTRY, tempNameEntryEnabled ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"[HomeScreen] Saved: Meter={tempWinningMeter}, Theme={tempTheme}, Logo={tempLogoPosition}, NameEntry={tempNameEntryEnabled}");
    }

    void ApplySettings()
    {
        if (slider_WinningMeter != null)
        {
            slider_WinningMeter.value = tempWinningMeter;
            UpdateWinningMeterText();
        }
        ApplyTheme(tempTheme);
        UpdateThemeHighlights();

        UpdateLogoHighlights();

        if (toggle_NameEntry != null)
            toggle_NameEntry.isOn = tempNameEntryEnabled;

        UpdateNameInputVisibility();
    }

    #endregion

    #region ==================== PANEL NAVIGATION ====================

    void ShowPanel(int panelNumber)
    {
        if (panel_Home != null)
            panel_Home.SetActive(panelNumber == 1);
        if (panel_CycleSelection != null)
            panel_CycleSelection.SetActive(panelNumber == 2);
        if (panel_Settings != null)
            panel_Settings.SetActive(panelNumber == 3);

        Debug.Log($"[HomeScreen] Panel {panelNumber} active");
    }

    #endregion

    #region ==================== PANEL 1: HOME ====================

    void CheckStartButton()
    {
        if (!tempNameEntryEnabled)
        {
            if (btn_Start != null)
                btn_Start.interactable = true;
            return;
        }

        bool p1Valid = player1NameInput != null && !string.IsNullOrWhiteSpace(player1NameInput.text);
        bool p2Valid = player2NameInput != null && !string.IsNullOrWhiteSpace(player2NameInput.text);

        if (btn_Start != null)
            btn_Start.interactable = p1Valid && p2Valid;
    }

    void UpdateNameInputVisibility()
    {
        if (player1NameInput != null)
            player1NameInput.gameObject.SetActive(tempNameEntryEnabled);
        if (player2NameInput != null)
            player2NameInput.gameObject.SetActive(tempNameEntryEnabled);

        CheckStartButton();
    }

    void OnClick_Start()
    {
        string p1Name = tempNameEntryEnabled && player1NameInput != null ? player1NameInput.text : "Player 1";
        string p2Name = tempNameEntryEnabled && player2NameInput != null ? player2NameInput.text : "Player 2";

        if (string.IsNullOrWhiteSpace(p1Name)) p1Name = "Player 1";
        if (string.IsNullOrWhiteSpace(p2Name)) p2Name = "Player 2";

        PlayerPrefs.SetString(KEY_P1_NAME, p1Name);
        PlayerPrefs.SetString(KEY_P2_NAME, p2Name);

        Debug.Log($"[HomeScreen] Players: {p1Name} vs {p2Name}");

        ResetCycleSelection();
        SetupForPlayer1();
        ShowPanel(2);

        PlayClickSound();
    }

    void OnClick_OpenSettings()
    {
        LoadSettings();
        ApplySettings();
        ShowPanel(3);

        PlayClickSound();
    }

    #endregion

    #region ==================== PANEL 2: CYCLE SELECTION ====================

    void ResetCycleSelection()
    {
        player1SelectedCycle = -1;
        player2SelectedCycle = -1;
        isPlayer1Confirmed = false;

        ResetButtonColors(player1CycleButtons);
        ResetButtonColors(player2CycleButtons);

        if (btn_Player1Confirm != null)
            btn_Player1Confirm.interactable = false;
        if (btn_ReadyToRace != null)
            btn_ReadyToRace.interactable = false;
    }

    void SetupForPlayer1()
    {
        SetButtonsInteractable(player1CycleButtons, true);
        SetButtonsInteractable(player2CycleButtons, false);

        if (btn_Player1Confirm != null)
        {
            btn_Player1Confirm.gameObject.SetActive(true);
            btn_Player1Confirm.interactable = false;
        }
        if (btn_ReadyToRace != null)
            btn_ReadyToRace.gameObject.SetActive(false);

        isPlayer1Confirmed = false;
    }

    void SetupForPlayer2()
    {
        SetButtonsInteractable(player1CycleButtons, false);
        SetButtonsInteractable(player2CycleButtons, true);

        if (btn_Player1Confirm != null)
            btn_Player1Confirm.gameObject.SetActive(false);
        if (btn_ReadyToRace != null)
        {
            btn_ReadyToRace.gameObject.SetActive(true);
            btn_ReadyToRace.interactable = false;
        }
    }

    void SelectPlayer1Cycle(int index)
    {
        if (isPlayer1Confirmed) return;

        player1SelectedCycle = index;
        HighlightButton(player1CycleButtons, index);

        if (btn_Player1Confirm != null)
            btn_Player1Confirm.interactable = true;

        Debug.Log($"[HomeScreen] P1 selected cycle {index}");
    }

    void SelectPlayer2Cycle(int index)
    {
        player2SelectedCycle = index;
        HighlightButton(player2CycleButtons, index);

        if (btn_ReadyToRace != null)
            btn_ReadyToRace.interactable = true;

        Debug.Log($"[HomeScreen] P2 selected cycle {index}");
    }

    void OnClick_Player1Confirm()
    {
        if (player1SelectedCycle == -1) return;

        isPlayer1Confirmed = true;
        PlayerPrefs.SetInt(KEY_P1_CYCLE, player1SelectedCycle);

        Debug.Log($"[HomeScreen] P1 confirmed cycle {player1SelectedCycle}");

        SetupForPlayer2();
        PlayClickSound();
    }

    void OnClick_ReadyToRace()
    {
        if (player2SelectedCycle == -1) return;

        PlayerPrefs.SetInt(KEY_P2_CYCLE, player2SelectedCycle);

        Debug.Log($"[HomeScreen] P2 confirmed cycle {player2SelectedCycle}. Loading race...");

        if (!string.IsNullOrEmpty(raceSceneName))
            SceneManager.LoadScene(raceSceneName);
        else
            Debug.LogError("[HomeScreen] Race scene name not set!");

        PlayClickSound();
    }

    void HighlightButton(Button[] buttons, int selectedIndex)
    {
        if (buttons == null) return;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                ColorBlock colors = buttons[i].colors;
                colors.normalColor = (i == selectedIndex) ? selectedColor : unselectedColor;
                buttons[i].colors = colors;
            }
        }
    }

    void ResetButtonColors(Button[] buttons)
    {
        if (buttons == null) return;

        foreach (var btn in buttons)
        {
            if (btn != null)
            {
                ColorBlock colors = btn.colors;
                colors.normalColor = unselectedColor;
                btn.colors = colors;
            }
        }
    }

    void SetButtonsInteractable(Button[] buttons, bool interactable)
    {
        if (buttons == null) return;

        foreach (var btn in buttons)
        {
            if (btn != null)
                btn.interactable = interactable;
        }
    }

    #endregion

    #region ==================== PANEL 3: SETTINGS ====================

    void OnWinningMeterChanged(float value)
    {
        tempWinningMeter = value;
        UpdateWinningMeterText();
    }

    void UpdateWinningMeterText()
    {
        if (txt_WinningMeterValue != null)
            txt_WinningMeterValue.text = $"{tempWinningMeter:F0}m";
    }

    void OnThemeSelected(int themeIndex)
    {
        tempTheme = themeIndex;
        ApplyTheme(themeIndex);
        UpdateThemeHighlights();
        Debug.Log($"[HomeScreen] Theme {themeIndex} selected");
        PlayClickSound();
    }

    void ApplyTheme(int themeIndex)
    {
        Sprite themeSprite = themeIndex switch
        {
            0 => themeA_Sprite,
            1 => themeB_Sprite,
            2 => themeC_Sprite,
            _ => themeA_Sprite
        };

        if (themeSprite != null)
        {
            if (img_HomeBackground != null)
                img_HomeBackground.sprite = themeSprite;
            if (img_CycleSelectionBackground != null)
                img_CycleSelectionBackground.sprite = themeSprite;
            if (img_SettingsBackground != null)
                img_SettingsBackground.sprite = themeSprite;
        }
    }

    void UpdateThemeHighlights()
    {
        SetButtonColor(btn_ThemeA, tempTheme == 0);
        SetButtonColor(btn_ThemeB, tempTheme == 1);
        SetButtonColor(btn_ThemeC, tempTheme == 2);
    }

    void OnLogoPositionSelected(int position)
    {
        tempLogoPosition = position;
        UpdateLogoHighlights();
        Debug.Log($"[HomeScreen] Logo position {position} (0=Left, 1=Center, 2=Right)");
    }

    void UpdateLogoHighlights()
    {
        SetButtonColor(btn_LogoLeft, tempLogoPosition == 0);
        SetButtonColor(btn_LogoCenter, tempLogoPosition == 1);
        SetButtonColor(btn_LogoRight, tempLogoPosition == 2);
    }

    void OnNameEntryToggleChanged(bool isOn)
    {
        tempNameEntryEnabled = isOn;
        Debug.Log($"[HomeScreen] Name entry: {isOn}");
    }

    void SetButtonColor(Button btn, bool isSelected)
    {
        if (btn == null) return;

        ColorBlock colors = btn.colors;
        colors.normalColor = isSelected ? selectedColor : unselectedColor;
        btn.colors = colors;
    }

    void OnClick_SaveSettings()
    {
        SaveSettings();
        ApplySettings();
        UpdateNameInputVisibility();
        ShowPanel(1);
        Debug.Log("[HomeScreen] Settings saved");
        PlayClickSound();
    }

    void OnClick_BackFromSettings()
    {
        LoadSettings();
        ApplySettings();
        ShowPanel(1);
        Debug.Log("[HomeScreen] Settings discarded");
        PlayClickSound();
    }
    void QuiteGame()
    {
        PlayClickSound();
        Debug.Log("Quit button pressed");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
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