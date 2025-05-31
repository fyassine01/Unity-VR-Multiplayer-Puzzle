using UnityEngine;
using TMPro; // For TextMeshProUGUI
using Photon.Pun;
using Photon.Realtime; // For Player type

public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public static GameManager Instance { get; private set; }
    private PhotonView pv;

    [Header("Timer Settings")]
    public float countdownDuration = 120f; // e.g., 2 minutes
    private float currentTime;
    private bool timerIsRunning = false; // Controls if timer counts down

    public enum GamePhase { WaitingForPlayers, Playing, GameOver }
    private GamePhase currentPhase = GamePhase.WaitingForPlayers;

    public int requiredPlayers = 2;
    private int playersReadyInScene = 0; // MasterClient tracks this

    [Header("Cube Tracking")]
    public int totalCubesToPlace = 0; // Set by CubeSpawner on MasterClient, synced to others
    private int cubesCorrectlyPlaced = 0; // Managed by MasterClient, synced for UI

    [Header("UI Elements")]
    public TextMeshProUGUI timerText;
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject waitingForPlayersPanel;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Optional, but usually not needed if Launcher loads scenes
        }
        else
        {
            Destroy(gameObject);
        }
        pv = GetComponent<PhotonView>();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        currentTime = countdownDuration;
        UpdateTimerDisplay(currentTime);
        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);
        currentPhase = GamePhase.WaitingForPlayers; // Explicitly set
        UpdateWaitingPanel(); // Update UI based on initial phase
    }

    void Update()
    {
        // --- Game Start Logic (Master Client Only) ---
        if (PhotonNetwork.IsMasterClient && currentPhase == GamePhase.WaitingForPlayers)
        {
            if (playersReadyInScene >= requiredPlayers && totalCubesToPlace > 0)
            {
                Debug.Log("GameManager (MasterClient): Conditions met to start game. Sending RPC_StartGame.");
                // Pass initialTotalCubes from MasterClient's knowledge
                pv.RPC("RPC_StartGame", RpcTarget.All, countdownDuration, totalCubesToPlace);
            }
        }

        // --- Timer Logic (During Playing Phase) ---
        if (currentPhase == GamePhase.Playing && timerIsRunning)
        {
            if (PhotonNetwork.IsMasterClient) // MasterClient is authoritative for the timer
            {
                currentTime -= Time.deltaTime;
                if (currentTime <= 0)
                {
                    currentTime = 0;
                    // timerIsRunning will be set to false in RPC_HandleGameEnd
                    Debug.Log("GameManager (MasterClient): Time's up! Sending RPC_HandleGameEnd(false).");
                    pv.RPC("RPC_HandleGameEnd", RpcTarget.All, false); // Time's up, players lose
                }
            }
            // All clients update their timer display based on synced currentTime
            UpdateTimerDisplay(currentTime);
        }
    }

    void UpdateTimerDisplay(float timeToDisplay)
    {
        if (timerText != null)
        {
            if (timeToDisplay < 0) timeToDisplay = 0;
            int minutes = Mathf.FloorToInt(timeToDisplay / 60F);
            int seconds = Mathf.FloorToInt(timeToDisplay % 60F); // Use % for remainder
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        else
        {
            // Debug.LogWarning("GameManager: TimerText UI element is not assigned!");
        }
    }

    void UpdateWaitingPanel()
    {
        if (waitingForPlayersPanel != null)
        {
            bool shouldBeActive = (currentPhase == GamePhase.WaitingForPlayers && PhotonNetwork.CurrentRoom.PlayerCount < requiredPlayers);
            if (waitingForPlayersPanel.activeSelf != shouldBeActive)
            {
                waitingForPlayersPanel.SetActive(shouldBeActive);
            }
        }
    }

    // Called by player scripts (FPSController for Mover, similar for Guider) via RPC to MasterClient
    [PunRPC]
    void RPC_PlayerIsReadyInScene(PhotonMessageInfo info)
    {
        if (PhotonNetwork.IsMasterClient && currentPhase == GamePhase.WaitingForPlayers)
        {
            // Could add a check to ensure each player only calls this once if needed
            playersReadyInScene++;
            Debug.Log($"GameManager (MasterClient): Player '{info.Sender.NickName}' reported ready. Total ready: {playersReadyInScene}/{requiredPlayers}");
            // The check to start the game is in Update()
        }
    }

    // Called by CubeSpawner (MasterClient only) after it knows the total number of cubes
    public void Master_RegisterTotalCubes(int count)
    {
        if (PhotonNetwork.IsMasterClient && currentPhase == GamePhase.WaitingForPlayers)
        {
            totalCubesToPlace = count;
            cubesCorrectlyPlaced = 0; // Reset counter
            Debug.Log($"GameManager (MasterClient): Total cubes to place registered: {totalCubesToPlace}");
            // The check to start the game is in Update()
        }
    }

    [PunRPC]
    void RPC_StartGame(float startTime, int initialTotalCubes)
    {
        if (currentPhase == GamePhase.GameOver)
        {
            Debug.LogWarning($"RPC_StartGame received by {PhotonNetwork.LocalPlayer.NickName}, but game is already over. Ignoring.");
            return; // Do not restart if game was already over
        }
        if (currentPhase == GamePhase.Playing && timerIsRunning)
        {
            Debug.LogWarning($"RPC_StartGame received by {PhotonNetwork.LocalPlayer.NickName}, but game is already playing. Ignoring duplicate.");
            return;
        }

        Debug.Log($"RPC_StartGame received by {PhotonNetwork.LocalPlayer.NickName}. Phase changing to Playing. StartTime: {startTime}, TotalCubes: {initialTotalCubes}");
        currentPhase = GamePhase.Playing;
        timerIsRunning = true;
        currentTime = startTime;
        totalCubesToPlace = initialTotalCubes; // All clients get the correct total
        cubesCorrectlyPlaced = 0;       // Reset for all clients

        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);
        UpdateWaitingPanel(); // Should hide it now
        UpdateTimerDisplay(currentTime);

        // Ensure player can move and cursor is locked (for Mover)
        FPSController localFpsController = FindLocalFPSController();
        if (localFpsController != null)
        {
            localFpsController.canMove = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Called by CategoryZone (MasterClient executes the placement logic) via RPC to MasterClient
    [PunRPC]
    void RPC_Master_CubePlacedCorrectly(PhotonMessageInfo info)
    {
        if (PhotonNetwork.IsMasterClient && currentPhase == GamePhase.Playing && timerIsRunning)
        {
            cubesCorrectlyPlaced++;
            Debug.Log($"GameManager (MasterClient): Cube placed correctly! Count: {cubesCorrectlyPlaced}/{totalCubesToPlace}");

            // This value (cubesCorrectlyPlaced) will be synced via OnPhotonSerializeView

            if (cubesCorrectlyPlaced >= totalCubesToPlace && totalCubesToPlace > 0) // Ensure totalCubes is positive
            {
                Debug.Log("GameManager (MasterClient): All cubes placed! Sending RPC_HandleGameEnd(true).");
                // timerIsRunning will be set to false in RPC_HandleGameEnd
                pv.RPC("RPC_HandleGameEnd", RpcTarget.All, true); // All cubes placed, players win
            }
        }
    }

    [PunRPC]
    void RPC_HandleGameEnd(bool playerWon)
    {
        if (currentPhase == GamePhase.GameOver)
        {
            Debug.LogWarning($"RPC_HandleGameEnd ({playerWon}) received by {PhotonNetwork.LocalPlayer.NickName}, but game is already in GameOver phase. Ignoring duplicate call.");
            return; // Prevent multiple end-game processing
        }

        Debug.Log($"RPC_HandleGameEnd ({playerWon}) received by {PhotonNetwork.LocalPlayer.NickName}. Phase changing to GameOver.");
        currentPhase = GamePhase.GameOver;
        timerIsRunning = false; // CRITICAL: Stop the timer countdown logic

        // --- UI Updates ---
        if (playerWon)
        {
            if (winPanel) winPanel.SetActive(true); else Debug.LogError("WinPanel is null or not assigned!");
            if (losePanel) losePanel.SetActive(false);
            if (timerText) timerText.text = "YOU WIN!";
            Debug.Log($"GameManager ({PhotonNetwork.LocalPlayer.NickName}): Displaying WIN panel.");
        }
        else
        {
            if (losePanel) losePanel.SetActive(true); else Debug.LogError("LosePanel is null or not assigned!");
            if (winPanel) winPanel.SetActive(false);
            if (timerText) timerText.text = "TIME'S UP!"; // Or "YOU LOSE!"
            Debug.Log($"GameManager ({PhotonNetwork.LocalPlayer.NickName}): Displaying LOSE panel.");
        }
        if (waitingForPlayersPanel) waitingForPlayersPanel.SetActive(false);

        // --- Disable Player Input & Show Cursor ---
        FPSController localFpsController = FindLocalFPSController();
        if (localFpsController != null)
        {
            Debug.Log($"GameManager ({PhotonNetwork.LocalPlayer.NickName}): Disabling movement for local FPSController.");
            localFpsController.canMove = false;
        }
        // else if (IsGuiderAndHasController()) { /* Disable Guider controls */ }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log($"GameManager ({PhotonNetwork.LocalPlayer.NickName}): Cursor unlocked and made visible.");
    }

    private FPSController FindLocalFPSController()
    {
        FPSController[] allControllers = FindObjectsOfType<FPSController>(); // This can be slow if many objects
        foreach (FPSController controller in allControllers)
        {
            if (controller.photonView != null && controller.photonView.IsMine)
            {
                return controller;
            }
        }
        return null;
    }

    public GamePhase GetCurrentPhase() // For other scripts to query state if needed
    {
        return currentPhase;
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // MasterClient (or designated authoritative source) writes data
        {
            stream.SendNext(currentTime);
            stream.SendNext(timerIsRunning); // Useful for clients to know if timer logic should apply locally
            stream.SendNext((int)currentPhase);
            stream.SendNext(cubesCorrectlyPlaced);
            stream.SendNext(totalCubesToPlace); // Sync this so clients know the goal
        }
        else // Receiving clients read data
        {
            this.currentTime = (float)stream.ReceiveNext();
            this.timerIsRunning = (bool)stream.ReceiveNext(); // Update local timerIsRunning
            GamePhase newPhase = (GamePhase)(int)stream.ReceiveNext();
            this.cubesCorrectlyPlaced = (int)stream.ReceiveNext();
            this.totalCubesToPlace = (int)stream.ReceiveNext(); // Update local totalCubesToPlace

            // If this client's phase is not GameOver, but the synced phase IS GameOver,
            // it might have missed the RPC_HandleGameEnd. Force local end-game state.
            if (this.currentPhase != GamePhase.GameOver && newPhase == GamePhase.GameOver)
            {
                Debug.LogWarning($"GameManager ({PhotonNetwork.LocalPlayer.NickName}): Game phase synced to GameOver. Forcing local end game UI/state.");
                bool likelyWon = (this.cubesCorrectlyPlaced >= this.totalCubesToPlace && this.totalCubesToPlace > 0);
                // Call parts of RPC_HandleGameEnd locally, or a dedicated local update function
                // Forcing the full RPC_HandleGameEnd locally might be too much if it sends more RPCs.
                // Simplified local handling:
                this.currentPhase = GamePhase.GameOver; // Set local phase
                this.timerIsRunning = false;          // Stop local timer
                if (likelyWon)
                {
                    if (winPanel) winPanel.SetActive(true); if (losePanel) losePanel.SetActive(false);
                    if (timerText) timerText.text = "YOU WIN!";
                }
                else
                {
                    if (losePanel) losePanel.SetActive(true); if (winPanel) winPanel.SetActive(false);
                    if (timerText) timerText.text = "TIME'S UP!";
                }
                FPSController localFps = FindLocalFPSController();
                if (localFps) localFps.canMove = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                this.currentPhase = newPhase; // Update local phase normally
            }


            UpdateTimerDisplay(this.currentTime); // Always update display with synced time
            UpdateWaitingPanel(); // Update waiting panel based on new phase
        }
    }

    // Photon Callbacks
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.NickName} entered room. Total players: {PhotonNetwork.CurrentRoom.PlayerCount}");
        UpdateWaitingPanel();
        // If MasterClient and game is in progress, could send current game state to new player via RPC.
        // For now, OnPhotonSerializeView will catch them up.
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.NickName} left room. Total players: {PhotonNetwork.CurrentRoom.PlayerCount}");
        if (currentPhase == GamePhase.Playing)
        {
            // Only MasterClient should decide to end the game
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.LogWarning("A player left mid-game. Ending game as a loss (MasterClient decision).");
                pv.RPC("RPC_HandleGameEnd", RpcTarget.All, false);
            }
        }
        else if (currentPhase == GamePhase.WaitingForPlayers)
        {
            UpdateWaitingPanel();
            if (PhotonNetwork.IsMasterClient)
            {
                // Potentially decrement playersReadyInScene if the leaving player had signaled ready.
                // This requires more complex tracking of who is ready.
                // A simpler approach is to let the game start logic in Update() re-evaluate based on current room count.
                // playersReadyInScene = Mathf.Clamp(PhotonNetwork.CurrentRoom.PlayerCount -1, 0 , requiredPlayers) // Example logic for re-evaluating based on current room count. Requires more nuance.
            }
        }
    }
}