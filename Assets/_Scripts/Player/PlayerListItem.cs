using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Steamworks;
using Mirror;

public class PlayerListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private RawImage avatar;
    [SerializeField] private TMP_Dropdown options;

    [Header("Leader Icons")]
    public GameObject leaderIcon;

    [Header("Ready Text")]
    public Transform[] readyTextsPos;
    public TMP_Text readyText;

    public GameObject optionsDropdown;

    [HideInInspector] public bool avatarRecieved;
    [HideInInspector] public ulong steamId;
    [HideInInspector] public int playerIdNumber;
    [HideInInspector] public int connectionId;
    [HideInInspector] public string username;
    [HideInInspector] public bool ready;

    protected Callback<AvatarImageLoaded_t> avatarImageLoaded;

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
        avatarImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarImageLoaded);
    }

    public void SetPlayerListItemValues()
    {
        nameText.text = username;

        if (ready)
        {
            readyText.text = "Ready";
            readyText.color = Color.green;
        }
        else
        {
            readyText.text = "Unready";
            readyText.color = Color.red;
        }

        if (!avatarRecieved)
            GetPlayerAvatar();
    }

    public void OnSliderChanged()
    {
        if (playerIdNumber == GameManager.instance.localPlayerManager.playerIdNumber) { return; }

        switch (options.value)
        {
            case 0:
                AddFriend();
                break;
            case 1:
                if (GameManager.instance.localPlayerManager.leader)
                {
                    SteamMatchmaking.SetLobbyOwner((CSteamID)LobbyManager.instance.joinedLobbyID, (CSteamID)steamId);
                    foreach (PlayerManager playerManager in Manager.PlayerManagers)
                    {
                        playerManager.leader = false;
                        if ((CSteamID)playerManager.steamId == SteamMatchmaking.GetLobbyOwner((CSteamID)LobbyManager.instance.joinedLobbyID))
                        {
                            playerManager.leader = true;
                        }
                    }
                    GameManager.instance.UpdatePlayersAndListItems();
                }
                break;
            case 2:
                if (GameManager.instance.localPlayerManager.leader)
                {
                    GameManager.instance.AddKickPlayer(steamId);
                    GameManager.instance.UpdatePlayersAndListItems();
                }
                break;
        }
    }

    public void AddFriend()
    {
        SteamFriends.ActivateGameOverlayToUser("steamid", (CSteamID)steamId);
    }

    //Avatar
    private void GetPlayerAvatar()
    {
        int imageId = SteamFriends.GetLargeFriendAvatar((CSteamID)steamId);

        if (imageId == -1) { return; }

        avatar.texture = GetSteamImageAsTexture(imageId);
    }

    private void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
    {
        if (callback.m_steamID.m_SteamID == steamId)
        {
            avatar.texture = GetSteamImageAsTexture(callback.m_iImage);
        }
        else
        {
            return;
        }
    }

    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);

        if (isValid)
        {
            byte[] image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

            if (isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }

        avatarRecieved = true;

        return texture;
    }
}
