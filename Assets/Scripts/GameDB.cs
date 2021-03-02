using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameDB : MonoBehaviour
{

    public List<GameObject> outfitList = new List<GameObject>();
    public List<Sprite> packageRarities = new List<Sprite>();

    private List<int> randList = new List<int>();

    public FirebaseManager DBManager;
    public NetworkManagement network;

    public float expRate = 3 / 2;

    public OutfitDatabase outfitDB;

    public Camera mainCamera, rewardCamera;

    public RewardRoom rewardRoomScript;
    public static MainUI mainUI;

    public GameObject destinationMarker;
    public float chatProximity;

    public Gradient rarityColors;
    public Button loginBtn;
    public Text loginResponse;




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
    [ClientCallback]
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "MainGame")
        {
            if (rewardRoomScript == null)
            {
                rewardRoomScript = FindObjectOfType<RewardRoom>();
            }
            if (mainUI == null)
            {
                mainUI = FindObjectOfType<MainUI>();
            }
            if (mainCamera == null || rewardCamera == null)
            {
                CameraIdentifier[] camList = FindObjectsOfType<CameraIdentifier>();
                foreach (CameraIdentifier cam in camList)
                {
                    if (cam.camName == "MainCamera")
                    {
                        mainCamera = cam.GetComponent<Camera>();
                    }
                    if (cam.camName == "RewardCamera")
                    {
                        rewardCamera = cam.GetComponent<Camera>();
                    }
                }
            }
        }
    }

    public List<int> GetRandomOutfits(int amount)
    {
        randList = new List<int>();
        List<int> playerOutfits = new List<int>();
        if (NetworkClient.connection.identity != null)
        {
            playerOutfits = NetworkClient.connection.identity.GetComponent<Player>().outfits;
        } else
        {
            playerOutfits = new List<int>();
        }
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

    public void SwitchToMainCamera()
    {
        rewardCamera.enabled = false;
        mainCamera.enabled = true;
    }

    public void SwitchToRewardCamera()
    {
        mainCamera.enabled = false;
        rewardCamera.enabled = true;
    }

    public static Vector3 RandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
            UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
            UnityEngine.Random.Range(bounds.min.z, bounds.max.z)
        );
    }

}
