using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardRoom : MonoBehaviour
{
    public GameObject LootBoxContainer;
    public GameObject OutfitContainer;

    public OutfitDatabase outfitDB;

    public Animator chestAnim;


    private void Update()
    {
        if (Camera.main.name == "RewardCamera")
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(r, out hit))
                {
                    if (hit.transform == LootBoxContainer.transform)
                    {
                        Debug.Log("Opening lootbox");
                        chestAnim.SetTrigger("Open");
                        StartCoroutine(BoxClick());
                    }
                }
            }
        }
    }


    public void ResetRoom()
    {
        chestAnim.SetTrigger("Close");
        LootBoxContainer.SetActive(true);
        OutfitContainer.SetActive(false);
        
    }

    public IEnumerator BoxClick()
    {
        LootBoxContainer.GetComponentInChildren<ParticleSystem>().Play();
        yield return new WaitForSeconds(2f);
        LootBoxContainer.SetActive(false);
        GenerateRewards();
        OutfitContainer.SetActive(true);
        chestAnim.SetTrigger("Close");
        
        
    }


    public void GenerateRewards()
    {
        List<int> generatedList = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            float lootSeed = Random.Range(1f, 100f);
            List<OutfitDatabase.DBOutfit> possibleRewards = new List<OutfitDatabase.DBOutfit>();
            foreach(OutfitDatabase.DBOutfit o in outfitDB.outfitList)
            {
                float outfitPercent = o.outfitPrefab.GetComponent<Outfit>().lootPercent * 100f;
                if (lootSeed < outfitPercent)
                {
                    possibleRewards.Add(o);
                }
            }
            OutfitDatabase.DBOutfit randomOutfit;
            if (possibleRewards.Count > 1)
            {
                randomOutfit = possibleRewards[Random.Range(0, possibleRewards.Count - 1)];
            } else
            {
                randomOutfit = possibleRewards[0];
            }
            generatedList.Add(randomOutfit.outfitId);

        }
        int j = 0;
        foreach (Transform t in OutfitContainer.transform)
        {
            RewardRoomOutfit sc = t.GetComponent<RewardRoomOutfit>();
            if (sc != null)
            {
                sc.ClearContainer();
                sc.InsertOutfit(generatedList[j]);
                j++;
            }
        }
    }

    public static int ClosestIndex(List<OutfitDatabase.DBOutfit> outfits, float target)
    {
        float min_difference = float.MaxValue;
        int index = 0;
        for(int i = 0; i < outfits.Count; i++)
        {
            float diff = Mathf.Abs(target - (outfits[i].outfitPrefab.GetComponent<Outfit>().lootPercent * 100f));
            if (diff < min_difference)
            {
                min_difference = diff;
                index = i;
            }
        }
        return index;
    }
}
