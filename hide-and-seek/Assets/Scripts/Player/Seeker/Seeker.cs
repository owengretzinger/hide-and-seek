using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Seeker : PlayerMovement
{
    public GameObject hotColdSliderPrefab;

    private Animator anim;
    public enum AnimationNames { Idle, Walk, ReaperAttack, SlimeIntoBall, SlimeOutOfBall, SlimeRollUp, SlimeRollDown, SlimeRollSide };


    public ParticleSystem trail;
    protected ParticleSystem.MainModule trailInterface;

    protected int hidersRemaining;

    protected HotColdMeter hotColdMeter;


    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }
    protected override void Start()
    {
        trailInterface = trail.main;
        base.Start();

        if (pView.IsMine)
        {
            lightFOV.radius = vision;
            eventSource.distance = vision;
            FindObjectOfType<Camera>().orthographicSize = vision + 0.5f;

            canMove = false;
            GameUI.CountdownOver += EnableMovement;

            hotColdMeter = gameObject.AddComponent<HotColdMeter>();
        }

        hidersRemaining = FindObjectsOfType<Hider>().Length;
    }
    //protected override void Update()
    //{
    //    base.Update();
    //}

    protected void EnableMovement()
    {
        GameUI.CountdownOver -= EnableMovement;
        canMove = true;
    }
    

    protected void PlayAnimation(AnimationNames clipName)
    {
        string clip = clipName.ToString();
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(clip))
            return;

        pView.RPC("PlayAnimationOverNetwork", RpcTarget.All, clip);
    }
    [PunRPC]
    protected void PlayAnimationOverNetwork(string clip)
    {
        anim.Play(clip);
    }

    
    

    // adding dead players
    public void UpdateMinimapPlayers()
    {
        if (!pView.IsMine) return;

        foreach (RectTransform hider in new List<RectTransform>(minimapHiders.Values))
        {
            Destroy(hider.gameObject);
        }
        minimapHiders.Clear();

        foreach (Hider hider in FindObjectsOfType<Hider>())
        {
            if (!hider.isDead) continue;

            RectTransform newIcon = Instantiate(minimapPlayerIcon.gameObject, minimapPlayerIcon.parent).GetComponent<RectTransform>();
            newIcon.GetComponent<Image>().color = new Color(1f, 0f, 0f);
            newIcon.SetAsFirstSibling();
            minimapHiders.Add(hider, newIcon);
        }
    }

    public override void OnDisable()
    {
        if (pView == null) return;

        if (pView.IsMine)
        {
            foreach (RectTransform hider in new List<RectTransform>(minimapHiders.Values))
            {
                Destroy(hider.gameObject);
            }
            minimapHiders.Clear();
        }
    }
}
