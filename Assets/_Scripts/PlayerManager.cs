
using UnityEngine;
using Steamworks;
using Mirror;
using UnityEngine.SceneManagement;
using TMPro;


public class PlayerManager : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject serverSide;
    [SerializeField] private GameObject clientSide;
    public Transform userInfoCanvas;
    public TMP_Text usernameText;

    //Player Info (static)
    [SyncVar] public ulong steamId;
    [SyncVar] public int playerIdNumber;
    [SyncVar] public int connectionId;
    [SyncVar] public bool leader;

    //Player Info (updated)
    [SyncVar(hook = nameof(PlayerNameUpdate))] public string username;

    private CustomNetworkManager manager;

    private CustomNetworkManager Manager
    {
        get
        {
            if (manager != null)
            {
                return manager;
            }
            return manager = NetworkManager.singleton as CustomNetworkManager;
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public override void OnStartAuthority()
    {
        CmdSetPlayerName(SteamFriends.GetPersonaName().ToString());

        gameObject.name = "LocalGamePlayer";

        GameManager.instance.FindLocalPlayerManager();

        clientSide.SetActive(true);
        serverSide.SetActive(false);
    }

    public override void OnStartClient()
    {
        Manager.PlayerManagers.Add(this);

        GameManager.instance.UpdateLobbyName();

        GameManager.instance.UpdatePlayerListItems();
    }

    public void LeaveLobby()
    {
        if (isOwned)
        {
            if (leader)
            {
                SteamMatchmaking.SetLobbyData((CSteamID)LobbyManager.instance.joinedLobbyID, "active", "false");
            }

            GameManager.instance.DestroyPlayerListItems();

            SteamMatchmaking.LeaveLobby((CSteamID)LobbyManager.instance.joinedLobbyID);

            if (isServer)
            {
                Manager.StopHost();
            }
            if (isClient)
            {
                Manager.StopClient();
            }
        }
    }

    public override void OnStopClient()
    {
        Debug.Log(username + " is quiting the game.");

        Manager.PlayerManagers.Remove(this);

        GameManager.instance.UpdatePlayerListItems();
    }

    [Command]
    private void CmdSetPlayerName(string username)
    {
        PlayerNameUpdate(this.username, username);
    }

    public void PlayerNameUpdate(string OldValue, string NewValue)
    {
        if (isServer) //Host
        {
            username = NewValue;
        }
        if (isClient) //Client
        {
            GameManager.instance.UpdatePlayerListItems();
        }
    }
}