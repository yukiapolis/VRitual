using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChoosePanel : MonoSingleton<ChoosePanel>
{
    public Dictionary<string, bool> ChoiceDic = new Dictionary<string, bool>();
    public GameObject ChoiceGo;
    public GameObject ItemS;
    public Transform Content;
    public TextMeshProUGUI QText;
    public TextMeshProUGUI TipText,TypeText;
    public Button Submit;
    public string rightAns = "";
    public string myAns = "";

    private ChooseInfo ci;
   
    protected  void Awake()
    {
        Submit.onClick.AddListener(SubmitAns);
        TipText.gameObject.SetActive(false);
        ChoiceGo.SetActive(false);


    }
    /// <summary>
    /// 显示选择题
    /// </summary>
    /// <param name="qStr"></param>
    /// <param name="tmpDict"></param>
    public void ShowChoose(ChooseInfo chooseInfo)
    {
        ci = chooseInfo;
        rightAns = "";
        myAns = "";
        ChoiceDic.Clear();
        TipText.gameObject.SetActive(false);
        ChoiceGo.SetActive(true);
        QText.text = chooseInfo.qStr;
        for (int i = 0; i < Content.childCount; i++)
        {
            Destroy(Content.GetChild(i).gameObject);
        }
        foreach (var item in chooseInfo.tmpDict)
        {
            GameObject go = Instantiate(ItemS, Content);
            go.SetActive(true);
            go.GetComponentInChildren<OptionItem>().SetInfo(item.Key);
            if (chooseInfo.isSingle)
            {
                go.GetComponent<Toggle>().group = Content.GetComponent<ToggleGroup>();
                TypeText.text = "单选";
            }
            else
            {
                TypeText.text = "多选";
            }
            ChoiceDic.Add(item.Key.Split('.')[1], item.Value);
            if (item.Value)
            {
                string[] str = item.Key.Split('.');
                if (str.Length > 0)
                    rightAns += str[0];
            }
        }
    }
    /// <summary>
    /// 判断是否正确
    /// </summary>
    /// <returns></returns>
    public  bool SubmitChoose()
    {
        myAns = "";
        for (int i = 0; i < Content.childCount; i++)
        {
            var option = Content.GetChild(i).GetComponent<OptionItem>();
            if (option.ToggleState())
                myAns += option.OptionStr;
        }
        Debug.Log($"我的答案：{myAns},正确答案：{rightAns}");
        if (myAns == rightAns)
            return true;
        else
            return false;
    }
    /// <summary>
    /// 确认提交
    /// </summary>
    public  void SubmitAns()
    {
        if (SubmitChoose())
        {
           HideGO();
           if (ci.a != null)
               ci.a?.Invoke();
        }
        else
        {
           TipText.gameObject.SetActive(true);
           TipText.text = "答案错误,正确答案为" + rightAns;
        }
    }
    public  void HideGO()
    {
        ChoiceGo.SetActive(false);
        ChoiceDic.Clear();
        rightAns = "";
    }
    protected  void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && rightAns != ""&& ChoiceGo.activeInHierarchy)
        {
            SubmitAns();
        }
    }
   
}
public class ChooseInfo
{
    public ChooseInfo()
        {
        tmpDict = new Dictionary<string, bool>();  
}
    /// <summary>
    /// 选择题问题
    /// </summary>
    public string qStr { get; set; }
    public Action a { get; set; }
    public bool isSingle { get; set; }
    public Dictionary<string, bool> tmpDict { get; set; }
}

