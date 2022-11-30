using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [HideInInspector] public PhotonView pView;

    public GameObject hiderPrefab;
    public GameObject[] seekerPrefabs;

    public GameObject chooseSeekerUIPrefab;
    private GameObject chooseSeekerUI;

    [HideInInspector] public GameObject playerObject;

    [HideInInspector] public HiderSkin hiderSkin;

    //[HideInInspector] public bool isNextSeeker;


    private void Awake()
    {
        pView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (pView.IsMine)
        {
            pView.RPC("SetHiderSkin", RpcTarget.AllBuffered, PlayerPrefs.GetInt("PlayerSkin"));
        }

        Spawn();
        RoomPlayersUI.instance.AddPlayerToUI(this);
    }

    [PunRPC]
    protected void SetHiderSkin(int index)
    {
        hiderSkin = PlayerSkinsDDOL.instance.skins[index];
    }

    public void Spawn()
    {
        if (pView.IsMine)
        {
            pView.RPC("RequestSpawn", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }
    [PunRPC]
    private void SyncPlayerObject(int viewID)
    {
        PhotonView playerView = PhotonView.Find(viewID);
        // we joined late and this was from a previous buffer
        if (playerView == null)
        {
            return;
        }
        playerObject = playerView.gameObject;

        //if (!hasBeenAddedToUI)
        //{
        //    RoomPlayersUI.instance.AddPlayerToUI(this);
        //    hasBeenAddedToUI = true;
        //}
    }

    [PunRPC]
    private void RequestSpawn(int number)
    {
        StartCoroutine(RequestSpawnWaitForPhotonView(number));
    }
    private IEnumerator RequestSpawnWaitForPhotonView(int number)
    {
        while (pView == null)
        {
            yield return null;
        }

        if (GameManager.instance.gamestate == GameManager.Gamestate.Pregame)
        {
            pView.RPC("SpawnAfterConfirmation", RpcTarget.All, number);
        }
        else if (GameManager.instance.gamestate == GameManager.Gamestate.Aftergame)
        {
            pView.RPC("Spectate", RpcTarget.All, number, true);
        }
        else
        {
            pView.RPC("Spectate", RpcTarget.All, number, false);
        }
    }

    [PunRPC]
    private void SpawnAfterConfirmation(int number)
    {
        if (pView == null) return;

        if (pView.IsMine && PhotonNetwork.LocalPlayer.ActorNumber == number)
        {
            playerObject = PhotonNetwork.Instantiate(hiderPrefab.name, Random.insideUnitCircle * 2f, Quaternion.identity);
            pView.RPC("SyncPlayerObject", RpcTarget.OthersBuffered, playerObject.GetPhotonView().ViewID);
        }
    }
    [PunRPC]
    private void Spectate(int number, bool afterGame)
    {
        if (pView.IsMine && PhotonNetwork.LocalPlayer.ActorNumber == number)
        {
            //RoomPlayersUI.instance.UpdatePlayerUI(true);

            

            if (afterGame)
            {
                GameUI.instance.waitingForPlayersText.text = "The game is just ending now";
                GameManager.instance.gamestate = GameManager.Gamestate.Aftergame;
            }
            else
            {
                GameUI.instance.waitingForPlayersText.text = "Spectating until this game is over";
                GameManager.instance.gamestate = GameManager.Gamestate.Ingame;
                StartCoroutine(WaitForSpectate());
            }
        }
    }

    public IEnumerator WaitForSpectate()
    {
        Seeker seeker = FindObjectOfType<Seeker>();
        while (seeker == null)
        {
            seeker = FindObjectOfType<Seeker>();
            Debug.Log("waiting for seeker");
            yield return null;
        }
        while (seeker.lightFOV == null || seeker.eventSource == null)
        {
            yield return null;
        }
        FindObjectOfType<CameraFollow>().target = seeker.transform;
        seeker.lightObject.SetActive(true);
        seeker.lightFOV.radius = seeker.vision;
        seeker.eventSource.distance = seeker.vision;
        FindObjectOfType<Camera>().orthographicSize = seeker.vision + 0.5f;
        if (seeker.LOStriggerObject != null) Destroy(seeker.LOStriggerObject);
        foreach (Transform child in seeker.GetComponentsInChildren<Transform>())
        {
            child.gameObject.layer = LayerMask.NameToLayer("Player");
        }
        seeker.GetComponentInChildren<ChangeColorOnLOSEvent>().OnLit();
        seeker.GetComponentInChildren<HideTrail>().Show();
        //SpriteRenderer seekerRend = seeker.GetComponentInChildren<SpriteRenderer>();
        //Color col = seekerRend.color;
        //col.a = 1f;
        //seekerRend.color = col;
    }


    [PunRPC]
    public void BecomeSeeker()
    {
        if (pView.IsMine)
        {
            Hider hider = playerObject.GetComponent<Hider>();
            if (hider != null)
            {
                hider.Respawn();
            }
            PhotonNetwork.Destroy(playerObject);

            chooseSeekerUI = Instantiate(chooseSeekerUIPrefab, GameObject.Find("Canvas").transform);
            Button[] seekerOptionButtons = chooseSeekerUI.GetComponentsInChildren<Button>();
            for (int i = 0; i < seekerOptionButtons.Length; i++)
            {
                int index = i;
                seekerOptionButtons[i].onClick.AddListener(delegate { ChooseSeeker(index); });
            }

            StartCoroutine(ChooseDefaultSeeker());
        }
    }

    private IEnumerator ChooseDefaultSeeker()
    {
        yield return new WaitForSeconds(GameUI.instance.countdownTime + 1f);

        if (FindObjectOfType<Seeker>() == null) ChooseSeeker(0);
    }

    public void ChooseSeeker(int seekerIndex)
    {
        if (pView.IsMine)
        {
            Destroy(chooseSeekerUI);

            playerObject = PhotonNetwork.Instantiate(seekerPrefabs[seekerIndex].name, Vector2.zero, Quaternion.identity);
            pView.RPC("SyncPlayerObject", RpcTarget.OthersBuffered, playerObject.GetPhotonView().ViewID);

            pView.RPC("SeekerChosen", RpcTarget.All);
        }
    }
    [PunRPC]
    private void SeekerChosen()
    {
        RoomPlayersUI.instance.HiderTurnsIntoSeeker(this);
    }



    [PunRPC]
    public void SetPosition(float x, float y)
    {
        if (pView.IsMine)
        {
            playerObject.transform.position = new Vector2(x, y);
            Hider hider = playerObject.GetComponent<Hider>();
            if (hider != null)
            {
                hider.Respawn();
            }
        }
    }
    
}
