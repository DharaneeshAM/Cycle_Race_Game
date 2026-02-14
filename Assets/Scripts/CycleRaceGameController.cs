using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class CycleRaceGameController : MonoBehaviour
{
    #region ==================== CONFIGURATION ====================

    [System.Serializable]
    public class GameConfig
    {
        [Header("Race Settings")]
        public float winningDistance = 100f;         
        public float countdownDuration = 3f;           
        public float winnerDisplayDelay = 5f;          

        [Header("Movement Settings")]
        public float maxSpeed = 15f;            
        public float acceleration = 5f;             
        public float deceleration = 8f;               
        public float turnSpeed = 80f;                 
        public float leanAngle = 15f;                
        public float leanSmoothing = 5f;              

        [Header("Energy Settings")]
        public float boostSpeedMultiplier = 1.5f;    
        public float maxEnergy = 100f;              
        public float energyDrainRate = 30f;         
        public float energyRechargeRate = 15f;       
        public float minEnergyToBoost = 10f;          

        [Header("Physics Settings")]
        public float groundCheckDistance = 0.5f;      
        public float stabilizationForce = 10f;        
        public float antiWheelieForce = 50f;         
        public float maxAngularVelocity = 2f;         
        public LayerMask groundLayer;                

        [Header("Track Boundary Settings")]
        public LayerMask trackBoundary;               
        public float boundaryPushForce = 20f;        
        public float boundaryCheckDistance = 1f;      
    }

    [System.Serializable]
    public class PlayerControls
    {
        [Header("Movement Keys")]
        public KeyCode forward = KeyCode.W;
        public KeyCode backward = KeyCode.S;
        public KeyCode left = KeyCode.A;
        public KeyCode right = KeyCode.D;

        [Header("Boost Key")]
        public KeyCode boost = KeyCode.LeftShift;
    }

    [System.Serializable]
    public class AnimationConfig
    {
        [Header("Animator Reference")]
        public Animator characterAnimator;

        [Header("Animation Parameter Names")]
        [Tooltip("Parameter name for Idle state (Bool)")]
        public string idleParam = "Idel";

        [Tooltip("Parameter name for Normal/Moving state (Bool)")]
        public string normalParam = "Normal";

        [Tooltip("Parameter name for Speed/Boost state (Bool)")]
        public string speedParam = "Speed";

        [Header("Animation State Tracking")]
        [HideInInspector] public AnimState currentState = AnimState.Idle;
    }

    public enum AnimState { Idle, Normal, Speed }

    [System.Serializable]
    public class PlayerConfig
    {
        [Header("Player Info")]
        public string playerName = "Player";

        [Header("Cycle References")]
        public Transform cycleTransform;
        public Rigidbody cycleRigidbody;
        public Collider cycleCollider;
        public MonoBehaviour bicycleVehicleScript;

        [Header("Controls (Customizable)")]
        public PlayerControls controls = new PlayerControls();

        [Header("Animation")]
        public AnimationConfig animation = new AnimationConfig();

        [HideInInspector] public float currentSpeed = 0f;
        [HideInInspector] public float distanceTraveled = 0f;
        [HideInInspector] public Vector3 lastPosition;
        [HideInInspector] public float currentLean = 0f;
        [HideInInspector] public bool isFinished = false;
        [HideInInspector] public float currentEnergy = 100f;
        [HideInInspector] public bool isBoosting = false;

        [HideInInspector] public float verticalInput = 0f;
        [HideInInspector] public float horizontalInput = 0f;
        [HideInInspector] public bool boostInput = false;
        [HideInInspector] public bool isMoving = false;
    }

    [System.Serializable]
    public class CountdownLights
    {
        [Header("Traffic Light GameObjects (with Image component)")]
        public GameObject light1;
        public GameObject light2;
        public GameObject light3;

        [Header("Light Sprites")]
        public Sprite redSprite;
        public Sprite greenSprite;
    }

    [System.Serializable]
    public class UIReferences
    {
        [Header("Countdown Lights")]
        public CountdownLights countdownLights;

        [Header("Player 1 UI")]
        public TextMeshProUGUI player1NameText;
        public TextMeshProUGUI player1DistanceText;
        public TextMeshProUGUI player1SpeedText;
        public Slider player1EnergySlider;
        public Image player1EnergyFill;

        [Header("Player 2 UI")]
        public TextMeshProUGUI player2NameText;
        public TextMeshProUGUI player2DistanceText;
        public TextMeshProUGUI player2SpeedText;
        public Slider player2EnergySlider;
        public Image player2EnergyFill;

        [Header("Winner Panel")]
        public GameObject winnerPanel;
        public TextMeshProUGUI winnerText;
        public TextMeshProUGUI TotalDistanceText;
    }

    #endregion

    #region ==================== REFERENCES ====================

    [Header("=== GAME CONFIGURATION ===")]
    [SerializeField] private GameConfig config = new GameConfig();

    [Header("=== PLAYER 1 ===")]
    [SerializeField]
    private PlayerConfig player1 = new PlayerConfig
    {
        playerName = "Player 1",
        controls = new PlayerControls
        {
            forward = KeyCode.W,
            backward = KeyCode.S,
            left = KeyCode.A,
            right = KeyCode.D,
            boost = KeyCode.LeftShift
        }
    };

    [Header("=== PLAYER 2 ===")]
    [SerializeField]
    private PlayerConfig player2 = new PlayerConfig
    {
        playerName = "Player 2",
        controls = new PlayerControls
        {
            forward = KeyCode.UpArrow,
            backward = KeyCode.DownArrow,
            left = KeyCode.LeftArrow,
            right = KeyCode.RightArrow,
            boost = KeyCode.RightShift
        }
    };

    [Header("=== UI REFERENCES ===")]
    [SerializeField] private UIReferences ui = new UIReferences();

    [Header("=== FINISH LINE ===")]
    [SerializeField] private Transform finishLine;

    [Header("=== LOGO GAME OBJECTS ===")]
    [Tooltip("Logo positioned on the Left")]
    [SerializeField] private GameObject logoLeft;
    [Tooltip("Logo positioned in the Center")]
    [SerializeField] private GameObject logoCenter;
    [Tooltip("Logo positioned on the Right")]
    [SerializeField] private GameObject logoRight;

    [Header("=== WIN SCENE ===")]
    [Tooltip("Name of the scene to load after winner is declared")]
    [SerializeField] private string winSceneName = "WinScene";

    [Header("=== AUDIO ===")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip countdownClip;
    [SerializeField] private AudioClip winnerClip;

    #endregion

    #region ==================== GAME STATE ====================

    public enum GameState { Waiting, Countdown, Racing, Finished }
    private GameState currentState = GameState.Waiting;

    private bool isDebugMode = true;

    #endregion

    #region ==================== UNITY LIFECYCLE ====================

    private void Awake()
    {
        LoadSettingsFromPlayerPrefs();
        ValidateReferences();
        InitializePlayers();
        SetupPlayerCollisionIgnore();
        ApplyLogoPosition();
    }

    private void Start()
    {
        SetupUI();
        StartCoroutine(StartCountdown());
        if (ui.TotalDistanceText != null)
            ui.TotalDistanceText.text = $"{config.winningDistance}m";
    }

    private void Update()
    {
        if (currentState == GameState.Racing)
        {
            ReadPlayerInput(player1);
            ReadPlayerInput(player2);
            ProcessPlayerMovement(player1);
            ProcessPlayerMovement(player2);
            UpdatePlayerAnimation(player1);
            UpdatePlayerAnimation(player2);
            UpdateEnergy(player1);
            UpdateEnergy(player2);

            UpdateDistanceTracking();
            UpdateUI();
            CheckWinCondition();
        }
        else if (currentState == GameState.Countdown || currentState == GameState.Waiting)
        {
            SetAnimationState(player1, AnimState.Idle);
            SetAnimationState(player2, AnimState.Idle);
        }

        DebugInputCheck();
    }

    private void FixedUpdate()
    {
        if (currentState == GameState.Racing)
        {
            ApplyPhysics(player1);
            ApplyPhysics(player2);

            CheckTrackBoundary(player1);
            CheckTrackBoundary(player2);
        }
    }

    #endregion

    #region ==================== INITIALIZATION ====================

    private void LoadSettingsFromPlayerPrefs()
    {
        float savedWinningMeter = PlayerPrefs.GetFloat("WinningMeter", 100f);
        config.winningDistance = savedWinningMeter;

        string p1Name = PlayerPrefs.GetString("Player1Name", "Player 1");
        string p2Name = PlayerPrefs.GetString("Player2Name", "Player 2");
        player1.playerName = p1Name;
        player2.playerName = p2Name;

        DebugLog($"Loaded settings: Distance={savedWinningMeter}m, P1={p1Name}, P2={p2Name}");
    }
    private void ApplyLogoPosition()
    {
        int logoPosition = PlayerPrefs.GetInt("LogoPosition", 1);

        if (logoLeft != null) logoLeft.SetActive(false);
        if (logoCenter != null) logoCenter.SetActive(false);
        if (logoRight != null) logoRight.SetActive(false);

        switch (logoPosition)
        {
            case 0:
                if (logoLeft != null) logoLeft.SetActive(true);
                DebugLog("Logo: Left");
                break;
            case 1:
                if (logoCenter != null) logoCenter.SetActive(true);
                DebugLog("Logo: Center");
                break;
            case 2:
                if (logoRight != null) logoRight.SetActive(true);
                DebugLog("Logo: Right");
                break;
        }
    }

    private void ValidateReferences()
    {
        DebugLog("Validating references...");

        if (player1.cycleTransform == null)
            DebugLogError("Player 1 Cycle Transform is not assigned!");
        if (player1.cycleRigidbody == null)
            DebugLogError("Player 1 Rigidbody is not assigned!");
        if (player1.animation.characterAnimator == null)
            DebugLogWarning("Player 1 Animator is not assigned!");

        if (player2.cycleTransform == null)
            DebugLogError("Player 2 Cycle Transform is not assigned!");
        if (player2.cycleRigidbody == null)
            DebugLogError("Player 2 Rigidbody is not assigned!");
        if (player2.animation.characterAnimator == null)
            DebugLogWarning("Player 2 Animator is not assigned!");

        if (ui.winnerPanel == null)
            DebugLogWarning("Winner Panel is not assigned!");

        DebugLog("Reference validation complete.");
    }

    private void InitializePlayers()
    {
        DebugLog("Initializing players...");
        if (player1.cycleTransform != null)
            player1.lastPosition = player1.cycleTransform.position;
        if (player2.cycleTransform != null)
            player2.lastPosition = player2.cycleTransform.position;

        player1.currentEnergy = config.maxEnergy;
        player2.currentEnergy = config.maxEnergy;

        ResetPlayerState(player1);
        ResetPlayerState(player2);

        ConfigureRigidbody(player1.cycleRigidbody);
        ConfigureRigidbody(player2.cycleRigidbody);

        SetBicycleVehicleActive(player1, false);
        SetBicycleVehicleActive(player2, false);

        SetAnimationState(player1, AnimState.Idle);
        SetAnimationState(player2, AnimState.Idle);

        DebugLog("Players initialized successfully.");
    }

    private void ResetPlayerState(PlayerConfig player)
    {
        player.verticalInput = 0f;
        player.horizontalInput = 0f;
        player.boostInput = false;
        player.isMoving = false;
        player.currentSpeed = 0f;
        player.isBoosting = false;
    }

    private void SetupPlayerCollisionIgnore()
    {
        DebugLog("Setting up player collision ignore...");

        if (player1.cycleCollider == null && player1.cycleTransform != null)
            player1.cycleCollider = player1.cycleTransform.GetComponent<Collider>();
        if (player2.cycleCollider == null && player2.cycleTransform != null)
            player2.cycleCollider = player2.cycleTransform.GetComponent<Collider>();

        if (player1.cycleCollider != null && player2.cycleCollider != null)
        {
            Physics.IgnoreCollision(player1.cycleCollider, player2.cycleCollider, true);
        }

        if (player1.cycleTransform != null && player2.cycleTransform != null)
        {
            Collider[] p1Colliders = player1.cycleTransform.GetComponentsInChildren<Collider>();
            Collider[] p2Colliders = player2.cycleTransform.GetComponentsInChildren<Collider>();

            foreach (Collider c1 in p1Colliders)
            {
                foreach (Collider c2 in p2Colliders)
                {
                    Physics.IgnoreCollision(c1, c2, true);
                }
            }
            DebugLog($"Ignored collisions between {p1Colliders.Length} P1 and {p2Colliders.Length} P2 colliders.");
        }
    }

    private void ConfigureRigidbody(Rigidbody rb)
    {
        if (rb == null) return;

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.maxAngularVelocity = config.maxAngularVelocity;
        rb.centerOfMass = new Vector3(0f, -0.3f, 0f);

        DebugLog($"Configured Rigidbody: {rb.gameObject.name}");
    }

    private void SetupUI()
    {
        DebugLog("Setting up UI...");

        if (ui.player1NameText != null)
            ui.player1NameText.text = player1.playerName;
        if (ui.player2NameText != null)
            ui.player2NameText.text = player2.playerName;

        if (ui.player1EnergySlider != null)
        {
            ui.player1EnergySlider.minValue = 0f;
            ui.player1EnergySlider.maxValue = config.maxEnergy;
            ui.player1EnergySlider.value = config.maxEnergy;
        }
        if (ui.player2EnergySlider != null)
        {
            ui.player2EnergySlider.minValue = 0f;
            ui.player2EnergySlider.maxValue = config.maxEnergy;
            ui.player2EnergySlider.value = config.maxEnergy;
        }

        if (ui.winnerPanel != null)
            ui.winnerPanel.SetActive(false);

        InitializeCountdownLights();

        UpdateUI();
        DebugLog("UI setup complete.");
    }

    #endregion

    #region ==================== COUNTDOWN SYSTEM ====================

    private void InitializeCountdownLights()
    {
        CountdownLights lights = ui.countdownLights;
        if (lights == null) return;

        SetLightSprite(lights.light1, lights.redSprite);
        SetLightSprite(lights.light2, lights.redSprite);
        SetLightSprite(lights.light3, lights.redSprite);

        if (lights.light1 != null) lights.light1.SetActive(true);
        if (lights.light2 != null) lights.light2.SetActive(true);
        if (lights.light3 != null) lights.light3.SetActive(true);
    }

    private void SetLightSprite(GameObject lightObj, Sprite sprite)
    {
        if (lightObj == null || sprite == null) return;

        Image img = lightObj.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = sprite;
        }
    }

    private IEnumerator StartCountdown()
    {
        currentState = GameState.Countdown;
        DebugLog("Starting countdown...");

        FreezePlayer(player1, true);
        FreezePlayer(player2, true);

        CountdownLights lights = ui.countdownLights;
        yield return new WaitForSeconds(1f);

        if (lights != null)
        {
            SetLightSprite(lights.light1, lights.greenSprite);
            PlayAudio(countdownClip);
            DebugLog("Countdown: Light 1 GREEN");
        }
        yield return new WaitForSeconds(1f);

        if (lights != null)
        {
            SetLightSprite(lights.light2, lights.greenSprite);
            PlayAudio(countdownClip);
            DebugLog("Countdown: Light 2 GREEN");
        }
        yield return new WaitForSeconds(1f);
        if (lights != null)
        {
            SetLightSprite(lights.light3, lights.greenSprite);
            PlayAudio(countdownClip);
            DebugLog("Countdown: Light 3 GREEN - GO!");
        }

        DebugLog("GO! Race started!");

        currentState = GameState.Racing;

        FreezePlayer(player1, false);
        FreezePlayer(player2, false);

        SetBicycleVehicleActive(player1, true);
        SetBicycleVehicleActive(player2, true);

        yield return new WaitForSeconds(0.5f);
        if (lights != null)
        {
            if (lights.light1 != null) lights.light1.SetActive(false);
            if (lights.light2 != null) lights.light2.SetActive(false);
            if (lights.light3 != null) lights.light3.SetActive(false);
        }
    }

    private void FreezePlayer(PlayerConfig player, bool freeze)
    {
        if (player.cycleRigidbody == null) return;

        if (freeze)
        {
            player.cycleRigidbody.isKinematic = true;
        }
        else
        {
            player.cycleRigidbody.isKinematic = false;
            ConfigureRigidbody(player.cycleRigidbody);
        }

        DebugLog($"{player.playerName} frozen: {freeze}");
    }

    private void SetBicycleVehicleActive(PlayerConfig player, bool active)
    {
        if (player.bicycleVehicleScript != null)
        {
            player.bicycleVehicleScript.enabled = active;
            DebugLog($"{player.playerName} BicycleVehicle script: {(active ? "ENABLED" : "DISABLED")}");
        }
    }

    #endregion

    #region ==================== PLAYER INPUT ====================
    private void ReadPlayerInput(PlayerConfig player)
    {
        if (player.isFinished || player.cycleTransform == null)
        {
            ResetPlayerState(player);
            return;
        }

        player.verticalInput = 0f;
        player.horizontalInput = 0f;
        if (Input.GetKey(player.controls.forward))
        {
            player.verticalInput = 1f;
        }
        else if (Input.GetKey(player.controls.backward))
        {
            player.verticalInput = -0.5f;
        }

        if (Input.GetKey(player.controls.left))
        {
            player.horizontalInput -= 1f;
        }
        if (Input.GetKey(player.controls.right))
        {
            player.horizontalInput += 1f;
        }

        player.horizontalInput = Mathf.Clamp(player.horizontalInput, -1f, 1f);

        player.boostInput = Input.GetKey(player.controls.boost);

        player.isMoving = Input.GetKey(player.controls.forward) ||
                          Input.GetKey(player.controls.backward) ||
                          Input.GetKey(player.controls.left) ||
                          Input.GetKey(player.controls.right);
    }
    private void ProcessPlayerMovement(PlayerConfig player)
    {
        if (player.isFinished || player.cycleTransform == null) return;

        player.isBoosting = player.boostInput &&
                           player.currentEnergy >= config.minEnergyToBoost &&
                           player.verticalInput > 0;
        float speedMultiplier = player.isBoosting ? config.boostSpeedMultiplier : 1f;
        float targetSpeed = player.verticalInput * config.maxSpeed * speedMultiplier;

        if (player.verticalInput != 0)
        {
            float accelRate = player.isBoosting ? config.acceleration * 1.5f : config.acceleration;
            player.currentSpeed = Mathf.MoveTowards(player.currentSpeed, targetSpeed, accelRate * Time.deltaTime);
        }
        else
        {
            player.currentSpeed = Mathf.MoveTowards(player.currentSpeed, 0f, config.deceleration * Time.deltaTime);
        }
        if (Mathf.Abs(player.currentSpeed) > 0.1f)
        {
            float turnAmount = player.horizontalInput * config.turnSpeed * Time.deltaTime;
            turnAmount *= Mathf.Clamp01(Mathf.Abs(player.currentSpeed) / config.maxSpeed);
            player.cycleTransform.Rotate(0f, turnAmount, 0f);
        }
        float normalizedSpeed = Mathf.Abs(player.currentSpeed) / (config.maxSpeed * config.boostSpeedMultiplier);
        float targetLean = -player.horizontalInput * config.leanAngle * normalizedSpeed;
        player.currentLean = Mathf.Lerp(player.currentLean, targetLean, config.leanSmoothing * Time.deltaTime);

        ApplyVisualLean(player);
    }

    private void ApplyVisualLean(PlayerConfig player)
    {
        if (player.cycleTransform == null) return;

        Vector3 currentEuler = player.cycleTransform.localEulerAngles;
        player.cycleTransform.localEulerAngles = new Vector3(0f, currentEuler.y, player.currentLean);
    }

    private void DebugInputCheck()
    {
        if (!isDebugMode) return;
        if (Input.GetKeyDown(player1.controls.forward))
            DebugLog($"Player 1: {player1.controls.forward} PRESSED");
        if (Input.GetKeyDown(player1.controls.boost))
            DebugLog($"Player 1: {player1.controls.boost} (Boost) PRESSED");
        if (Input.GetKeyDown(player2.controls.forward))
            DebugLog($"Player 2: {player2.controls.forward} PRESSED");
        if (Input.GetKeyDown(player2.controls.boost))
            DebugLog($"Player 2: {player2.controls.boost} (Boost) PRESSED");
    }

    #endregion

    #region ==================== ANIMATION SYSTEM ====================
    private void UpdatePlayerAnimation(PlayerConfig player)
    {
        if (player.animation.characterAnimator == null) return;
        if (player.isFinished)
        {
            SetAnimationState(player, AnimState.Idle);
            return;
        }

        AnimState targetState;

        if (player.isBoosting)
        {
            targetState = AnimState.Speed;
        }
        else if (player.isMoving)
        {
            targetState = AnimState.Normal;
        }
        else
        {
            targetState = AnimState.Idle;
        }
        if (targetState != player.animation.currentState)
        {
            SetAnimationState(player, targetState);
        }
    }
    private void SetAnimationState(PlayerConfig player, AnimState newState)
    {
        if (player.animation.characterAnimator == null) return;

        Animator anim = player.animation.characterAnimator;

        if (!string.IsNullOrEmpty(player.animation.idleParam))
            anim.SetBool(player.animation.idleParam, false);
        if (!string.IsNullOrEmpty(player.animation.normalParam))
            anim.SetBool(player.animation.normalParam, false);
        if (!string.IsNullOrEmpty(player.animation.speedParam))
            anim.SetBool(player.animation.speedParam, false);

        switch (newState)
        {
            case AnimState.Idle:
                if (!string.IsNullOrEmpty(player.animation.idleParam))
                    anim.SetBool(player.animation.idleParam, true);
                break;

            case AnimState.Normal:
                if (!string.IsNullOrEmpty(player.animation.normalParam))
                    anim.SetBool(player.animation.normalParam, true);
                break;

            case AnimState.Speed:
                if (!string.IsNullOrEmpty(player.animation.speedParam))
                    anim.SetBool(player.animation.speedParam, true);
                break;
        }

        player.animation.currentState = newState;
        DebugLog($"{player.playerName} animation: {newState}");
    }

    #endregion

    #region ==================== ENERGY SYSTEM ====================

    private void UpdateEnergy(PlayerConfig player)
    {
        if (player.isFinished) return;

        if (player.isBoosting)
        {
            player.currentEnergy -= config.energyDrainRate * Time.deltaTime;
            player.currentEnergy = Mathf.Max(0f, player.currentEnergy);

            if (player.currentEnergy <= 0f)
            {
                player.isBoosting = false;
                DebugLog($"{player.playerName} energy depleted!");
            }
        }
        else
        {
            player.currentEnergy += config.energyRechargeRate * Time.deltaTime;
            player.currentEnergy = Mathf.Min(config.maxEnergy, player.currentEnergy);
        }
    }

    #endregion

    #region ==================== TRACK BOUNDARY ====================

    private void CheckTrackBoundary(PlayerConfig player)
    {
        if (player.cycleTransform == null || player.cycleRigidbody == null) return;
        if (config.trackBoundary == 0) return;

        Vector3 playerPos = player.cycleTransform.position;

        Vector3[] checkDirections = new Vector3[]
        {
            player.cycleTransform.right,
            -player.cycleTransform.right,
            player.cycleTransform.forward,
            -player.cycleTransform.forward
        };

        foreach (Vector3 dir in checkDirections)
        {
            RaycastHit hit;
            if (Physics.Raycast(playerPos, dir, out hit, config.boundaryCheckDistance, config.trackBoundary))
            {
                Vector3 pushDirection = -dir;
                float pushStrength = config.boundaryPushForce * (1f - (hit.distance / config.boundaryCheckDistance));
                player.cycleRigidbody.AddForce(pushDirection * pushStrength, ForceMode.Acceleration);
            }
        }
    }

    #endregion

    #region ==================== PHYSICS ====================

    private void ApplyPhysics(PlayerConfig player)
    {
        if (player.cycleRigidbody == null || player.cycleTransform == null) return;
        if (player.isFinished) return;

        Vector3 moveDirection = player.cycleTransform.forward * player.currentSpeed;

        float currentYVelocity = player.cycleRigidbody.velocity.y;
        currentYVelocity = Mathf.Clamp(currentYVelocity, -20f, 5f);
        moveDirection.y = currentYVelocity;

        player.cycleRigidbody.velocity = moveDirection;

        ApplyStabilization(player);
    }

    private void ApplyStabilization(PlayerConfig player)
    {
        if (player.cycleRigidbody == null || player.cycleTransform == null) return;

        Vector3 frontPos = player.cycleTransform.position + player.cycleTransform.forward * 0.5f;
        Vector3 backPos = player.cycleTransform.position - player.cycleTransform.forward * 0.5f;

        bool frontGrounded = Physics.Raycast(frontPos, Vector3.down, config.groundCheckDistance, config.groundLayer);
        bool backGrounded = Physics.Raycast(backPos, Vector3.down, config.groundCheckDistance, config.groundLayer);
        bool centerGrounded = Physics.Raycast(player.cycleTransform.position, Vector3.down, config.groundCheckDistance, config.groundLayer);

        if (backGrounded && !frontGrounded)
        {
            Vector3 frontDownForce = Vector3.down * config.antiWheelieForce;
            player.cycleRigidbody.AddForceAtPosition(frontDownForce, frontPos, ForceMode.Acceleration);
        }

        if (frontGrounded && !backGrounded)
        {
            Vector3 backDownForce = Vector3.down * config.antiWheelieForce;
            player.cycleRigidbody.AddForceAtPosition(backDownForce, backPos, ForceMode.Acceleration);
        }

        if (!frontGrounded && !backGrounded && !centerGrounded)
        {
            player.cycleRigidbody.AddForce(Vector3.down * config.stabilizationForce, ForceMode.Acceleration);
        }

        Vector3 currentRotation = player.cycleTransform.eulerAngles;
        float xRot = currentRotation.x;
        if (xRot > 180f) xRot -= 360f;

        if (Mathf.Abs(xRot) > 5f)
        {
            player.cycleTransform.eulerAngles = new Vector3(0f, currentRotation.y, currentRotation.z);
        }

        Vector3 angVel = player.cycleRigidbody.angularVelocity;
        angVel.x = Mathf.Clamp(angVel.x, -config.maxAngularVelocity, config.maxAngularVelocity);
        angVel.z = Mathf.Clamp(angVel.z, -config.maxAngularVelocity, config.maxAngularVelocity);
        player.cycleRigidbody.angularVelocity = angVel;
    }

    #endregion

    #region ==================== DISTANCE TRACKING ====================

    private void UpdateDistanceTracking()
    {
        TrackPlayerDistance(player1);
        TrackPlayerDistance(player2);
    }

    private void TrackPlayerDistance(PlayerConfig player)
    {
        if (player.cycleTransform == null || player.isFinished) return;

        Vector3 currentPos = player.cycleTransform.position;

        Vector3 horizontalCurrent = new Vector3(currentPos.x, 0, currentPos.z);
        Vector3 horizontalLast = new Vector3(player.lastPosition.x, 0, player.lastPosition.z);

        float distanceThisFrame = Vector3.Distance(horizontalCurrent, horizontalLast);

        if (player.currentSpeed > 0.1f)
        {
            player.distanceTraveled += distanceThisFrame;
        }

        player.lastPosition = currentPos;
    }

    #endregion

    #region ==================== UI UPDATE ====================

    private void UpdateUI()
    {
        // Player 1 UI
        if (ui.player1DistanceText != null)
            ui.player1DistanceText.text = $"{player1.distanceTraveled:F1}m";
        if (ui.player1SpeedText != null)
        {
            float speedKmh = Mathf.Abs(player1.currentSpeed * 3.6f);
            string boostIndicator = player1.isBoosting ? " [BOOST]" : "";
            ui.player1SpeedText.text = $"{speedKmh:F1} km/h{boostIndicator}";
        }
        if (ui.player1EnergySlider != null)
        {
            ui.player1EnergySlider.value = player1.currentEnergy;
            if (ui.player1EnergyFill != null)
                ui.player1EnergyFill.color = GetEnergyColor(player1.currentEnergy);
        }

        if (ui.player2DistanceText != null)
            ui.player2DistanceText.text = $"{player2.distanceTraveled:F1}m";
        if (ui.player2SpeedText != null)
        {
            float speedKmh = Mathf.Abs(player2.currentSpeed * 3.6f);
            string boostIndicator = player2.isBoosting ? " [BOOST]" : "";
            ui.player2SpeedText.text = $"{speedKmh:F1} km/h{boostIndicator}";
        }
        if (ui.player2EnergySlider != null)
        {
            ui.player2EnergySlider.value = player2.currentEnergy;
            if (ui.player2EnergyFill != null)
                ui.player2EnergyFill.color = GetEnergyColor(player2.currentEnergy);
        }
    }

    private Color GetEnergyColor(float energy)
    {
        float percentage = energy / config.maxEnergy;

        if (percentage > 0.5f)
            return Color.Lerp(Color.yellow, Color.green, (percentage - 0.5f) * 2f);
        else
            return Color.Lerp(Color.red, Color.yellow, percentage * 2f);
    }

    #endregion

    #region ==================== WIN CONDITION ====================

    private void CheckWinCondition()
    {
        if (!player1.isFinished && player1.distanceTraveled >= config.winningDistance)
        {
            DeclareWinner(player1);
        }
        else if (!player2.isFinished && player2.distanceTraveled >= config.winningDistance)
        {
            DeclareWinner(player2);
        }
    }

    private void DeclareWinner(PlayerConfig winner)
    {
        currentState = GameState.Finished;
        winner.isFinished = true;

        DebugLog($"WINNER: {winner.playerName}!");

        PlayAudio(winnerClip);

        StopPlayer(player1);
        StopPlayer(player2);

        PlayerPrefs.SetString("WinnerName", winner.playerName);
        PlayerPrefs.SetFloat("WinnerDistance", winner.distanceTraveled);
        PlayerPrefs.Save();

        SaveToHistory(winner.playerName, winner.distanceTraveled);

        if (ui.winnerPanel != null)
        {
            ui.winnerPanel.SetActive(true);
            if (ui.winnerText != null)
                ui.winnerText.text = $"{winner.playerName} WINS!";
        }

        StartCoroutine(GoToWinScene());
    }

    private void SaveToHistory(string playerName, float distance)
    {
        for (int i = 4; i >= 1; i--)
        {
            string prevName = PlayerPrefs.GetString($"HistoryName_{i}", "");
            float prevDist = PlayerPrefs.GetFloat($"HistoryDist_{i}", 0f);

            PlayerPrefs.SetString($"HistoryName_{i + 1}", prevName);
            PlayerPrefs.SetFloat($"HistoryDist_{i + 1}", prevDist);
        }
        PlayerPrefs.SetString("HistoryName_1", playerName);
        PlayerPrefs.SetFloat("HistoryDist_1", distance);
        PlayerPrefs.Save();

        DebugLog($"Saved to history: {playerName} - {distance:F1}m");
    }

    private void StopPlayer(PlayerConfig player)
    {
        player.currentSpeed = 0f;
        player.isBoosting = false;
        player.isMoving = false;
        ResetPlayerState(player);

        SetAnimationState(player, AnimState.Idle);

        if (player.cycleRigidbody != null)
        {
            player.cycleRigidbody.velocity = Vector3.zero;
            player.cycleRigidbody.angularVelocity = Vector3.zero;
        }

        SetBicycleVehicleActive(player, false);
        DebugLog($"{player.playerName} stopped.");
    }

    private IEnumerator GoToWinScene()
    {
        DebugLog($"Going to win scene in {config.winnerDisplayDelay} seconds...");
        yield return new WaitForSeconds(config.winnerDisplayDelay);

        if (!string.IsNullOrEmpty(winSceneName))
        {
            SceneManager.LoadScene(winSceneName);
        }
        else
        {
            DebugLogError("Win scene name not set!");
        }
    }

    #endregion

    #region ==================== FINISH LINE COLLISION ====================

    public void OnPlayerCrossedFinishLine(GameObject playerObject)
    {
        if (currentState != GameState.Racing) return;

        if (playerObject == player1.cycleTransform?.gameObject)
        {
            DebugLog("Player 1 crossed finish line!");
            DeclareWinner(player1);
        }
        else if (playerObject == player2.cycleTransform?.gameObject)
        {
            DebugLog("Player 2 crossed finish line!");
            DeclareWinner(player2);
        }
    }

    #endregion

    #region ==================== PUBLIC API ====================

    public void SetPlayerNames(string name1, string name2)
    {
        player1.playerName = string.IsNullOrEmpty(name1) ? "Player 1" : name1;
        player2.playerName = string.IsNullOrEmpty(name2) ? "Player 2" : name2;
        DebugLog($"Player names set: {player1.playerName}, {player2.playerName}");
    }

    public void SetWinningDistance(float distance)
    {
        config.winningDistance = Mathf.Clamp(distance, 50f, 500f);
        DebugLog($"Winning distance set to: {config.winningDistance}m");
    }

    public GameState GetCurrentState() => currentState;
    public float GetPlayerDistance(int playerNumber) => playerNumber == 1 ? player1.distanceTraveled : player2.distanceTraveled;
    public float GetPlayerSpeed(int playerNumber) => playerNumber == 1 ? player1.currentSpeed : player2.currentSpeed;
    public float GetPlayerEnergy(int playerNumber) => playerNumber == 1 ? player1.currentEnergy : player2.currentEnergy;

    public void SetPlayer1Controls(KeyCode forward, KeyCode backward, KeyCode left, KeyCode right, KeyCode boost)
    {
        player1.controls.forward = forward;
        player1.controls.backward = backward;
        player1.controls.left = left;
        player1.controls.right = right;
        player1.controls.boost = boost;
        DebugLog($"Player 1 controls updated");
    }
    public void SetPlayer2Controls(KeyCode forward, KeyCode backward, KeyCode left, KeyCode right, KeyCode boost)
    {
        player2.controls.forward = forward;
        player2.controls.backward = backward;
        player2.controls.left = left;
        player2.controls.right = right;
        player2.controls.boost = boost;
        DebugLog($"Player 2 controls updated");
    }

    #endregion

    #region ==================== AUDIO ====================

    private void PlayAudio(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    #endregion

    #region ==================== DEBUG LOGGING ====================

    private void DebugLog(string message)
    {
        if (isDebugMode)
            Debug.Log($"[CycleRace] {message}");
    }

    private void DebugLogWarning(string message)
    {
        if (isDebugMode)
            Debug.LogWarning($"[CycleRace] WARNING: {message}");
    }

    private void DebugLogError(string message)
    {
        Debug.LogError($"[CycleRace] ERROR: {message}");
    }

    #endregion

    #region ==================== GIZMOS ====================

    private void OnDrawGizmos()
    {
        if (finishLine != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(finishLine.position, new Vector3(10f, 3f, 1f));
        }

        if (player1.cycleTransform != null)
        {
            Vector3 p1Front = player1.cycleTransform.position + player1.cycleTransform.forward * 0.5f;
            Vector3 p1Back = player1.cycleTransform.position - player1.cycleTransform.forward * 0.5f;

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(player1.cycleTransform.position, player1.cycleTransform.position + Vector3.down * config.groundCheckDistance);
            Gizmos.DrawLine(p1Front, p1Front + Vector3.down * config.groundCheckDistance);
            Gizmos.DrawLine(p1Back, p1Back + Vector3.down * config.groundCheckDistance);
        }

        if (player2.cycleTransform != null)
        {
            Vector3 p2Front = player2.cycleTransform.position + player2.cycleTransform.forward * 0.5f;
            Vector3 p2Back = player2.cycleTransform.position - player2.cycleTransform.forward * 0.5f;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(player2.cycleTransform.position, player2.cycleTransform.position + Vector3.down * config.groundCheckDistance);
            Gizmos.DrawLine(p2Front, p2Front + Vector3.down * config.groundCheckDistance);
            Gizmos.DrawLine(p2Back, p2Back + Vector3.down * config.groundCheckDistance);
        }
    }

    #endregion
}
public class FinishLineTrigger : MonoBehaviour
{
    [SerializeField] private CycleRaceGameController gameController;

    private void OnTriggerEnter(Collider other)
    {
        if (gameController != null)
        {
            Debug.Log($"[FinishLine] {other.gameObject.name} crossed the finish line!");
            gameController.OnPlayerCrossedFinishLine(other.gameObject);
        }
    }
}