using UnityEngine;
using System.Collections;
using Photon.Pun;
using LOS;
using LOS.Event;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    public GameObject lightObject;
    public int testNum;

    public float speed;
    public float vision;


    protected float currentMovementSpeed;

    [HideInInspector] public bool canMove;

    protected Vector2 movementVector;

    protected Transform trans;
    protected Rigidbody2D rb;
    [HideInInspector] public PhotonView pView;
    protected SpriteRenderer rend;
    //private bool flipSprite;
    protected BoxCollider2D col;
    [HideInInspector] public LOSRadialLight lightFOV;
    [HideInInspector] public LOSEventSource eventSource;

    protected float xPosLastFrame = 0f;

    //protected GameObject barrierSliderObject;
    //protected GameObject hotColdSliderObject;

    protected RectTransform minimapPlayerIcon;
    protected float minimapConversion;

    protected Dictionary<Hider, RectTransform> minimapHiders = new Dictionary<Hider, RectTransform>();

    public GameObject LOStriggerObjectPrefab;
    [HideInInspector] public GameObject LOStriggerObject;

    protected virtual void Start() 
    {
        currentMovementSpeed = speed;
        canMove = true;
		trans = transform;
        rb = GetComponent<Rigidbody2D>();
        rend = GetComponentInChildren<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();

        pView = GetComponent<PhotonView>();
        lightFOV = GetComponentInChildren<LOSRadialLight>();
        eventSource = GetComponentInChildren<LOSEventSource>();
        
        if (pView.IsMine)
        {
            FindObjectOfType<CameraFollow>().target = trans;
            Destroy(GetComponent<LOSEventTrigger>());
            Destroy(GetComponent<ChangeColorOnLOSEvent>());

            HideTrail trail = GetComponent<HideTrail>();
            if (trail != null) Destroy(trail);
            foreach (Transform child in GetComponentsInChildren<Transform>())
            {
                child.gameObject.layer = LayerMask.NameToLayer("Player");
            }
            pView.RPC("SetName", RpcTarget.AllBuffered, pView.Owner.NickName);

            
            

            minimapPlayerIcon = GameObject.Find("Minimap Player Icon").GetComponent<RectTransform>();
            minimapConversion = 0.026f * minimapPlayerIcon.parent.GetComponent<RectTransform>().sizeDelta.x;

            //if (pView.Owner.NickName == "test") StartCoroutine(TestMove());
        }
        else
        {
            LOStriggerObject = Instantiate(LOStriggerObjectPrefab, trans);
            lightObject.SetActive(false);
            col.isTrigger = true;
            col.size = new Vector2(col.size.x * 2f, col.size.y * 2f);
            canMove = false;
        }
    }

    [PunRPC]
    protected void SetName(string name)
    {
        GetComponentInChildren<TMPro.TMP_Text>().text = name;
    }


    protected virtual void Update()
    {
        if (pView.IsMine)
        {
            minimapPlayerIcon.anchoredPosition = trans.position * minimapConversion;
            foreach (Hider hider in new List<Hider>(minimapHiders.Keys))
            {
                if (hider != null) minimapHiders[hider].anchoredPosition = hider.trans.position * minimapConversion;
            }
        }

        if (!canMove/* || pView.Owner.NickName == "test"*/)
            return;

        movementVector = new Vector2(Input.GetAxisRaw("Horizontal" + testNum), Input.GetAxisRaw("Vertical" + testNum)).normalized;
    }
    private IEnumerator TestMove()
    {

        int dir = Random.Range(0, 2);
        if (dir == 0) dir = -1;
        movementVector = new Vector2(dir, 0f);

        yield return new WaitForSeconds(1f);

        StartCoroutine(TestMove());
    }

    protected virtual void FixedUpdate()
    {
        if (!canMove)
            return;

        float difference = trans.position.x - xPosLastFrame;
        bool shouldFlipRend;
        if (difference < -0.01f)
        {
            shouldFlipRend = true;
        }
        else if (difference > 0.01f)
        {
            shouldFlipRend = false;
        }
        else
        {
            shouldFlipRend = rend.flipX;
        }
        if (rend.flipX != shouldFlipRend)
        {
            pView.RPC("FlipRend", RpcTarget.All, shouldFlipRend);
        }
        xPosLastFrame = trans.position.x;

        rb.MovePosition(rb.position + movementVector * currentMovementSpeed * Time.fixedDeltaTime);
    }

    [PunRPC]
    protected void FlipRend(bool shouldFlip)
    {
        if (rend != null) rend.flipX = shouldFlip;
    }

    //public void OnPhotonInstantiate(PhotonMessageInfo info)
    //{
    //    Debug.Log("instantiated");
    //    if (lightFOV != null) lightFOV.Recalculate();
    //}

    //public override void OnPhotonInstantiate(PhotonMessageInfo info)
    //{
    //    lightFOV.Recalculate();
    //}
}
