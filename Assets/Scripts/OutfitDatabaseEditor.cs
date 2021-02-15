using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class OutfitDatabaseEditor : EditorWindow
{
    public OutfitDatabase outfitList;
    private int viewIndex = 1;

    [MenuItem("Window/OutfitList Editor %#e")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(OutfitDatabaseEditor));
    }

    private void OnEnable()
    {
        if (EditorPrefs.HasKey("ObjectPath"))
        {
            string objectPath = EditorPrefs.GetString("ObjectPath");
            outfitList = AssetDatabase.LoadAssetAtPath(objectPath, typeof(OutfitDatabase)) as OutfitDatabase;
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Outfit Editor", EditorStyles.boldLabel);
        if (outfitList != null)
        {
            if (GUILayout.Button("Show outfit list"))
            {
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = outfitList;
            }
        }

        if (GUILayout.Button("Open Outfit List"))
        {
            OpenOutfitList();
        }
        if (GUILayout.Button("New Outfit List"))
        {
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = outfitList;
        }
        GUILayout.EndHorizontal();

        if (outfitList == null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            if (GUILayout.Button("Create new Outfit List"))
            {
                CreateNewOutfitList();
            }
            if (GUILayout.Button("Open existing Outfit List"))
            {
                OpenOutfitList();
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(20);

        if (outfitList != null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);

            if (GUILayout.Button("Previous", GUILayout.ExpandWidth(false)))
            {
                if (viewIndex > 1)
                {
                    viewIndex--;
                }
            }
            if (GUILayout.Button("Next", GUILayout.ExpandWidth(false)))
            {
                if (viewIndex < outfitList.outfitList.Count)
                {
                    viewIndex++;
                }
            }

            GUILayout.Space(60);
            if (GUILayout.Button("Add outfit", GUILayout.ExpandWidth(false))) {
                AddOutfit();
            }
            if (GUILayout.Button("Remove outfit",GUILayout.ExpandWidth(false)))
            {
                DeleteOutfit(viewIndex - 1);
            }

            GUILayout.EndHorizontal();

            if (outfitList.outfitList == null)
            {
                Debug.Log("Outfit List empty");
            }
            if (outfitList.outfitList.Count > 0)
            {
                GUILayout.BeginHorizontal();
                viewIndex = Mathf.Clamp(EditorGUILayout.IntField("Current outfit", viewIndex, GUILayout.ExpandWidth(false)), 1, outfitList.outfitList.Count);
                EditorGUILayout.LabelField("of " + outfitList.outfitList.Count.ToString() + " outfits", "", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();

                outfitList.outfitList[viewIndex - 1].outfitId = EditorGUILayout.IntField("Outfit ID", outfitList.outfitList[viewIndex - 1].outfitId);
                outfitList.outfitList[viewIndex - 1].outfitName = EditorGUILayout.TextField("Outfit Name", outfitList.outfitList[viewIndex - 1].outfitName as string);
                outfitList.outfitList[viewIndex - 1].outfitPrefab = EditorGUILayout.ObjectField("Outfit Prefab", outfitList.outfitList[viewIndex - 1].outfitPrefab, typeof(GameObject), false) as GameObject;

                GUILayout.Space(10);
            } else
            {
                GUILayout.Label("Outfit list is empty");
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(outfitList);
            }
        }

        void CreateNewOutfitList()
        {
            viewIndex = 1;
            OutfitDatabase asset = ScriptableObject.CreateInstance<OutfitDatabase>();
            AssetDatabase.CreateAsset(asset, "Assets/Data/OutfitList.asset");
            AssetDatabase.SaveAssets();
            if (asset)
            {
                outfitList.outfitList = new List<OutfitDatabase.DBOutfit>();
                string relPath = AssetDatabase.GetAssetPath(asset);
                EditorPrefs.SetString("ObjectPath", relPath);
            }
        }

        void OpenOutfitList()
        {
            string absPath = EditorUtility.OpenFilePanel("Select Outfit List", "", "");
            if (absPath.StartsWith(Application.dataPath))
            {
                string relPath = absPath.Substring(Application.dataPath.Length - "Assets".Length);
                outfitList = AssetDatabase.LoadAssetAtPath(relPath, typeof(OutfitDatabase)) as OutfitDatabase;
                if (outfitList.outfitList == null)
                    outfitList.outfitList = new List<OutfitDatabase.DBOutfit>();
                if (outfitList)
                {
                    EditorPrefs.SetString("ObjectPath", relPath);
                }
            }
        }


        void AddOutfit()
        {
            OutfitDatabase.DBOutfit newOutfit = new OutfitDatabase.DBOutfit();
            newOutfit.outfitName = "New Outfit";
            newOutfit.outfitId = viewIndex + 1;
            outfitList.outfitList.Add(newOutfit);
            viewIndex = outfitList.outfitList.Count;
        }

        void DeleteOutfit(int index)
        {
            outfitList.outfitList.RemoveAt(index);
        }




    }
}
#endif
