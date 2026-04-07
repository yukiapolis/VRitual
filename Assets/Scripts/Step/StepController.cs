using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepController : MonoSingleton<StepController>
{
    private int stepIndex = 0;
    /// <summary>
    /// 步骤改变事件
    /// </summary>
    public Action<StepBase> StepChangAction;
    /// <summary>
    /// 开始事件/结束事件
    /// </summary>
    public Action<List<StepBase>> StartAction;
    public Action EndAction;
    /// <summary>
    /// 步骤列表
    /// </summary>
    public List<StepBase> StepList;
    private StepBase currentStep;
    public StepBase CurrentStep => currentStep;
    public GameObject EndGO;
    private void Awake()
    {
    }
    private void Start()
    {
        stepIndex = -1;
        InitStep();
    }
    private void OnEnable()
    {
        EndAction += OnStepEnd;
    }
    /// <summary>
    /// 步骤完成调用
    /// </summary>
    private void OnStepEnd()
    {
        Debug.Log("所有流程已经完成"); 
    }

    private void OnDisable()
    {
        EndAction -= OnStepEnd;
    }
    /// <summary>
    /// 初始化步骤
    /// </summary>
    public void InitStep() 
    {
        StartAction?.Invoke(StepList);
        stepIndex = 0;
        OnStepChange(stepIndex);
    }
    /// <summary>
    /// 切换步骤
    /// </summary>
    /// <param name="stepindex"></param>
    public void ChangeStep(int stepindex) 
    {
        stepIndex = stepindex;
        OnStepChange(stepIndex);
    }
    /// <summary>
    /// 下一步骤
    /// </summary>
    public void NextStep() 
    {
        Debug.Log("下一个步骤");
        stepIndex++;
        OnStepChange(stepIndex);
    }
    /// <summary>
    /// 步骤改变事件
    /// </summary>
    private void OnStepChange(int stepindex) 
    {
        stepIndex = stepindex;

        if (stepindex >= StepList.Count) 
        {
            EndAction?.Invoke();
            return;
        }

        for (int i = 0; i < StepList.Count; i++)
        {
            StepBase step = StepList[i];
            string stepName = step != null ? step.name : "null";
            string goName = step != null ? step.gameObject.name : "null";
            string typeName = step != null ? step.GetType().Name : "null";
            Debug.Log($"StepList[{i}] -> component={stepName}, gameObject={goName}, type={typeName}");
        }

        currentStep = StepList[stepindex];
        Debug.Log($"切换到步骤: index={stepindex}, component={currentStep.name}, gameObject={currentStep.gameObject.name}, type={currentStep.GetType().Name}");
        currentStep.OnStart();
        StepChangAction?.Invoke(currentStep);
        //ItemInfoPanel.Ins.ShowInfo(true,new InfoItemData() {title= currentStep .SD.name,info= currentStep .SD.content});
    }
}