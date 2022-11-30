using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReaperSeeker : Seeker
{
    public float slowTime;
    public float attackDistance;


    protected override void Update()
    {
        base.Update();

        if (!canMove)
            return;

        if (Input.GetMouseButtonDown(0) && currentMovementSpeed > speed * 0.75f)
        {
            StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        canMove = false;
        trailInterface.startLifetime = 1f;

        yield return null; // for trail

        //pView.RPC("PlayIdleAnimation", RpcTarget.All);
        PlayAnimation(AnimationNames.ReaperAttack);

        Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 vectorToTarget = target - (Vector2)trans.position;

        Vector2 newPos;

        if (vectorToTarget.magnitude < attackDistance) newPos = target;
        else newPos = (Vector2)trans.position + vectorToTarget.normalized * attackDistance;

        RaycastHit2D wallHit = Physics2D.Raycast(trans.position, vectorToTarget, (newPos - (Vector2)trans.position).magnitude, LayerMask.GetMask("Object"));
        RaycastHit2D enemyHit = Physics2D.Raycast(trans.position, vectorToTarget, (newPos - (Vector2)trans.position).magnitude + 0.6f, LayerMask.GetMask("Enemy"));

        if (enemyHit && wallHit)
        {
            if ((wallHit.point - (Vector2)trans.position).magnitude < (enemyHit.point - (Vector2)trans.position).magnitude)
            {
                enemyHit = new RaycastHit2D();
            }
        }
        int side = 1;
        if (enemyHit && enemyHit.collider.gameObject.GetComponentInChildren<SpriteRenderer>().color.a != 0f)
        {
            Hider enemy = enemyHit.collider.gameObject.GetComponent<Hider>();
            if (!enemy.isDead)
            {
                side = vectorToTarget.x > 0f ? -1 : 1;
                newPos = enemyHit.collider.transform.position + Vector3.right * side * 0.5f + Vector3.up * 0.1f;

                enemyHit.collider.gameObject.GetComponent<Hider>().pView.RPC("GetKilled", RpcTarget.AllBuffered);

                GameManager.instance.photonView.RPC("SeekerGotAKill", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);

                hidersRemaining--;
                if (hidersRemaining == 0) GameManager.instance.photonView.RPC("EndRound", RpcTarget.All);
            }
            else
            {
                enemyHit = new RaycastHit2D();
            }
        }
        else if (wallHit)
        {
            newPos = wallHit.point - vectorToTarget.normalized * 0.2f;
        }

        trans.position = newPos;

        bool shouldFlipRend;
        if (enemyHit)
        {
            if (trans.position.x < enemyHit.collider.transform.position.x) shouldFlipRend = false;
            else shouldFlipRend = true;
        }
        else
        {
            if (vectorToTarget.x < 0f) shouldFlipRend = true;
            else if (vectorToTarget.x > 0f) shouldFlipRend = false;
            else shouldFlipRend = rend.flipX;
        }
        xPosLastFrame = trans.position.x;
        pView.RPC("FlipRend", RpcTarget.All, shouldFlipRend);

        hotColdMeter.scrambleHotColdSliderTime = 7f;

        float timer = 0.416f;
        while (timer > 0f)
        {
            if (enemyHit) trans.position = enemyHit.collider.transform.position + Vector3.right * side * 0.5f + Vector3.up * 0.1f;

            timer -= Time.deltaTime;
            yield return null;
        }


        //pView.RPC("PlayIdleAnimation", RpcTarget.All);
        PlayAnimation(AnimationNames.Idle);
        trailInterface.startLifetime = 0.3f;


        if (hidersRemaining != 0) canMove = true;

        currentMovementSpeed = 0f;
        float slowdownAmount = speed / slowTime;
        while (currentMovementSpeed < speed)
        {
            currentMovementSpeed += slowdownAmount * Time.deltaTime;
            yield return null;
        }
        currentMovementSpeed = speed;
    }
}
