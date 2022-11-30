using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{

    Transform trans;

    private float scaleX;

    private readonly float delayLow = 1f;
    private readonly float delayHigh = 3f;
    private readonly float speedLow = 0.5f;
    private readonly float speedHigh = 2f;

    private void Start()
    {
        trans = GetComponent<Transform>();
        scaleX = trans.localScale.x;

        StartCoroutine(MoveDoor(true));
    }

    private IEnumerator MoveDoor(bool open)
    {
        float delay = GetDelay();
        yield return new WaitForSeconds(delay);

        float speed = GetSpeed();

        float target = open ? 1.9f : 0.1f;

        int direction = open ? 1 : -1;

        if (open)
        {
            while (trans.localScale.y < target)
            {
                Wait(speed, direction);
                yield return null;
            }
        }
        else
        {
            while (trans.localScale.y > target)
            {
                Wait(speed, direction);
                yield return null;
            }
        }

        StartCoroutine(MoveDoor(!open));
    }

    private void Wait(float speed, float direction)
    {
        float scaleY = trans.localScale.y + speed * direction * Time.deltaTime;
        trans.localScale = new Vector3(scaleX, scaleY, 1f);
    }



    private float GetDelay()
    {
        return Random.Range(delayLow, delayHigh);
    }

    private float GetSpeed()
    {
        return Random.Range(speedLow, speedHigh);
    }
}
