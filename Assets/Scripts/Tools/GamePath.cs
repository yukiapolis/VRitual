using System.IO;
using UnityEngine;

public class GamePath : Singleton<GamePath>
{
    public string FilePath = "";

    protected override void Init()
    {
        base.Init();
        FilePath = Application.platform==RuntimePlatform.WindowsEditor|| Application.platform == RuntimePlatform.WindowsPlayer ? Application.dataPath.Substring(0, Application.dataPath.IndexOf("Assets"))+ "/StreamingAssets" : UnityEngine.Application.persistentDataPath;
       
    }
}