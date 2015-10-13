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

}
