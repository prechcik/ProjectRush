using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDB : MonoBehaviour
{

    public List<GameObject> outfitList = new List<GameObject>();
    public List<Sprite> packageRarities = new List<Sprite>();

    private List<int> randList = new List<int>();

    public FirebaseManager DBManager;


    // Start is called before the first frame update
    void Start()
    {

        DontDestroyOnLoad(this.gameObject);
        DBManager = FindObjectOfType<FirebaseManager>();


    }

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        DBManager = FindObjectOfType<FirebaseManager>();
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
            int rand = UnityEngine.Random.Range(1, outfitList.Count);
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

    public Sprite GetRaritySprite(int index)
    {
        return packageRarities[index];
    }
}
