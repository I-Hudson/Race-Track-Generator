using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(PGSave))]
public class PGSaveEditor : Editor
{
    //Ref to PGSave
    PGSave pgSave;

    //name of the folder create
    private string folderName = "";

    private void OnEnable()
    {
        //Get a ref to the PGSave object on the GameObject
        pgSave = (PGSave)target;
    }

    public override void OnInspectorGUI()
    {
        folderName = EditorGUILayout.TextField(new GUIContent("Folder Name"), folderName);

        if (GUILayout.Button("Save"))
        {
            if (folderName != "")
            {
                CheckForRootFolder();
                CreateFolder();
            }
            else
            {
                Debug.Log("PGSave: There must be a valid folder name.");
            }
        }
    }

    /// <summary>
    /// Check if a Race Tracks folder exists
    /// </summary>
    private void CheckForRootFolder()
    {
        if (!Directory.Exists(Application.dataPath + "/Race Tracks"))
        {
            Directory.CreateDirectory(Application.dataPath + "/Race Tracks");
        }
    }

    /// <summary>
    /// Create a new folder for this track
    /// This folder will store the prefab, track mesh and curb mesh
    /// </summary>
    private void CreateFolder()
    {
        //Setup the path to the new folder
        string path = Application.dataPath + "/Race Tracks/" + folderName;

        if (!Directory.Exists(path))
        {
            //Create the new folder
            Directory.CreateDirectory(path);
            SaveTrackMesh(folderName);
            SaveCurbMesh(folderName);
            SavePrefab(folderName);
            AssetDatabase.SaveAssets();
        }
        else
        {
            Debug.Log("PGSave: A folder with '" + folderName + "' already exists");
        }
    }

    /// <summary>
    /// Save the Track Mesh
    /// </summary>
    private void SaveTrackMesh(string path)
    {
        if (AssetDatabase.LoadAssetAtPath("Assets/Race Tracks/ " + path + " /TrackMesh.asset", typeof(Mesh)) == null)
        {
            Mesh meshToSave = pgSave.gameObject.GetComponent<MeshFilter>().sharedMesh;
            if (meshToSave)
            {
                AssetDatabase.CreateAsset(meshToSave, "Assets/Race Tracks/" + path + "/TrackMesh.asset");
            }
            else
            {
                Debug.Log("PGSave: Mesh To Save is null. Please check a mesh is present.");
            }
        }
        else
        {
            Debug.Log("PGSave: Asset already exists at path: " + path);
        }
    }

    /// <summary>
    /// Save the Curb Mesh
    /// </summary>
    private void SaveCurbMesh(string path)
    {
        if (AssetDatabase.LoadAssetAtPath("Assets/Race Tracks/ " + path + " /CurbMesh.asset", typeof(Mesh)) == null)
        {
            Mesh meshToSave = pgSave.gameObject.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;
            if (meshToSave)
            {
                AssetDatabase.CreateAsset(meshToSave, "Assets/Race Tracks/" + path + "/CurbMesh.asset");
            }
            else
            {
                Debug.Log("PGSave: Mesh To Save is null. Please check a mesh is present.");
            }
        }
        else
        {
            Debug.Log("PGSave: Asset already exists at path: " + path);
        }
    }

    /// <summary>
    /// Save the track GameObejct to a prefab
    /// </summary>
    /// <param name="path"></param>
    private void SavePrefab(string path)
    {
        if (AssetDatabase.LoadAssetAtPath("Assets/Race Tracks/" + path + "/Prefab.prefab", typeof(GameObject)) == null)
        {
            Object prefab = PrefabUtility.CreateEmptyPrefab("Assets/Race Tracks/" + path + "/Prefab.prefab");
            GameObject objectToSave = Instantiate(pgSave.gameObject);
            DestroyImmediate(objectToSave.GetComponent<PGSave>());

            PrefabUtility.ReplacePrefab(objectToSave, prefab);
            DestroyImmediate(objectToSave);
        }
    }
}
