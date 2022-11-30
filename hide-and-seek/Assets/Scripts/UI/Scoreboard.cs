using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class Scoreboard : MonoBehaviourPunCallbacks
{
    public static Scoreboard instance;

    public CanvasGroup scoreboardGroup;

    public TMP_Text[] scoreTexts;
    [HideInInspector] public Dictionary<Player, TMP_Text> scores = new Dictionary<Player, TMP_Text>();

    public Transform leftColumn;
    public Transform rightColumn;

    private Player[] photonPlayers;

    private void Start()
    {
        instance = this;

        if (!PhotonNetwork.IsConnected)
            return;

        photonPlayers = PhotonNetwork.PlayerList;
        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            AddPlayerToScoreboard(photonPlayers[i]);
        }
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        AddPlayerToScoreboard(other);
    }

    private void AddPlayerToScoreboard(Player player)
    {
        scoreTexts[scores.Count].gameObject.SetActive(true);
        scores.Add(player, scoreTexts[scores.Count]);
        UpdateScoreboard(player, 0);
    }

    [PunRPC]
    private void PlayerGainsScore(int actorNumber, int pointsGained)
    {
        StartCoroutine(WaitForUpdateScore(PhotonNetwork.CurrentRoom.GetPlayer(actorNumber), pointsGained));
    }
    private IEnumerator WaitForUpdateScore(Player player, int pointsGained)
    {
        while (!scores.ContainsKey(player))
        {
            yield return null;
        }

        UpdateScoreboard(player, pointsGained);
    }

    public void UpdateScoreboard(Player player, int scoreChange)
    {
        scores[player].text = player.NickName + ": " + (GetScore(player) + scoreChange).ToString();

        // get sorted list of scores
        List<Player> sortedPlayers = scores.Keys.OrderByDescending(p => GetScore(p)).ToList();
        // set order in hierarchy & layout group will take care of the rest
        foreach (Player p in new List<Player>(scores.Keys))
        {
            int place = sortedPlayers.IndexOf(p);
            if (place < 5) scores[p].transform.SetParent(leftColumn);
            else scores[p].transform.SetParent(rightColumn);
            scores[p].transform.SetSiblingIndex(sortedPlayers.IndexOf(p) % 5);
        }
    }

    private int GetScore(Player player)
    {
        return int.Parse(scores[player].text.Split(':')[1].Substring(1));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ShowScoreboard();
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            HideScoreboard();
        }
    }

    private void ShowScoreboard()
    {
        scoreboardGroup.alpha = 1f;
    }

    private void HideScoreboard()
    {
        scoreboardGroup.alpha = 0f;
    }
}
