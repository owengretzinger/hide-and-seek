using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;
using System;

public class GameUI : MonoBehaviourPunCallbacks
{
    [HideInInspector] public int countdownTime;

    public static GameUI instance;
    public GameObject startRoundButton;
    public TMP_Text waitingForPlayersText;
    public TMP_Text winnerText;

    public TMP_Text countdownText;

    public GameObject clockObject;
    public TMP_Text clockText;


    private readonly int gameDurationPerHider = 30;
    [HideInInspector] public int clockTime;




    public static event Action CountdownOver;
    public static void FinishCountdown()
    {
        if (CountdownOver != null)
            CountdownOver.Invoke();
    }

    private void Awake()
    {
        instance = this;
        countdownTime = 2;

        if (!PhotonNetwork.IsConnected)
            return;

        waitingForPlayersText.gameObject.SetActive(true);
        UpdateWaitingText();
    }


    public void RestartRound()
    {
        clockObject.SetActive(false);
        waitingForPlayersText.gameObject.SetActive(true);
        winnerText.gameObject.SetActive(false);
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 3 && PhotonNetwork.IsMasterClient)
        {
            startRoundButton.SetActive(true);
            waitingForPlayersText.gameObject.SetActive(false);
        }
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        UpdateWaitingText();
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 2/*3*/ && PhotonNetwork.IsMasterClient && GameManager.instance.gamestate == GameManager.Gamestate.Pregame)
        {
            StartCoroutine(EnableStartRoundButton());
        }
    }
    private IEnumerator EnableStartRoundButton()
    {
        while (FindObjectsOfType<Hider>().Length < PhotonNetwork.CurrentRoom.PlayerCount)
        {
            yield return null;
        }
        startRoundButton.SetActive(true);
        waitingForPlayersText.gameObject.SetActive(false);
    }
    private void UpdateWaitingText()
    {
        int playersNeeded = 3 - PhotonNetwork.CurrentRoom.PlayerCount;

        if (playersNeeded == 2)
        {
            waitingForPlayersText.text = "Waiting for at least " + 2 + " more players";
        }
        else if (playersNeeded == 1)
        {
            waitingForPlayersText.text = "Waiting for at least " + 1 + " more player";
        }
        else
        {
            waitingForPlayersText.text = "Waiting for the host to start the game";
        }
    }

    public void UpdateWinnerText(string winner)
    {
        winnerText.gameObject.SetActive(true);
        winnerText.text = winner.ToUpper() + " WINS!";
    }

    public void StartRound()
    {
        GameManager.instance.StartRound();
        photonView.RPC("NetworkStartRound", RpcTarget.All);
    }

    [PunRPC]
    private void NetworkStartRound()
    {
        startRoundButton.SetActive(false);
        waitingForPlayersText.gameObject.SetActive(false);
        StartCoroutine(Countdown());
        GameManager.instance.gamestate = GameManager.Gamestate.Ingame;
    }

    private IEnumerator Countdown()
    {
        countdownText.gameObject.SetActive(true);

        countdownText.text = "RUN FOR YOUR LIVES";
        yield return new WaitForSeconds(1f);

        int timer = countdownTime;
        while (timer > 0)
        {
            countdownText.text = timer.ToString();
            timer--;
            yield return new WaitForSeconds(1f);
        }
        countdownText.text = "READY OR NOT";
        yield return new WaitForSeconds(1f);
        countdownText.text = "HERE I COME";

        FinishCountdown();

        yield return new WaitForSeconds(1f);
        countdownText.gameObject.SetActive(false);

        if (FindObjectOfType<Seeker>().gameObject.GetPhotonView().IsMine) clockText.text = "Kill the hiders";

        StartCoroutine(Clock());
    }

    private IEnumerator Clock()
    {
        clockObject.SetActive(true);

        clockTime = gameDurationPerHider * FindObjectsOfType<Hider>().Length;
        while (clockTime > 0 && GameManager.instance.gamestate == GameManager.Gamestate.Ingame)
        {
            float minsBeforeFloor = clockTime / 60f;
            int minutes = (int)Math.Floor(minsBeforeFloor);
            int seconds = clockTime % 60;
            string stringSeconds = seconds.ToString().Length == 1 ? "0" + seconds.ToString() : seconds.ToString();
            clockText.text = minutes.ToString() + ":" + stringSeconds;

            clockTime--;

            yield return new WaitForSeconds(1);
        }
        clockText.text = "0:00";

        if (GameManager.instance.gamestate == GameManager.Gamestate.Ingame && PhotonNetwork.IsMasterClient)
        {
            GameManager.instance.photonView.RPC("EndRound", RpcTarget.All);
        }
    }
}
