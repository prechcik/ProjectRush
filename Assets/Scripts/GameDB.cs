using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDB : MonoBehaviour
{

    public List<GameObject> outfitList = new List<GameObject>();

    private List<int> randList = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<int> GetRandomOutfits(int amount)
    {
        randList = new List<int>();
        
        while(randList.Count < amount)
        {
            int rand = Random.Range(0, outfitList.Count);
            if (!randList.Contains(rand))
            {
                randList.Add(rand);
            }
        }
        return randList;
    }

    public Outfit GetOutfit(int id)
    {
        foreach (GameObject o in outfitList)
        {
            Outfit ou = o.GetComponent<Outfit>();
            if (ou.id == id)
            {
                return ou;
            }
        }
        return null;
    }
}
