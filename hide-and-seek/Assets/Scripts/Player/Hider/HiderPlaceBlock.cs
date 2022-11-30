using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class HiderPlaceBlock : MonoBehaviour
{
    private Transform trans;
    private PhotonView pView;
    private GameObject barrierSliderObject;


    [HideInInspector] public Hider hiderScript;

    private GameObject lastBarrier;
    [HideInInspector] public Slider barrierCooldownSlider;
    private CanvasGroup barrierCooldownAlpha;

    private bool canPlaceBarrier;


    private void Start()
    {
        trans = transform;
        pView = GetComponent<PhotonView>();
        if (pView.IsMine)
        {
            barrierSliderObject = GameObject.Find("Barrier Cooldown").transform.GetChild(0).gameObject;
            barrierSliderObject.SetActive(false);

            barrierSliderObject.SetActive(true);
            barrierCooldownAlpha = barrierSliderObject.GetComponent<CanvasGroup>();
            barrierCooldownSlider = barrierCooldownAlpha.GetComponentInChildren<Slider>();
            barrierCooldownSlider.value = barrierCooldownSlider.maxValue;
            canPlaceBarrier = true;
        }
    }

    private void Update()
    {
        if (!hiderScript.canMove)
            return;

        if (Input.GetMouseButtonDown(0) && canPlaceBarrier)
        {
            PlaceBarrier();
        }
    }


    private void PlaceBarrier()
    {
        barrierCooldownAlpha.alpha = 0.8f;
        barrierCooldownSlider.value = 0f;
        canPlaceBarrier = false;

        Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition) - trans.position;

        Vector2 pos = (Vector2)trans.position + target.normalized * 0.5f;

        float direction = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;

        GameObject barrier = PhotonNetwork.Instantiate("Barrier", pos, Quaternion.Euler(0, 0, direction));
        lastBarrier = barrier;

        pView.RPC("SyncBarrierObject", RpcTarget.All, barrier.GetPhotonView().ViewID);
    }
    [PunRPC]
    private void SyncBarrierObject(int viewID)
    {
        PhotonView barrierView = PhotonView.Find(viewID);
        StartCoroutine(DestroyBarrier(barrierView.gameObject));
    }
    private IEnumerator DestroyBarrier(GameObject barrier)
    {
        SpriteRenderer barrierRend = barrier.GetComponent<SpriteRenderer>();

        float lifetime = 10f;
        while (lifetime > 0f)
        {
            if (barrierRend == null) break;

            Color barrierCol = barrierRend.color;
            barrierCol.a -= Time.deltaTime * (15f / 255f);
            barrierRend.color = barrierCol;

            yield return null;
            lifetime -= Time.deltaTime;
        }

        if (barrierRend != null)
        {
            barrier.GetComponent<BoxCollider2D>().enabled = false;

            float alpha = barrierRend.color.a;
            while (alpha > 0f && barrier != null)
            {
                if (barrierRend == null) break;

                Color barrierCol = barrierRend.color;
                barrierCol.a -= Time.deltaTime * 2f;
                alpha = barrierCol.a;
                barrierRend.color = barrierCol;
                yield return null;
            }
        }

        if (barrier != null) if (barrier.GetPhotonView().IsMine)
            {
                PhotonNetwork.Destroy(barrier);
                StartCoroutine(RechargeBarrier());
            }


    }

    private IEnumerator RechargeBarrier()
    {
        while (barrierCooldownSlider.value < barrierCooldownSlider.maxValue)
        {
            barrierCooldownSlider.value += Time.deltaTime;
            yield return null;
        }
        barrierCooldownAlpha.alpha = 1f;
        canPlaceBarrier = true;
    }

    public void Respawn()
    {
        if (pView.IsMine && lastBarrier != null)
        {
            PhotonNetwork.Destroy(lastBarrier);
        }
        //if (resetNetworkPlayer) pView.RPC("ResetNetworkPlayer", RpcTarget.All);
        if (barrierCooldownSlider != null) barrierCooldownSlider.value = barrierCooldownSlider.maxValue;
        canPlaceBarrier = true;
    }
}
