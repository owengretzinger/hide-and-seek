using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;

    public enum Gamestate { Pregame, Ingame, Aftergame };
    public Gamestate gamestate;

    public GameObject playerPrefab;

    //private bool firstRound;

    //private List<PlayerManager> playerList = new List<PlayerManager>();
    //private int seekerIndex;

    private void Start()
    {
        instance = this;
        //seekerIndex = -1;

        //PhotonNetwork.OfflineMode = true;
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene("Menu");
            return;
        }
        GameObject p = PhotonNetwork.Instantiate(playerPrefab.name, Vector2.zero, Quaternion.identity);
        //photonView.RPC("AddPlayerToList", RpcTarget.MasterClient, p.GetPhotonView().ViewID);

        gamestate = Gamestate.Pregame;
        //firstRound = true;
    }
    //[PunRPC]
    //private void AddPlayerToList(int viewID)
    //{
    //    PhotonView playerView = PhotonView.Find(viewID);
    //    playerList.Add(playerView.GetComponent<PlayerManager>());
    //}

    //[PunRPC]
    //public void SendPlayerOrder()
    //{
    //    List<int> playerOrder = new List<int>();
    //    for (int i = 0; i < playerList.Count; i++)
    //    {
    //        int index = i + seekerIndex + 1;
    //        if (index >= playerList.Count) index -= playerList.Count;

    //        playerOrder.Add(playerList[index].gameObject.GetPhotonView().ViewID);
    //    }
    //    RoomPlayersUI.instance.photonView.RPC("ReceivePlayerOrder", RpcTarget.All, playerOrder.ToArray());
    //}


    public void StartRound()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //PlayerManager[] players = FindObjectsOfType<PlayerManager>();

            //int seekerIndex = -1;
            //if (firstRound)
            //{
            //    seekerIndex = UnityEngine.Random.Range(0, players.Length);
            //    firstRound = false;
            //}
            //else
            //{
            //    foreach (PlayerManager player in players)
            //    {
            //        if (player.isNextSeeker)
            //        {
            //            seekerIndex = Array.IndexOf(players, player);
            //            player.isNextSeeker = false;
            //            break;
            //        }
            //    }
            //}
            //seekerIndex++;
            //if (seekerIndex >= playerList.Count) seekerIndex = 0;
            RoomPlayersUI.instance.playerOrder[0].pView.RPC("BecomeSeeker", RpcTarget.All);

            //playerList[seekerIndex].pView.RPC("BecomeSeeker", RpcTarget.All);


            for (int i = 1; i < RoomPlayersUI.instance.playerOrder.Count; i++)
            {
                //if (i == seekerIndex) continue;

                //int index = i;
                //if (seekerIndex < index) index--;

                float angle = (360f / (RoomPlayersUI.instance.playerOrder.Count - 1)) * (i - 1) - 90f;

                Vector2 pos = new Vector2(Mathf.Sin(Mathf.Deg2Rad * angle), Mathf.Cos(Mathf.Deg2Rad * angle)) * 0.8f;

                RoomPlayersUI.instance.playerOrder[i].pView.RPC("SetPosition", RpcTarget.All, pos.x, pos.y);
                //playerList[i].pView.RPC("SetPosition", RpcTarget.All, pos.x, pos.y);
            }
        }
    }

    [PunRPC]
    public void EndRound()
    {
        StartCoroutine(EndRoundRoutine());
    }

    private IEnumerator EndRoundRoutine()
    {
        gamestate = Gamestate.Aftergame;

        Hider[] hiders = FindObjectsOfType<Hider>();
        List<Hider> winners = new List<Hider>();
        foreach (Hider hider in hiders)
        {
            if (hider.isDead == false)
            {
                winners.Add(hider);
            }
        }
        yield return new WaitForSeconds(1f);

        PlayerMovement[] playerMovements = FindObjectsOfType<PlayerMovement>();
        foreach (PlayerMovement player in playerMovements)
        {
            player.canMove = false;
        }

        CameraFollow cam = FindObjectOfType<CameraFollow>();
        if (winners.Count > 0)
        {
            int points = (int)Math.Round(hiders.Length * 100f / winners.Count);
            foreach (Hider winner in winners)
            {
                GameUI.instance.UpdateWinnerText(winner.pView.Owner.NickName);
                cam.target = winner.transform;
                winner.lightObject.SetActive(true);
                winner.lightFOV.radius = winner.vision;

                if (PhotonNetwork.IsMasterClient) Scoreboard.instance.photonView.RPC("PlayerGainsScore", RpcTarget.AllBuffered, winner.pView.Owner.ActorNumber, points);

                yield return new WaitForSeconds(2f);

                if (winner.lightObject != null) winner.lightObject.SetActive(false);
            }
        }
        else
        {
            Seeker winner = FindObjectOfType<Seeker>();

            GameUI.instance.UpdateWinnerText(winner.pView.Owner.NickName);
            cam.target = winner.transform;
            winner.lightObject.SetActive(true);
            winner.lightFOV.radius = winner.vision;

            int points = GameUI.instance.clockTime * 5;
            if (PhotonNetwork.IsMasterClient) Scoreboard.instance.photonView.RPC("PlayerGainsScore", RpcTarget.AllBuffered, winner.pView.Owner.ActorNumber, points);

            yield return new WaitForSeconds(2f);
        }

        //Hider winner = null;
        //foreach (Hider hider in hiders)
        //{
        //    if (hider.isDead == false)
        //    {
        //        winner = hider;
        //        break;
        //    }
        //}

        //yield return new WaitForSeconds(1f);

        //GameUI.instance.UpdateWinnerText(winner.pView.Owner.NickName);
        //FindObjectOfType<CameraFollow>().target = winner.transform;
        //winner.lightObject.SetActive(true);
        //winner.lightFOV.radius = winner.vision;

        
        StartCoroutine(RestartRound());
    }
    [PunRPC]
    private void SeekerGotAKill(int actorNumber)
    {
        if (PhotonNetwork.IsMasterClient) Scoreboard.instance.photonView.RPC("PlayerGainsScore", RpcTarget.AllBuffered, actorNumber, 50);
    }

    private IEnumerator RestartRound()
    {
        gamestate = Gamestate.Pregame;

        

        PlayerMovement[] playerMovements = FindObjectsOfType<PlayerMovement>();
        foreach (PlayerMovement player in playerMovements)
        {
            Hider hider = player.GetComponent<Hider>();
            if (hider != null) hider.Respawn();
            if (player.pView.IsMine) PhotonNetwork.Destroy(player.gameObject);
        }

        //yield return new WaitForSeconds(1f);
        bool allNull = false;
        while (!allNull)
        {
            allNull = true;
            foreach (PlayerMovement player in playerMovements)
            {
                if (player != null) allNull = false;
            }
            yield return null;
        }

        if (PhotonNetwork.IsMasterClient) photonView.RPC("SpawnInPlayersOnRestart", RpcTarget.All);
    }

    [PunRPC]
    private void SpawnInPlayersOnRestart()
    {
        RoomPlayersUI.instance.SeekerTurnsIntoHider();

        foreach (PlayerManager player in FindObjectsOfType<PlayerManager>())
        {
            player.Spawn();
        }

        GameUI.instance.RestartRound();
    }


    /// <summary>
    /// Called when the local player left the room. We need to load the launcher scene.
    /// </summary>
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public override void OnPlayerEnteredRoom(Player other)
    {

    }


    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("\"{0}\" left the room.", other.NickName);

        // update ui or something instead later idk
        PhotonNetwork.Disconnect();
    }


    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

}
