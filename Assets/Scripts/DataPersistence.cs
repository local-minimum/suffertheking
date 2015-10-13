using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class DataPersistence {

    public static void SaveData(System.Object o, string filename)
    {
        if (o is MonoBehaviour)
            throw new Exception("Shouldn't serialize monobehaviours");

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(FullPath(filename));

        bf.Serialize(file, o);
        file.Close();
    }

    public static T LoadData<T>(string filename)
    {
        if (HasSavedData(filename))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(FullPath(filename), FileMode.Open);
            var data = (T)bf.Deserialize(file);
            file.Close();
            return data;
        }
        throw new Exception("No file found");
    }

    public static bool HasSavedData(string filename)
    {
        return File.Exists(FullPath(filename));
    }

    static string FullPath(string filename)
    {
        return Application.persistentDataPath + "/" + filename;
    }

    static string ObjectPathInEditor(GameObject go)
    {
        string path = "/" + go.name;
        var t = go.transform;

        while (t.parent != null)
        {
            path = "/" + t.name + path;
            t = t.parent;
        }

        return path;
    }

    
    [Serializable]
    public class AssetReference
    {
        public string path;
        public int index;

        public T GetComponent<T>()
        {

            return GetGameObject().GetComponents<T>()[index];
        }

        public GameObject GetGameObject()
        {
            return GameObject.Find(path);
        }
    }

    public static AssetReference SerializableReference(MonoBehaviour mb)
    {
        var ar = new AssetReference();
        ar.path = ObjectPathInEditor(mb.gameObject);
        var sameTypes = mb.gameObject.GetComponents(mb.GetType());

        for (int i=0; i<sameTypes.Length; i++)
            if (sameTypes[i] == mb)
            {
                ar.index = i;
                break;
            }

        return ar;
    }

}
