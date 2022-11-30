using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HiderAnimator : MonoBehaviour
{
    private SpriteRenderer rend;

    public int walkFPS = 5;

    [HideInInspector] public int skinIndex;

    public enum Animations { Idle, Walk, Dead };
    private Animations currentAnimation;

    private PhotonView pView;

    private void Start()
    {
        rend = GetComponentInChildren<SpriteRenderer>();
        pView = GetComponent<PhotonView>();

        if (pView.IsMine)
        {
            int skin = PlayerPrefs.GetInt("PlayerSkin");
            pView.RPC("SetSkin", RpcTarget.AllBuffered, skin);
            pView.RPC("SetPlayerUI", RpcTarget.All);
        }

        currentAnimation = Animations.Idle;
        pView.RPC("Idle", RpcTarget.All);
    }

    [PunRPC]
    protected void SetSkin(int index)
    {
        skinIndex = index;
    }
    [PunRPC]
    protected void SetPlayerUI()
    {
        //FindObjectOfType<RoomPlayersUI>().UpdatePlayerUI();
    }

    [PunRPC]
    public void Play(Animations newAnimation)
    {
        if (currentAnimation == newAnimation || currentAnimation == Animations.Dead)
            return;

        pView.RPC("StopRoutines", RpcTarget.All);
        currentAnimation = newAnimation;
        if (newAnimation == Animations.Idle) pView.RPC("Idle", RpcTarget.All);
        else if (newAnimation == Animations.Walk) pView.RPC("Walk", RpcTarget.All);
        else if (newAnimation == Animations.Dead) pView.RPC("Die", RpcTarget.All);
    }

    [PunRPC]
    private void StopRoutines()
    {
        StopAllCoroutines();
    }

    [PunRPC]
    private void Idle()
    {
        ChangeSprite(PlayerSkinsDDOL.instance.skins[skinIndex].idle);
    }

    [PunRPC]
    private void Walk()
    {
        StartCoroutine(WalkRoutine());
    }

    private IEnumerator WalkRoutine()
    {
        while (true)
        {
            ChangeSprite(PlayerSkinsDDOL.instance.skins[skinIndex].walking1);
            yield return new WaitForSeconds(1f / walkFPS);
            ChangeSprite(PlayerSkinsDDOL.instance.skins[skinIndex].walking2);
            yield return new WaitForSeconds(1f / walkFPS);
        }
    }

    [PunRPC]
    private void Die()
    {
        ChangeSprite(PlayerSkinsDDOL.instance.skins[skinIndex].dead);
    }

    private void ChangeSprite(Sprite sprite)
    {
        if (rend != null)
        {
            rend.sprite = sprite;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
