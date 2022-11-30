using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Skin", menuName = "Player Skin")]
public class HiderSkin : ScriptableObject
{
    public Sprite idle;
    public Sprite walking1;
    public Sprite walking2;
    public Sprite dead;
}
