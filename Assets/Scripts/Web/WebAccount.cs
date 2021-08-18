using UnityEngine;

[System.Serializable]
public class WebAccount
{
    public string username;
    public string password;

    public string ToJsonString() 
    {
        return JsonUtility.ToJson(this);
    }
}
