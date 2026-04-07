using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StepBase : MonoBehaviour
{
    [Tooltip("步骤数据")]
    public StepData SD;

    [Header("提醒配置")]
    [SerializeField] protected bool enableStepReminder = true;
    [SerializeField] protected float reminderDelay = 8f;
    [SerializeField] protected string reminderButtonText = "下一步";

    protected int StepIndex = 0;
    private Coroutine reminderCoroutine;
    private float duration=0;
    private void Start()
    {
    }
    /// <summary>
    /// 初始化该步骤内容
    /// </summary>
    public virtual void Init() 
    {
    
    }
    /// <summary>
    /// 步骤开始
    /// </summary>
    public virtual void OnStart() 
    {
        CancelReminder();
        StepIndex = -1;
        SetStep();
    }
    protected virtual void SetStep()
    {
        CancelReminder();
        StepIndex++;
        Debug.Log(StepIndex);

    }
    /// <summary>
    /// 步骤结束调用
    /// </summary>
    public virtual void OnEnd()
    {
        CancelReminder();
        StepAffirmPanel.Instance.Register(reminderButtonText,()=> {
            StepController.Instance.NextStep();
        });
    }

    protected void RegisterStepReminder(Action action)
    {
        if (!enableStepReminder || action == null)
        {
            return;
        }

        CancelReminder();
        reminderCoroutine = StartCoroutine(ShowReminderAfterDelay(reminderDelay, action));
    }

    protected void CancelReminder()
    {
        if (reminderCoroutine == null)
        {
            return;
        }

        StopCoroutine(reminderCoroutine);
        reminderCoroutine = null;
    }
    /// <summary>
    /// 延时触发
    /// </summary>
    /// <param name="t"></param>
    /// <param name="callback"></param>
    protected void DelayedTrigger(float t, Action callback) 
    {
        StartCoroutine(WaitTimeDo(t,callback));
    }
    /// <summary>
    /// 等待某段时间后做某事
    /// </summary>
    /// <param name="t"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    private IEnumerator WaitTimeDo(float t,Action callback) 
    {
        yield return new WaitForSeconds(t);
        callback?.Invoke();
    }

    private IEnumerator ShowReminderAfterDelay(float t, Action callback)
    {
        yield return new WaitForSeconds(t);
        reminderCoroutine = null;
        StepAffirmPanel.Instance.Register(reminderButtonText, () => {
            CancelReminder();
            callback?.Invoke();
        });
    }
    /// <summary>
    /// 设置跟随
    /// </summary>
    /// <param name="current">当前物体</param>
    /// <param name="parent">跟随的目标物体</param>
    protected void SetFollow(Transform current,Transform parent) 
    {
            current.SetParent(parent);
            current.localPosition = Vector3.zero;
            current.localEulerAngles = Vector3.zero;
    }
}
[Serializable]
public class StepData 
{
    public int id;
    public string name;
    public string content;
}
