using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using LOS;
using UnityEngine.UI;
using Photon.Realtime;
using LOS.Event;
using System;

public class Hider : PlayerMovement
{
    [HideInInspector] public HiderAnimator anim;
    [HideInInspector] public bool isDead;

    private HiderPlaceBlock blockScript;
    //public GameObject barrierPrefab;


    protected override void Start()
    {
        anim = GetComponent<HiderAnimator>();
        
        base.Start();

        blockScript = gameObject.AddComponent<HiderPlaceBlock>();
        blockScript.hiderScript = this;

        if (pView.IsMine)
        {
            lightFOV.radius = vision;
            eventSource.distance = vision;
            FindObjectOfType<Camera>().orthographicSize = vision + 0.5f;

            StartCoroutine(UpdateMinimapPlayers());
        }
    }
    public IEnumerator UpdateMinimapPlayers()
    {
        if (GameManager.instance.gamestate == GameManager.Gamestate.Pregame) yield return new WaitForSeconds(2f);

        foreach (RectTransform hider in new List<RectTransform>(minimapHiders.Values))
        {
            Destroy(hider.gameObject);
        }
        minimapHiders.Clear();

        foreach (Hider hider in FindObjectsOfType<Hider>())
        {
            if (minimapPlayerIcon == null) continue;
            if (minimapPlayerIcon.parent == null) continue;

            RectTransform newIcon = Instantiate(minimapPlayerIcon.gameObject, minimapPlayerIcon.parent).GetComponent<RectTransform>();
            if (hider.isDead)
            {
                newIcon.GetComponent<Image>().color = new Color(1f, 0f, 0f);
            }
            else if (hider == this)
            {
                newIcon.GetComponent<Image>().color = new Color(0f, 40f / 255f, 1f);
            }
            else
            {
                newIcon.GetComponent<Image>().color = new Color(70f / 255f, 1f, 1f);
            }
            if (hider != this) newIcon.SetAsFirstSibling();
            minimapHiders.Add(hider, newIcon);
        }
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        // man idk
        if (this != null) 
            if (gameObject.GetPhotonView() != null) 
                if (gameObject.GetPhotonView().Owner != other) 
                    if (pView != null)
                        if (pView.IsMine)
                            StartCoroutine(UpdateMinimapPlayers());
    }

    protected override void Update()
    {
        base.Update();

        if (!canMove)
            return;

        if (movementVector.magnitude > 0.1f) anim.Play(HiderAnimator.Animations.Walk);
        else anim.Play(HiderAnimator.Animations.Idle);

        
    }

    [PunRPC]
    public void GetKilled()
    {
        StartCoroutine(GetKilledRoutine());
    }
    private IEnumerator GetKilledRoutine()
    {
        while (anim == null)
        {
            yield return null;
        }

        anim.Play(HiderAnimator.Animations.Dead);
        canMove = false;
        rb.isKinematic = true;
        isDead = true;
        RoomPlayersUI.instance.HiderDied(this);

        FindObjectOfType<Seeker>().UpdateMinimapPlayers();

        // spectating
        if (pView.IsMine)
        {
            PlayerManager[] managers = FindObjectsOfType<PlayerManager>();
            foreach (PlayerManager manager in managers)
            {
                if (manager.pView.IsMine)
                {
                    yield return new WaitForSeconds(1.5f);
                    if (GameManager.instance.gamestate == GameManager.Gamestate.Ingame)
                    {
                        lightObject.SetActive(false);
                        LOStriggerObject = Instantiate(LOStriggerObjectPrefab, trans);
                        ChangeColorOnLOSEvent changeColScript = LOStriggerObject.GetComponent<ChangeColorOnLOSEvent>();
                        Color white = Color.white;
                        changeColScript.litColor = white;
                        white.a = 0f;
                        changeColScript.notLitColor = white;
                        foreach (Transform child in GetComponentsInChildren<Transform>())
                        {
                            child.gameObject.layer = LayerMask.NameToLayer("Enemy");
                        }
                        LOStriggerObject.layer = LayerMask.NameToLayer("LOS Trigger");

                        StartCoroutine(manager.WaitForSpectate());
                    }
                    break;
                }
            }
        }
    }


    public virtual void Respawn()
    {
        blockScript.Respawn();
        if (pView.IsMine)
        {
            StartCoroutine(UpdateMinimapPlayers());
        }
    }
    //[PunRPC]
    //private void ResetNetworkPlayer()
    //{
    //    //StopAllCoroutines();
    //    // find object bc for some reason didnt work otherwise
    //    //LOSRadialLight lightFOV = FindObjectOfType<LOSRadialLight>();


    //    //if (lightFOV != null) lightFOV.Recalculate();
    //    //GameObject.Find("Clock Tip Text").GetComponent<TMPro.TMP_Text>().text = "Be the last one standing";
    //}

    new private void OnDisable()
    {
        foreach (RectTransform hider in new List<RectTransform>(minimapHiders.Values))
        {
            if (hider != null) Destroy(hider.gameObject);
        }
        minimapHiders.Clear();
    }
}
