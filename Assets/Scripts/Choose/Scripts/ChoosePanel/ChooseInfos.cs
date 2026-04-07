using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[SerializeField]
public class ChooseInfos : MonoBehaviour
{
    public AudioClip clip;
    public ChooseGroup _chooseGroup;
    private List<Choose> Choose;
    private List<ChooseInfo> chooseInfos;
    private void Awake()
    {
    }
    public void StartChoose(int index=0)
    {
        StartAnswer(index, () =>
        {
         Debug.Log("你的所有答题已经完成");
        });
    }
    /// <summary>
    /// 开始答题
    /// </summary>
    /// <param name="groupIndex">试题组索引</param>
    /// <param name="callback">回调</param>
    public void StartAnswer(int groupIndex=0,Action callback=null)
    {
        if (groupIndex < 0 || groupIndex > _chooseGroup.group.Count - 1) return;
        Choose = _chooseGroup.group[groupIndex].list;
        chooseInfos = new List<ChooseInfo>();
        for (int i = 0; i < Choose.Count; i++)
        {
            int tempInt = i;
            ChooseInfo ci = new ChooseInfo();
            ci.qStr = Choose[i].QStr;
            ci.isSingle = Choose[i].isSingle;
            foreach (var temp in Choose[i].chooseItems)
            {
                ci.tmpDict.Add(temp.str, temp.IsRight);
            }
            if (i < Choose.Count - 1)
            {
                ci.a = () => { ChoosePanel.Instance.ShowChoose(chooseInfos[tempInt + 1]); };
            }
            else if (i == Choose.Count - 1)
            {
                ci.a = () => {
                    Debug.Log("本步骤所有答题已完成");
                    AudioManager.Instance.AS.PlayOneShot(clip);
                    callback?.Invoke();
                };
            }
            chooseInfos.Add(ci);
        }
        if (chooseInfos.Count > 0)
        {
            ChoosePanel.Instance.ShowChoose(chooseInfos[0]);
        }
    }
}
[Serializable]
public class ChooseGroup
{
    public List<ChooseList> group;
}
[Serializable]
public class ChooseList
{
    public List<Choose> list;
}
[Serializable]
public class Choose
{

    /// <summary>
    /// 问题
    /// </summary>
    public string QStr;
    public bool isSingle;
    public List<ChooseItem> chooseItems;
}
[Serializable]
public class ChooseItem
{
    public string str;
    public bool IsRight;
}
