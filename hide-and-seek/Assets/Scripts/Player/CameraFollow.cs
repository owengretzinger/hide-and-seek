using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    [Range(0f, 1f)]
    public float smoothing;
    public Transform target;
    private Transform me;



    private void Start()
    {
        me = transform;
    }

    private void FixedUpdate()
    {
        if (target == null) return;

        Vector3 pos = Vector3.Lerp(me.position, target.position, smoothing);
        pos.z = me.position.z;
        me.position = pos;
    }
}
