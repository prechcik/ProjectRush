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
    public int x, y, z;

    public PlayerInfo(int id, string username, string nickname, int currentOutfit, string outfits, int x, int y, int z)
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
