using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct PlayerInfo : NetworkMessage
{
    public int id;
    public string username;
    public string nickname;
    public int currentOutfit;
    public string outfits;
    public float x, y, z;
    public string userId;
    public int experience;
    public int level;
    
}
