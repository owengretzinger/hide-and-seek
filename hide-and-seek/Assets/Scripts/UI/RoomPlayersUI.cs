using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class RoomPlayersUI : MonoBehaviourPunCallbacks
{
    public static RoomPlayersUI instance;

    public RectTransform roomPlayersUIParent;
    public GameObject playerUIprefab;
    public Sprite seekerSprite;

    private PlayerManager currentSeeker;

    // to reset the red colouring after a round
    private List<PlayerManager> deadPlayers = new List<PlayerManager>();

    // keeping track of the order of players in the top left so that the seeker is always top left
    [HideInInspector] public List<PlayerManager> playerOrder = new List<PlayerManager>();
    // references which players are tied to which UI elements
    private Dictionary<PlayerManager, GameObject> playersOnUI = new Dictionary<PlayerManager, GameObject>();
    
    private bool hasPlayerOrder;


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {

        if (!PhotonNetwork.IsConnected)
            return;

        if (!PhotonNetwork.IsMasterClient) photonView.RPC("SendPlayerOrder", RpcTarget.MasterClient);
    }


    [PunRPC]
    public void SendPlayerOrder()
    {
        List<int> playerIDOrder = new List<int>();
        for (int i = 0; i < playerOrder.Count; i++)
        {
            playerIDOrder.Add(playerOrder[i].gameObject.GetPhotonView().ViewID);
        }
        photonView.RPC("ReceivePlayerOrder", RpcTarget.All, playerIDOrder.ToArray());
    }

    [PunRPC]
    public void ReceivePlayerOrder(int[] viewIDs)
    {
        if (hasPlayerOrder)
            return;

        playerOrder = new List<PlayerManager>();
        foreach (int viewID in viewIDs)
        {
            PhotonView playerView = PhotonView.Find(viewID);
            PlayerManager player = playerView.GetComponent<PlayerManager>();
            playerOrder.Add(player);
        }

        hasPlayerOrder = true;

        StartCoroutine(SetPlayerOrder());
    }

    private IEnumerator SetPlayerOrder()
    {
        while (playersOnUI.Count < playerOrder.Count) yield return null;

        foreach (PlayerManager player in new List<PlayerManager>(playersOnUI.Keys))
        {
            playersOnUI[player].transform.SetSiblingIndex(playerOrder.IndexOf(player));

            AddPlayerToUIRoutine(player);
        }
    }


    [PunRPC]
    public void RotateUI()
    {
        PlayerManager player = playerOrder[0];
        playerOrder.Remove(player);
        playerOrder.Add(player);
        playersOnUI[player].transform.SetAsLastSibling();
    }

    public void AddPlayerToUI(PlayerManager player)
    {
        StartCoroutine(AddPlayerToUIRoutine(player));
    }
    private IEnumerator AddPlayerToUIRoutine(PlayerManager player)
    {
        while (player.playerObject == null) yield return null;

        GameObject element = Instantiate(playerUIprefab, roomPlayersUIParent);

        Image img = element.GetComponentInChildren<Image>();
        TMP_Text text = element.GetComponentInChildren<TMP_Text>();
        text.text = player.gameObject.GetPhotonView().Owner.NickName;
        playersOnUI.Add(player, element);

        if (player.playerObject.GetComponent<Hider>() != null)
        {
            img.sprite = player.hiderSkin.idle;
        }
        // we joined during a game and this is a seeker
        else
        {
            HiderTurnsIntoSeeker(player);
        }

        if (!playerOrder.Contains(player))
        {
            playerOrder.Add(player);
        }
    }


    public void HiderDied(Hider hider)
    {
        if (GameManager.instance.gamestate != GameManager.Gamestate.Ingame)
            return;

        // update minimap
        foreach (Hider h in FindObjectsOfType<Hider>())
        {
            if (h.pView.IsMine) StartCoroutine(h.UpdateMinimapPlayers());
        }

        PlayerManager player = GetPlayerFromHider(hider);

        Image img = playersOnUI[player].GetComponentInChildren<Image>();
        img.color = new Color(0.5f, 0f, 0f, 0.8f);

        deadPlayers.Add(player);
    }

    public void HiderTurnsIntoSeeker(PlayerManager player)
    {
        // change sprite to reaper
        Image img = playersOnUI[player].GetComponentInChildren<Image>();
        img.sprite = seekerSprite;
        // make it bigger and move up & left a bit
        RectTransform imgTrans = img.GetComponent<RectTransform>();
        imgTrans.sizeDelta = new Vector2(300f, 300f);
        imgTrans.anchoredPosition = new Vector2(-20f, 5f);

        currentSeeker = player;
    }

    public void SeekerTurnsIntoHider()
    {
        ResetDeadPlayers();
        RotateUI();
        StartCoroutine(SeekerTurnsIntoHiderRoutine());
    }
    private IEnumerator SeekerTurnsIntoHiderRoutine()
    {
        while (currentSeeker.playerObject == null) yield return null;

        // change sprite to their skin
        Image img = playersOnUI[currentSeeker].GetComponentInChildren<Image>();
        img.sprite = currentSeeker.hiderSkin.idle;

        // reset size
        RectTransform imgTrans = img.GetComponent<RectTransform>();
        imgTrans.sizeDelta = new Vector2(120f, 120f);
        imgTrans.anchoredPosition = new Vector2(0f, -60f);

        currentSeeker = null;
    }
    private void ResetDeadPlayers()
    {
        foreach (PlayerManager player in deadPlayers)
        {
            Image img = playersOnUI[player].GetComponentInChildren<Image>();
            img.color = new Color(1f, 1f, 1f);
        }

        deadPlayers.Clear();
    }


    private PlayerManager GetPlayerFromHider(Hider lookingFor)
    {
        PlayerManager hiderPlayer = null;
        foreach (PlayerManager player in FindObjectsOfType<PlayerManager>())
        {
            if (player.playerObject == null) continue; // someone joined during a round

            Hider playerHiderScript = player.playerObject.GetComponent<Hider>();

            if (playerHiderScript == lookingFor)
            {
                hiderPlayer = player;
            }
        }
        if (hiderPlayer == null)
        {
            Debug.LogError("Could not find the player manager for hider " + lookingFor.gameObject.GetPhotonView().Owner.NickName);
        }
        return hiderPlayer;
    }
}
