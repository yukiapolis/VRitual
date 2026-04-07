using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClickQuit : MonoBehaviour
{
    private Button btn;
    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(() => {
            Application.Quit();
            Debug.Log("退出程序");
        });
    }
}
