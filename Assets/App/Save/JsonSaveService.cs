using System.IO;
using UnityEngine;

public static class JsonSaveService
{
    static string Path => System.IO.Path.Combine(Application.persistentDataPath, "aq30_save.json");

    public static bool HasSave() => File.Exists(Path);

    public static void Save(SaveBlob blob){
        try{
            var json = JsonUtility.ToJson(blob, prettyPrint:true);
            File.WriteAllText(Path, json);
            Debug.Log("[Save] Wrote " + Path);
        }catch(System.Exception ex){
            Debug.LogError("[Save] Write failed: " + ex.Message);
        }
    }

    public static SaveBlob Load(){
        try{
            if(!File.Exists(Path)) return null;
            var json = File.ReadAllText(Path);
            var blob = JsonUtility.FromJson<SaveBlob>(json);
            return blob;
        }catch(System.Exception ex){
            Debug.LogWarning("[Save] Load failed: " + ex.Message);
            return null;
        }
    }

    public static void Clear(){
        try{
            if(File.Exists(Path)) File.Delete(Path);
            Debug.Log("[Save] Cleared save");
        }catch(System.Exception ex){
            Debug.LogWarning("[Save] Clear failed: " + ex.Message);
        }
    }
}
