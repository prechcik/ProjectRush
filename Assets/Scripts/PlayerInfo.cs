using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerInfo
{
    public int id;
    public string username;
    public string nickname;
    public int currentOutfit;
    public string outfits;
    public float x, y, z;
    public string userId;
    public PlayerInfo(int id, string username, string nickname, int currentOutfit, string outfits, float x, float y, float z)
    {
        this.id = id;
        this.username = username;
        this.nickname = nickname;
        this.currentOutfit = currentOutfit;
        this.x = x;
        this.y = y;
        this.z = z;
        this.outfits = outfits;

    }

    public PlayerInfo() { }
}
