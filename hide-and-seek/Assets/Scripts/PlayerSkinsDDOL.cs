using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkinsDDOL : MonoBehaviour
{
    public static PlayerSkinsDDOL instance;

    public HiderSkin[] skins;


    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
    }
}
