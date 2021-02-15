using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OutfitData", menuName = "Data/OutfitData", order = 1)]
public class OutfitDatabase : ScriptableObject
{

    public List<DBOutfit> outfitList = new List<DBOutfit>();

    public DBOutfit GetOutfit(int id)
    {
        foreach(DBOutfit o in outfitList)
        {
            if (o.outfitId == id)
            {
                return o;
            }
        }
        return null;
    }

    [Serializable]
    public class DBOutfit
    {
        public int outfitId;
        public string outfitName;
        public GameObject outfitPrefab;

    }


}
