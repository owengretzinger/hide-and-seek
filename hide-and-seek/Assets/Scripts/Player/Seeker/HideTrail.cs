using UnityEngine;
using System.Collections;
using LOS.Event;

public class HideTrail : MonoBehaviour
{
    private ParticleSystem trail;

    void Start()
    {
        if (transform.parent.GetComponent<Seeker>() == null)
        {
            Destroy(this);
            return;
        }

        trail = transform.parent.GetComponentInChildren<ParticleSystem>();

        LOSEventTrigger trigger = GetComponent<LOSEventTrigger>();
        trigger.OnNotTriggered += Hide;
        trigger.OnTriggered += Show;

        //Hide();
    }

    private void Hide()
    {
        trail.Stop();
    }

    public void Show()
    {
        trail.Play();
    }
}
