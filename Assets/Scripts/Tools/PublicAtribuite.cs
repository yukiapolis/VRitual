using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PublicAtribuite : Singleton<PublicAtribuite>
{
    public string FilePath 
    {
        get =>Application.platform==RuntimePlatform.Android?Application.persistentDataPath:Application.dataPath+"/";
    }
    public Gender CurrentGender = Gender.Male;
}
public enum Gender 
{
    Male,//─ллн
    Woman//┼«лн
}