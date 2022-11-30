using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObjectManager : MonoBehaviour
{
    public GameObject doorPrefab;
    public Transform[] doorSpawns;

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        if (!PhotonNetwork.IsMasterClient)
            return;

        foreach (Transform doorTrans in doorSpawns)
        {
            GameObject door = PhotonNetwork.Instantiate(doorPrefab.name, doorTrans.position, doorTrans.rotation);
            door.AddComponent<Door>();
        }
    }
}
