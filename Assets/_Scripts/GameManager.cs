using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Mirror;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("References")]
    [SerializeField] private GameObject playerListItemPrefab;
    [SerializeField] private TMP_Text lobbyNameText;
    [SerializeField] private Transform content;

    private PlayerManager localPlayerManager;

    private bool PlayerItemsCreated;
    private List<PlayerListItem> playerListItems = new();

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
        if (instance == null) { instance = this; }
    }

    public void FindLocalPlayerManager()
    {
        localPlayerManager = GameObject.Find("LocalGamePlayer").GetComponent<PlayerManager>();
    }

    //QuitLobby
    public void LeaveLobby()
    {
        localPlayerManager.LeaveLobby();
    }

    //Update Lobby Data
    public void UpdateLobbyName()
    {
        lobbyNameText.text = SteamMatchmaking.GetLobbyData((CSteamID)LobbyManager.instance.joinedLobbyID, "name");
    }

    //Update PlayerListITems
    public void UpdatePlayerListItems()
    {
        if (!PlayerItemsCreated) { CreateListItems(); }
        if (playerListItems.Count < Manager.PlayerManagers.Count) { CreateNewListItems(); }
        if (playerListItems.Count > Manager.PlayerManagers.Count) { RemoveListItems(); }
        if (playerListItems.Count == Manager.PlayerManagers.Count) { UpdateListItems(); }
    }

    //Player List Items
    private void CreateListItems()
    {
        foreach (PlayerManager playerManager in Manager.PlayerManagers)
        {
            GameObject playerListItem = Instantiate(playerListItemPrefab);
            PlayerListItem playerListItemScript = playerListItem.GetComponent<PlayerListItem>();


            //PlayerListItemScript
            if (playerManager.leader)
                playerListItemScript.leaderIcon.SetActive(true);

            playerListItemScript.username = playerManager.username;
            playerListItemScript.connectionID = playerManager.connectionId;
            playerListItemScript.steamId = playerManager.steamId;
            playerListItemScript.SetPlayerListItemValues();

            //PlayerListItem
            playerListItem.transform.SetParent(content);
            playerListItem.transform.localScale = Vector3.one;

            playerListItems.Add(playerListItemScript);
        }

        PlayerItemsCreated = true;
    }

    private void CreateNewListItems()
    {
        foreach (PlayerManager playerManager in Manager.PlayerManagers)
        {
            if (!playerListItems.Any(b => b.connectionID == playerManager.connectionId))
            {
                GameObject playerListItem = Instantiate(playerListItemPrefab);
                PlayerListItem playerListItemScript = playerListItem.GetComponent<PlayerListItem>();

                //PlayerListItemScript
                if (playerManager.leader)
                    playerListItemScript.leaderIcon.SetActive(true);

                playerListItemScript.username = playerManager.username;
                playerListItemScript.connectionID = playerManager.connectionId;
                playerListItemScript.steamId = playerManager.steamId;
                playerListItemScript.SetPlayerListItemValues();

                //PlayerListItem
                playerListItem.transform.SetParent(content);
                playerListItem.transform.localScale = Vector3.one;

                playerListItems.Add(playerListItemScript);
            }
        }
    }

    private void UpdateListItems()
    {
        foreach (PlayerManager playerManager in Manager.PlayerManagers)
        {
            foreach (PlayerListItem playerListItemScript in playerListItems)
            {
                if (playerListItemScript.connectionID == playerManager.connectionId)
                {
                    //PlayerManager
                    playerManager.usernameText.text = playerManager.username;

                    //PlayerListItemScript
                    if (playerManager.leader)
                        playerListItemScript.leaderIcon.SetActive(true);

                    if (playerManager.isOwned || SteamFriends.GetFriendRelationship((CSteamID)playerManager.steamId) == EFriendRelationship.k_EFriendRelationshipFriend)
                    {
                        playerListItemScript.addFriendButton.SetActive(false);
                        playerListItemScript.leaderIcon.transform.position = playerListItemScript.leaderIconOld.position;
                    }
                    else
                    {
                        playerListItemScript.addFriendButton.SetActive(true);
                        playerListItemScript.leaderIcon.transform.position = playerListItemScript.leaderIconNew.position;
                    }

                    playerListItemScript.username = playerManager.username;
                    playerListItemScript.SetPlayerListItemValues();
                }
            }
        }
    }

    private void RemoveListItems()
    {
        List<PlayerListItem> playerListItemsToRemove = new();

        foreach (PlayerListItem playerListItem in playerListItems)
        {
            if (!Manager.PlayerManagers.Any(b => b.connectionId == playerListItem.connectionID))
            {
                playerListItemsToRemove.Add(playerListItem);
            }
        }

        foreach (PlayerListItem playerListItemToRemove in playerListItemsToRemove)
        {
            playerListItems.Remove(playerListItemToRemove);
            Destroy(playerListItemToRemove.gameObject);
        }
    }

    public void DestroyPlayerListItems()
    {
        foreach (PlayerListItem playerListItem in playerListItems)
        {
            Destroy(playerListItem.gameObject);
        }
        playerListItems.Clear();
    }
}