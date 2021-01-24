using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDB : MonoBehaviour
{

    public List<GameObject> outfitList = new List<GameObject>();
    public List<Sprite> packageRarities = new List<Sprite>();

    private List<int> randList = new List<int>();

    public FirebaseManager DBManager;
    public NetworkManagement network;

    public float expRate = 3 / 2;


    // Start is called before the first frame update
    void Start()
    {

        DontDestroyOnLoad(this.gameObject);
        DBManager = FindObjectOfType<FirebaseManager>();
        network = FindObjectOfType<NetworkManagement>();
    }

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        DBManager = FindObjectOfType<FirebaseManager>();
        network = FindObjectOfType<NetworkManagement>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<int> GetRandomOutfits(int amount)
    {
        randList = new List<int>();
        List<int> playerOutfits = NetworkClient.connection.identity.GetComponent<Player>().outfits;
        List<int> allOutfits = new List<int>();
        foreach (GameObject g in outfitList)
        {
            allOutfits.Add(g.GetComponent<Outfit>().id);
        }
        List<int> possibleOutfits = allOutfits.Except(playerOutfits).ToList();
        
        int possibleRewards = possibleOutfits.Count;
        if (possibleRewards > amount) { possibleRewards = amount; }
        List<int> shuffledRewards = possibleOutfits.OrderBy(item => Guid.NewGuid()).ToList();

        for (int i = 0; i < possibleRewards; i++)
        {
            randList.Add(shuffledRewards.ElementAt(i));
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
