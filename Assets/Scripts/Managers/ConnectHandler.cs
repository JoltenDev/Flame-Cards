using Netcode.Transports;
using Steamworks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConnectHandler : ModifiedNetworkBehaviour
{
    public enum JoinType { id, code }

    [Header("Player Count")]
    [SerializeField] int maxPlayers = 4;

    [Header("Buttons")]
    [SerializeField] Button hostButton;
    [SerializeField] Button joinButton;
    [SerializeField] TMP_InputField joinInputField;

    protected Callback<LobbyCreated_t> lobbyCreatedCallback;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;

    const string hostKey = "HostAddress";

    void OnEnable()
    {
        hostButton.onClick.AddListener(delegate {
            HostLobby();

            hostButton.interactable = false;
        });

        joinButton.onClick.AddListener(delegate {
            //_ = JoinLobbyAsync(JoinType.code, new CSteamID(), joinButton, joinInputField.text);

            joinButton.interactable = false;
        });

        if (!SteamManager.Initialized)
        {
            NetworkLogger.LogError(this, "[NetworkLobbyManager] Steam not initialized!");
            return;
        }

        lobbyCreatedCallback = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequest);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    void OnDisable()
    {
        if (hostButton != null) hostButton.onClick.RemoveAllListeners();
        if (joinButton != null) joinButton.onClick.RemoveAllListeners();

        lobbyCreatedCallback?.Dispose();
        gameLobbyJoinRequested?.Dispose();
        lobbyEntered?.Dispose();
    }

    /// <summary>
    /// Initiates the creation of a Steam friends-only lobby with a maximum of 4 players,
    /// and disables the UI button to prevent repeated lobby creation attempts.
    /// </summary>
    public void HostLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePrivate, maxPlayers);
        NetworkLogger.LogDebug(this, "Attempting to create a connection", objectNameColor);
    }

    /// <summary>
    /// Handles the Steamworks callback after attempting to create a lobby.
    /// If creation is successful, starts the host, sets lobby metadata, and logs success.
    /// Otherwise, logs a warning and re-enables the UI button.
    /// </summary>
    /// <param name="callback">The callback data containing the result and the created lobby Steam ID.</param>
    void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            NetworkLogger.LogError(this, "[<color=#7DC2FF>NetworkLobbyManager</color>] Failed to create lobby");

            hostButton.interactable = true;
            return;
        }

        NetworkLogger.LogDebug(this, "Connection successfully created", objectNameColor);

        CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        SteamMatchmaking.SetLobbyData(lobbyId, hostKey, SteamUser.GetSteamID().ToString());
        //_ = SendLobbyDataAsync(lobbyId);

        // Set Lobby Name
        SteamMatchmaking.SetLobbyData(lobbyId, "name", SteamFriends.GetPersonaName() + "'s Lobby");

        // Set Ping Location
        SteamNetworkingUtils.GetLocalPingLocation(out SteamNetworkPingLocation_t myLocation);
        SteamNetworkingUtils.ConvertPingLocationToString(ref myLocation, out string locationString, 1024);
        SteamMatchmaking.SetLobbyData(lobbyId, "host_location", locationString);

        NetworkManager.Singleton.StartHost();
        //NetworkSceneLoader.Instance.LoadScene(NetworkSceneLoader.SceneType.Menu, NetworkSceneLoader.SceneType.Lobby);'
        NetworkManager.Singleton.SceneManager.LoadScene("Game (Scene)", LoadSceneMode.Single);
    }

    /// <summary>
    /// Handles the Steamworks callback when a join request to a lobby is received from an invitation or friends.
    /// </summary>
    /// <param name="callback">The callback data containing the Steam ID of the lobby to join.</param>
    void OnLobbyJoinRequest(GameLobbyJoinRequested_t callback)
    {
        //_ = JoinLobbyAsync(JoinType.id, callback.m_steamIDLobby);
        NetworkLogger.LogDebug(this, "Attempting to connect to a connection", objectNameColor);
    }

    /// <summary>
    /// Handles the Steamworks callback when a lobby is successfully entered.
    /// Extracts the host Steam ID from lobby metadata and initiates connection setup.
    /// </summary>
    /// <param name="callback">The callback data containing the entered lobby's Steam ID.</param>
    void OnLobbyEntered(LobbyEnter_t callback)
    {
        CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        string hostSteamIdStr = SteamMatchmaking.GetLobbyData(lobbyId, hostKey);

        if (!ulong.TryParse(hostSteamIdStr, out ulong hostSteamIdULong))
        {
            NetworkLogger.LogError(this, "[NetworkLobbyManager] Failed to parse host Steam ID from lobby metadata.");
            return;
        }

        NetworkLogger.LogDebug(this, "Connection successfully entered", objectNameColor);

        var steamTransport = (SteamNetworkingSocketsTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        steamTransport.ConnectToSteamID = hostSteamIdULong;

        // Check Join State
        string joinable = SteamMatchmaking.GetLobbyData(lobbyId, "joinable");

        int currentPlayers = SteamMatchmaking.GetNumLobbyMembers(lobbyId);
        int maxPlayers = SteamMatchmaking.GetLobbyMemberLimit(lobbyId);

        if (currentPlayers > maxPlayers)
        {
            SteamMatchmaking.LeaveLobby(lobbyId);
            NetworkLogger.LogError(this, "Failed to join lobby:\nLobby is full.");
            return;
        }

        if (!NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.StartClient();
        }
    }

    /*
    /// <summary>
    /// Asynchronously creates the lobby data in Firebase and starts the network client if successful.
    /// </summary>
    /// <param name="lobbyId">The Steam lobby ID to register in Firebase.</param>
    /// <returns>
    /// A <see cref="Task{Boolean}"/> that resolves to <c>true</c> if lobby creation and client start were successful; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method should only be called after verifying and assigning the host's Steam ID in the transport.
    /// </remarks>
    async Task<bool> SendLobbyDataAsync(CSteamID lobbyId)
    {
        try
        {
            bool success = await FirebaseManager.Instance.CreateLobbyDataAsync(lobbyId);

            if (!success)
            {
                Debug.LogError("[NetworkLobbyManager] Failed to create lobby data in Firebase.");
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[NetworkLobbyManager] Exception during OnLobbyEntered: {e.Message}");
            return false;
        }
    }

    public async Task JoinLobbyAsync(JoinType type, CSteamID lobbyId = new CSteamID(), Button buttonUsed = null, string code = "NA")
    {
        if (!NetworkEnvironment.UseSteam)
        {
            Debug.Log("[NetworkLobbyManager] Joining LOCAL lobby");
            NetworkManager.Singleton.StartClient();
            return;
        }

        if (type == JoinType.id)
            Debug.LogWarning("[<color=#7DC2FF>NetworkLobbyManager</color>] Attempting to join lobby through lobby id...");

        if (type == JoinType.code)
        {
            lobbyId = await FirebaseManager.Instance.RetrieveCodeAsync(code);

            if (lobbyId.IsValid())
            {
                Debug.LogWarning($"[<color=#7DC2FF>NetworkLobbyManager</color>] Successfully retrieved lobby code [{lobbyId}], attempting to join lobby...");
            }
            else
            {
                Debug.LogWarning($"[<color=#7DC2FF>NetworkLobbyManager</color>] Invalid or missing lobby for code: {code}");

                failureMessage.MessageFailure("Failed to join lobby:\nInvalid Lobby Code.");
                buttonUsed.interactable = true;
                return;
            }
        }

        SteamMatchmaking.JoinLobby(lobbyId);
    }
    */
}
