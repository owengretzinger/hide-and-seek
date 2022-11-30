using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlimeSeeker : Seeker
{
    public float rollDuration;
    public float rollSpeed;
    public float rollRechargeTime;
    public PhysicsMaterial2D rollMat;
    public GameObject attackChargeUIPrefab;
    public GameObject attackChargeUI;

    public Color darkGreen;
    public Color limeGreen;


    private int attackCharges;
    private Image[] attackChargeIndicators = new Image[3];
    private bool isRolling;
    private float rollRechargeTimer;


    protected override void Start()
    {
        base.Start();

        attackCharges = 3;
        
        if (pView.IsMine)
        {
            attackChargeUI = Instantiate(attackChargeUIPrefab, GameObject.Find("Canvas").transform);
            for (int i = 0; i < 3; i++)
            {
                attackChargeIndicators[i] = attackChargeUI.transform.GetChild(i).GetComponent<Image>();
            }
        }
    }

    protected override void Update()
    {
        base.Update();


        if (!canMove)
            return;

        if (rollRechargeTimer > 0f)
        {
            rollRechargeTimer -= Time.deltaTime;
        }
        else if (attackCharges < 3)
        {
            attackChargeIndicators[attackCharges].gameObject.SetActive(true);
            attackCharges++;
            rollRechargeTimer = rollRechargeTime;
        }

        if (movementVector.magnitude > 0.1f) PlayAnimation(AnimationNames.Walk);
        else PlayAnimation(AnimationNames.Idle);

        if (Input.GetMouseButtonDown(0) && attackCharges > 0)
        {
            StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        canMove = false;
        trailInterface.startLifetime = 1f;
        rb.sharedMaterial = rollMat;
        rollRechargeTimer = rollRechargeTime;
        trail.transform.localPosition = new Vector2(0f, -0.1f);
        trailInterface.startColor = limeGreen;

        yield return null; // for trail

        attackCharges--;
        attackChargeIndicators[attackCharges].gameObject.SetActive(false);

        PlayAnimation(AnimationNames.SlimeIntoBall);
        yield return new WaitForSeconds(0.33f);

        PlayAnimation(AnimationNames.SlimeRollDown);

        isRolling = true;

        Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 vectorToTarget = target - (Vector2)trans.position;
        movementVector = vectorToTarget.normalized;

        rb.velocity = movementVector * rollSpeed;

        yield return new WaitForSeconds(rollDuration);

        rb.velocity = Vector2.zero;

        PlayAnimation(AnimationNames.SlimeOutOfBall);
        yield return new WaitForSeconds(0.33f);

        trail.transform.localPosition = new Vector2(0f, 0.03f);
        trailInterface.startColor = darkGreen;
        rb.sharedMaterial = null;
        trailInterface.startLifetime = 0.3f;

        isRolling = false;
        if (hidersRemaining != 0 && GameManager.instance.gamestate == GameManager.Gamestate.Ingame) canMove = true;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isRolling)
            return;

        Hider hider = collision.gameObject.GetComponent<Hider>();

        if (hider == null)
            return;

        if (hider.isDead)
            return;

        hider.pView.RPC("GetKilled", RpcTarget.AllBuffered);

        GameManager.instance.photonView.RPC("SeekerGotAKill", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);

        hidersRemaining--;
        if (hidersRemaining == 0) GameManager.instance.photonView.RPC("EndRound", RpcTarget.All);
    }



    public override void OnDisable()
    {
        if (pView.IsMine)
        {
            Destroy(attackChargeUI);
        }

        base.OnDisable();
    }
}
