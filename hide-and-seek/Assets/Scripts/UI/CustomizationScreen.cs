using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;

public class CustomizationScreen : MonoBehaviour
{
    public TMP_InputField nameInputField;

    public Image skinImage;
    private HiderSkin[] skins;
    private int currentFrame;

    private int skinIndex;

    const string playerNamePrefKey = "PlayerName";
    const string playerSkinPrefKey = "PlayerSkin";

    public GameObject howToPlay;

    private void Start()
    {
        skins = PlayerSkinsDDOL.instance.skins;

        nameInputField.characterLimit = 10;
        string name = "Player";
        if (PlayerPrefs.HasKey(playerNamePrefKey))
        {
            name = PlayerPrefs.GetString(playerNamePrefKey);
            nameInputField.text = name;
        }
        PhotonNetwork.NickName = name;

        if (!PlayerPrefs.HasKey(playerSkinPrefKey))
        {
            skinIndex = 1;
            PlayerPrefs.SetInt(playerSkinPrefKey, skinIndex);
        }
        else
        {
            skinIndex = PlayerPrefs.GetInt(playerSkinPrefKey);
        }
        
        StartCoroutine("AnimateSkin");

        //int slices = 3;
        //for (int i = 0; i < slices; i++)
        //{
        //    float angle = (360f / slices) * i;

        //    Vector2 pos = new Vector2(Mathf.Sin(Mathf.Deg2Rad * angle), Mathf.Cos(Mathf.Deg2Rad * angle));

        //    Debug.Log("angle: " + angle + "pos: " + pos);
        //}
    }


    private IEnumerator AnimateSkin()
    {
        currentFrame = currentFrame == 0 ? 1 : 0;

        skinImage.sprite = currentFrame == 0 ? skins[skinIndex].walking1 : skins[skinIndex].walking2;

        yield return new WaitForSeconds(0.2f);

        StartCoroutine("AnimateSkin");
    }

    public void ChangeSkin(int direction)
    {
        skinIndex += direction;

        if (skinIndex < 0) skinIndex = skins.Length - 1;
        if (skinIndex >= skins.Length) skinIndex = 0;

        PlayerPrefs.SetInt(playerSkinPrefKey, skinIndex);
        skinImage.sprite = currentFrame == 0 ? skins[skinIndex].walking1 : skins[skinIndex].walking2;
    }

    public void SetPlayerName(string value)
    {
        if (value.Contains(":"))
        {
            value = nameInputField.text.Remove(value.IndexOf(":"));
            nameInputField.text = value;
        }
        if (string.IsNullOrEmpty(value))
        {
            value = "Player";
        }
        PhotonNetwork.NickName = value;
        PlayerPrefs.SetString(playerNamePrefKey, value);
    }

    public void ShowHowToPlay()
    {
        //howToPlay.SetActive(true);
        Application.OpenURL("https://owen-g.itch.io/hidenseek");
    }
}
