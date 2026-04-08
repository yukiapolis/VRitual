using UnityEngine;

public class State_Greeting : StepBase
{
    [Header("主持人")]
    public NPCNavController NPC_Male;
    public NPCNavController NPC_Woman;

    [Header("男性互动 IK")]
    public UniversalIKController MaleIK;

    [Header("问候点位")]
    public Transform MaleBehindChairPoint;
    public Transform FemaleReturnPoint;
    public Transform MaleReturnPoint;
    public Transform Head;
    public Transform LeftJB;
    public Transform RightJB;
    public Transform FemaleLeftFootTarget;
    public Transform FemaleRightFootTarget;

    [Header("男性回应判定")]
    public float requiredBowForwardY = -0.35f;
    public float responseCheckInterval = 0.15f;
    public bool requirePlayerGreetingResponse = true;

    [Header("男性动态目标点")]
    public float maleFrontTargetDistance = 0.8f;
    public float maleFrontTargetHeightOffset = -0.2f;
    public float maleContactTargetDistance = 0.18f;
    public float maleContactTargetHeightOffset = -0.03f;

    [Header("时序")]
    public float hostWalkLeadDelay = 0.2f;
    public float inviteStandDelay = 1f;
    public float femaleFootTouchDuration = 2f;
    public float femaleObserveFeetDuration = 1.5f;
    public float maleForeheadContactDuration = 3f;
    public float returnSettleDelay = 1.5f;

    [Header("女性邀请动作")]
    public string femaleInviteSitTriggerName = "InviteSit";

    [Header("女性动态目标点")]
    public float femaleFrontTargetDistance = 0.85f;
    public float femaleFrontTargetHeightOffset = 0f;
    public float femaleFootTouchBackOffset = 0.18f;
    public float femaleFootTouchSideOffset = 0f;

    private Animator maleAni;
    private Animator womanAni;
    private GameObject maleFrontRuntimeTarget;
    private GameObject maleContactRuntimeTarget;
    private GameObject femaleFrontRuntimeTarget;
    private GameObject femaleFootRuntimeTarget;

    private void Awake()
    {
        maleAni = NPC_Male != null ? NPC_Male.GetComponent<Animator>() : null;
        womanAni = NPC_Woman != null ? NPC_Woman.GetComponent<Animator>() : null;
        EnsureMaleRuntimeTargets();
    }

    public override void OnStart()
    {
        Debug.Log($"State_Greeting.OnStart: gender={(PublicAtribuite.Instance != null ? PublicAtribuite.Instance.CurrentGender.ToString() : "Unknown")}");
        base.OnStart();
    }

    protected override void SetStep()
    {
        base.SetStep();

        switch (StepIndex)
        {
            case 0:
                if (PublicAtribuite.Instance != null && PublicAtribuite.Instance.CurrentGender == Gender.Woman)
                {
                    Debug.Log("State_Greeting: 进入女性玩家问候流程。");
                    RunFemaleGreeting();
                }
                else
                {
                    Debug.Log("State_Greeting: 进入男性玩家问候流程。");
                    RunMaleGreeting();
                }
                break;
        }
    }

    private void RunFemaleGreeting()
    {
        if (maleAni == null || womanAni == null)
        {
            Debug.LogError("State_Greeting.RunFemaleGreeting: 缺少主持人 Animator，无法执行问候流程。");
            return;
        }

        maleAni.SetBool("Sit", false);
        womanAni.SetBool("SitDown", false);
        Debug.Log("State_Greeting.RunFemaleGreeting: 先让男女主持起身。");

        DelayedTrigger(1.2f, () => {
            MoveHost(NPC_Male, maleAni, MaleBehindChairPoint, "Walk", "MaleBehindChairPoint", () => {
                Debug.Log("State_Greeting.RunFemaleGreeting: 男主持已到椅子后方。");
            });
        });

        DelayedTrigger(hostWalkLeadDelay, () => {
            UpdateFemaleRuntimeTarget();
            MoveHost(NPC_Woman, womanAni, femaleFrontRuntimeTarget != null ? femaleFrontRuntimeTarget.transform : null, "Walk", "FemaleFrontRuntimeTarget", () => {
                FaceHostTowardPlayer(NPC_Woman, "RunFemaleGreeting-FrontOfPlayer");
                Debug.Log("State_Greeting.RunFemaleGreeting: 女主持已到玩家前方，准备引导玩家起身。");

                DelayedTrigger(inviteStandDelay, () => {
                    Debug.Log("State_Greeting FemaleStandUp START: 准备触发女性玩家站起，command=ZL");

                    if (PlayerController.Instance == null)
                    {
                        Debug.LogError("State_Greeting FemaleStandUp ERROR: PlayerController.Instance 为 null，无法触发女性玩家站起。");
                        return;
                    }

                    Debug.Log($"State_Greeting FemaleStandUp BEFORE DirectStand: poseController={(PlayerController.Instance.poseController != null ? PlayerController.Instance.poseController.name : "null")}, playerAnimator={(PlayerController.Instance.playerAnimator != null ? PlayerController.Instance.playerAnimator.name : "null")}, currentPose={(PlayerController.Instance.poseController != null ? PlayerController.Instance.poseController.CurrentPose.ToString() : "null")}");

                    Transform femaleStandAnchor = GameObject.Find("Female_PlayerStandTra")?.transform;
                    if (PlayerController.Instance.poseController != null)
                    {
                        Debug.Log($"State_Greeting FemaleStandUp DIRECT MoveToPose: anchor={(femaleStandAnchor != null ? femaleStandAnchor.name : "null")}, anchorPos={(femaleStandAnchor != null ? femaleStandAnchor.position.ToString() : "null")}");
                        PlayerController.Instance.poseController.MoveToPose(PlayerPoseController.PlayerPose.Stand, femaleStandAnchor, () => {
                            Debug.Log($"State_Greeting FemaleStandUp CALLBACK: Direct MoveToPose(Stand) 已回调，currentPose={(PlayerController.Instance.poseController != null ? PlayerController.Instance.poseController.CurrentPose.ToString() : "null")}, playerAnimator={(PlayerController.Instance.playerAnimator != null ? PlayerController.Instance.playerAnimator.name : "null")}");
                            Debug.Log("State_Greeting.RunFemaleGreeting: 女性玩家已站起，开始进入脚部互动。");

                            Debug.Log("State_Greeting.RunFemaleGreeting: 女性玩家站起后，女主持不再二次走位，直接在当前位置进入脚部互动。");
                            AlignFemaleHostTowardFeet();
                            womanAni.SetBool("DunXia", true);

                            DelayedTrigger(femaleFootTouchDuration, () => {
                                Debug.Log("State_Greeting.RunFemaleGreeting: 触脚动作进行中，给玩家低头观察时间。");
                                AlignFemaleHostTowardFeet();

                                DelayedTrigger(femaleObserveFeetDuration, () => {
                                    womanAni.SetBool("DunXia", false);
                                    Debug.Log($"State_Greeting.RunFemaleGreeting: 摸脚动作结束，触发女主持 InviteSit={femaleInviteSitTriggerName} 示意玩家回到地坐。");
                                    womanAni.SetTrigger(femaleInviteSitTriggerName);

                                    DelayedTrigger(0.8f, () => {
                                        PlayerController.Instance.MoveCameraToPosition("DX", () => {
                                            Debug.Log("State_Greeting.RunFemaleGreeting: 女性玩家已回到地坐姿态，主持人返回原位。");
                                            ReturnFemaleGreetingHosts();
                                        });
                                    });
                                });
                            });
                        });
                    }
                    else
                    {
                        Debug.LogError("State_Greeting FemaleStandUp ERROR: poseController 为空，无法直接让女性玩家站起。");
                    }
                });
            });
        });
    }

    private void ReturnFemaleGreetingHosts()
    {
        MoveHost(NPC_Woman, womanAni, FemaleReturnPoint, "Walk", "FemaleReturnPoint", () => {
            womanAni.SetBool("SitDown", true);
            Debug.Log("State_Greeting.ReturnFemaleGreetingHosts: 女主持已回位并坐下。");
        });

        MoveHost(NPC_Male, maleAni, MaleReturnPoint, "Walk", "MaleReturnPoint", () => {
            maleAni.SetBool("Sit", true);
            Debug.Log("State_Greeting.ReturnFemaleGreetingHosts: 男主持已回位并坐下。");

            DelayedTrigger(returnSettleDelay, () => {
                Debug.Log("State_Greeting.ReturnFemaleGreetingHosts: 女性问候流程完成，进入下一步。");
                OnEnd();
            });
        });
    }

    private void RunMaleGreeting()
    {
        if (maleAni == null)
        {
            Debug.LogError("State_Greeting.RunMaleGreeting: 缺少男主持 Animator，无法执行问候流程。");
            return;
        }

        maleAni.SetBool("Sit", false);
        Debug.Log("State_Greeting.RunMaleGreeting: 先让男主持起身。");

        DelayedTrigger(1.2f, () => {
            UpdateMaleRuntimeTargets();
            MoveHost(NPC_Male, maleAni, maleFrontRuntimeTarget != null ? maleFrontRuntimeTarget.transform : null, "Walk", "MaleFrontRuntimeTarget", () => {
                Debug.Log("State_Greeting.RunMaleGreeting: 男主持已到玩家面前，做抬小臂示意玩家起身。");
                maleAni.SetTrigger("TaiShou");

                DelayedTrigger(inviteStandDelay, () => {
                    Debug.Log("State_Greeting.RunMaleGreeting: 开始触发男玩家站起。");
                    PlayerController.Instance.MoveCameraToPosition("ZL", () => {
                        Debug.Log("State_Greeting.RunMaleGreeting: 男玩家已站起，开始肩搭肩与额头靠近。");
                        StartMaleContactGesture();
                    });
                });
            });
        });
    }

    private void StartMaleContactGesture()
    {
        if (MaleIK == null)
        {
            Debug.LogWarning("State_Greeting.StartMaleContactGesture: 未绑定 MaleIK，将跳过肩搭肩 IK，仅保留时序。");
            MoveMaleToForeheadContactPointAndContinue();
            return;
        }

        MaleIK.SetHeadTarget(Head);
        MaleIK.SetHandTarget(true, RightJB);
        MaleIK.SetHandTarget(false, LeftJB);
        Debug.Log($"State_Greeting.StartMaleContactGesture: 已启用 MaleIK，Head={(Head != null ? Head.name : "null")}, RightJB={(RightJB != null ? RightJB.name : "null")}, LeftJB={(LeftJB != null ? LeftJB.name : "null")}");

        MoveMaleToForeheadContactPointAndContinue();
    }

    private void MoveMaleToForeheadContactPointAndContinue()
    {
        UpdateMaleRuntimeTargets();
        if (maleContactRuntimeTarget == null)
        {
            Debug.LogWarning("State_Greeting.MoveMaleToForeheadContactPointAndContinue: 未创建 maleContactRuntimeTarget，将在当前点位直接执行接触时序。");
            CompleteMaleContactGesture();
            return;
        }

        MoveHost(NPC_Male, maleAni, maleContactRuntimeTarget.transform, "Walk", "MaleContactRuntimeTarget", () => {
            Debug.Log($"State_Greeting.MoveMaleToForeheadContactPointAndContinue: 男主持已前探到额头接触动态目标点 {maleContactRuntimeTarget.name}。");
            CompleteMaleContactGesture();
        });
    }

    private void CompleteMaleContactGesture()
    {
        if (!requirePlayerGreetingResponse)
        {
            Debug.LogWarning("State_Greeting.CompleteMaleContactGesture: 已关闭玩家回应要求，将退回固定时长等待。");
            DelayedTrigger(maleForeheadContactDuration, FinishMaleGreetingAfterResponse);
            return;
        }

        PlayerPoseController poseController = PlayerController.Instance != null ? PlayerController.Instance.poseController : null;
        if (poseController == null)
        {
            Debug.LogWarning("State_Greeting.CompleteMaleContactGesture: 未找到 PlayerPoseController，无法等待玩家回应，改用固定时长。");
            DelayedTrigger(maleForeheadContactDuration, FinishMaleGreetingAfterResponse);
            return;
        }

        poseController.PlayGreetingResponse();
        Debug.Log("State_Greeting.CompleteMaleContactGesture: 已触发玩家回应动作，开始等待玩家低头回应。");
        WaitForMalePlayerResponse();
    }

    private void WaitForMalePlayerResponse()
    {
        PlayerPoseController poseController = PlayerController.Instance != null ? PlayerController.Instance.poseController : null;
        if (poseController == null)
        {
            Debug.LogWarning("State_Greeting.WaitForMalePlayerResponse: 未找到 PlayerPoseController，直接结束问候。");
            FinishMaleGreetingAfterResponse();
            return;
        }

        bool bowed = poseController.IsHeadBowedForGreeting(requiredBowForwardY);
        if (bowed)
        {
            Debug.Log("State_Greeting.WaitForMalePlayerResponse: 玩家已低头回应，结束男性问候。");
            FinishMaleGreetingAfterResponse();
            return;
        }

        Debug.Log("State_Greeting.WaitForMalePlayerResponse: 玩家尚未低头回应，继续等待。");
        DelayedTrigger(responseCheckInterval, WaitForMalePlayerResponse);
    }

    private void FinishMaleGreetingAfterResponse()
    {
        if (MaleIK != null)
        {
            MaleIK.SetHeadTarget(Head, false);
            MaleIK.SetHandTarget(true, RightJB, false);
            MaleIK.SetHandTarget(false, LeftJB, false);
            Debug.Log("State_Greeting.FinishMaleGreetingAfterResponse: 已关闭 MaleIK 肩搭肩 / 额头接触动作。");
        }

        maleAni.SetTrigger("TaiShou");
        Debug.Log("State_Greeting.FinishMaleGreetingAfterResponse: 男主持完成非语言提示，准备让玩家重新坐下。");

        DelayedTrigger(0.8f, () => {
            Debug.Log("State_Greeting.FinishMaleGreetingAfterResponse: 开始触发男玩家重新坐下。");
            PlayerController.Instance.MoveCameraToPosition("ZX", () => {
                Debug.Log("State_Greeting.FinishMaleGreetingAfterResponse: 男玩家坐下完成，男主持开始走回原位。");
                ReturnMaleGreetingHost();
            });
        });
    }

    private void ReturnMaleGreetingHost()
    {
        MoveHost(NPC_Male, maleAni, MaleReturnPoint, "Walk", "MaleReturnPoint", () => {
            maleAni.SetBool("Sit", true);
            Debug.Log("State_Greeting.ReturnMaleGreetingHost: 男主持已回位并坐下。");

            DelayedTrigger(returnSettleDelay, () => {
                Debug.Log("State_Greeting.ReturnMaleGreetingHost: 男性问候流程完成，进入下一步。");
                OnEnd();
            });
        });
    }

    private void EnsureMaleRuntimeTargets()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (maleFrontRuntimeTarget == null)
        {
            maleFrontRuntimeTarget = new GameObject("MaleGreetingFrontRuntimeTarget");
            maleFrontRuntimeTarget.hideFlags = HideFlags.HideInHierarchy;
        }

        if (maleContactRuntimeTarget == null)
        {
            maleContactRuntimeTarget = new GameObject("MaleGreetingContactRuntimeTarget");
            maleContactRuntimeTarget.hideFlags = HideFlags.HideInHierarchy;
        }

        if (femaleFrontRuntimeTarget == null)
        {
            femaleFrontRuntimeTarget = new GameObject("FemaleGreetingFrontRuntimeTarget");
            femaleFrontRuntimeTarget.hideFlags = HideFlags.HideInHierarchy;
        }

        if (femaleFootRuntimeTarget == null)
        {
            femaleFootRuntimeTarget = new GameObject("FemaleGreetingFootRuntimeTarget");
            femaleFootRuntimeTarget.hideFlags = HideFlags.HideInHierarchy;
        }
    }

    private void UpdateMaleRuntimeTargets()
    {
        EnsureMaleRuntimeTargets();

        Transform trackedHead = PlayerController.Instance != null ? PlayerController.Instance.controlledCamera : null;
        Transform rigRoot = PlayerController.Instance != null ? PlayerController.Instance.playerRigRoot : null;
        if (trackedHead == null)
        {
            Debug.LogWarning("State_Greeting.UpdateMaleRuntimeTargets: 未找到 trackedHead，无法生成男性动态目标点。");
            return;
        }

        Vector3 flatForward = rigRoot != null ? rigRoot.forward : trackedHead.forward;
        flatForward.y = 0f;
        if (flatForward.sqrMagnitude < 0.0001f)
        {
            flatForward = trackedHead.forward;
            flatForward.y = 0f;
        }
        if (flatForward.sqrMagnitude < 0.0001f)
        {
            flatForward = Vector3.forward;
        }
        flatForward.Normalize();

        Vector3 basePosition = rigRoot != null ? rigRoot.position : trackedHead.position;

        Vector3 frontTargetPos = basePosition + flatForward * maleFrontTargetDistance;
        frontTargetPos.y = NPC_Male != null ? NPC_Male.transform.position.y : 0f;
        maleFrontRuntimeTarget.transform.position = frontTargetPos;
        maleFrontRuntimeTarget.transform.rotation = Quaternion.LookRotation(-flatForward, Vector3.up);

        Vector3 contactTargetPos = trackedHead.position + flatForward * maleContactTargetDistance;
        contactTargetPos.y = trackedHead.position.y + maleContactTargetHeightOffset;
        maleContactRuntimeTarget.transform.position = contactTargetPos;
        maleContactRuntimeTarget.transform.rotation = Quaternion.LookRotation(-flatForward, Vector3.up);

        Debug.Log($"State_Greeting.UpdateMaleRuntimeTargets: head={trackedHead.name}, headPos={trackedHead.position}, rigRoot={(rigRoot != null ? rigRoot.name : "null")}, rigPos={(rigRoot != null ? rigRoot.position.ToString() : "null")}, npcMaleY={(NPC_Male != null ? NPC_Male.transform.position.y.ToString("F3") : "null")}, forward={flatForward}, frontTargetPos={maleFrontRuntimeTarget.transform.position}, contactTargetPos={maleContactRuntimeTarget.transform.position}");
    }

    private void UpdateFemaleRuntimeTarget()
    {
        if (femaleFrontRuntimeTarget == null)
        {
            EnsureMaleRuntimeTargets();
        }

        Transform trackedHead = PlayerController.Instance != null ? PlayerController.Instance.controlledCamera : null;
        Transform rigRoot = PlayerController.Instance != null ? PlayerController.Instance.playerRigRoot : null;
        if (trackedHead == null)
        {
            Debug.LogWarning("State_Greeting.UpdateFemaleRuntimeTarget: 未找到 trackedHead，无法生成女性前方动态目标点。");
            return;
        }

        Vector3 flatForward = rigRoot != null ? rigRoot.forward : trackedHead.forward;
        flatForward.y = 0f;
        if (flatForward.sqrMagnitude < 0.0001f)
        {
            flatForward = trackedHead.forward;
            flatForward.y = 0f;
        }
        if (flatForward.sqrMagnitude < 0.0001f)
        {
            flatForward = Vector3.forward;
        }
        flatForward.Normalize();

        Vector3 basePosition = rigRoot != null ? rigRoot.position : trackedHead.position;
        Vector3 frontTargetPos = basePosition + flatForward * femaleFrontTargetDistance;
        frontTargetPos.y = NPC_Woman != null ? NPC_Woman.transform.position.y + femaleFrontTargetHeightOffset : 0f;
        femaleFrontRuntimeTarget.transform.position = frontTargetPos;
        femaleFrontRuntimeTarget.transform.rotation = Quaternion.LookRotation(-flatForward, Vector3.up);

        Debug.Log($"State_Greeting.UpdateFemaleRuntimeTarget: head={(trackedHead != null ? trackedHead.name : "null")}, headPos={(trackedHead != null ? trackedHead.position.ToString() : "null")}, rigRoot={(rigRoot != null ? rigRoot.name : "null")}, rigPos={(rigRoot != null ? rigRoot.position.ToString() : "null")}, forward={flatForward}, femaleFrontTargetPos={femaleFrontRuntimeTarget.transform.position}");
    }

    private void FaceHostTowardPlayer(NPCNavController host, string source)
    {
        if (host == null)
        {
            return;
        }

        Transform trackedHead = PlayerController.Instance != null ? PlayerController.Instance.controlledCamera : null;
        if (trackedHead == null)
        {
            Debug.LogWarning($"State_Greeting.FaceHostTowardPlayer: source={source}, trackedHead 为空。");
            return;
        }

        Vector3 lookDir = trackedHead.position - host.transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        host.transform.rotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
        Debug.Log($"State_Greeting.FaceHostTowardPlayer: source={source}, host={host.name}, hostPos={host.transform.position}, playerPos={trackedHead.position}, lookDir={lookDir}");
    }

    private void UpdateFemaleFootRuntimeTarget()
    {
        if (femaleFootRuntimeTarget == null)
        {
            EnsureMaleRuntimeTargets();
        }

        if (FemaleLeftFootTarget == null || FemaleRightFootTarget == null)
        {
            Debug.LogWarning("State_Greeting.UpdateFemaleFootRuntimeTarget: 女性脚部 target 未绑定，无法生成脚部互动目标点。");
            return;
        }

        Vector3 leftFoot = FemaleLeftFootTarget.position;
        Vector3 rightFoot = FemaleRightFootTarget.position;
        Vector3 feetCenter = (leftFoot + rightFoot) * 0.5f;
        Vector3 playerForward = PlayerController.Instance != null && PlayerController.Instance.playerRigRoot != null ? PlayerController.Instance.playerRigRoot.forward : Vector3.forward;
        playerForward.y = 0f;
        if (playerForward.sqrMagnitude < 0.0001f)
        {
            playerForward = Vector3.forward;
        }
        playerForward.Normalize();

        Vector3 playerRight = Vector3.Cross(Vector3.up, playerForward).normalized;
        Vector3 targetPos = feetCenter - playerForward * femaleFootTouchBackOffset + playerRight * femaleFootTouchSideOffset;
        targetPos.y = NPC_Woman != null ? NPC_Woman.transform.position.y : 0f;
        femaleFootRuntimeTarget.transform.position = targetPos;
        femaleFootRuntimeTarget.transform.rotation = Quaternion.LookRotation(playerForward, Vector3.up);

        Debug.Log($"State_Greeting.UpdateFemaleFootRuntimeTarget: leftFoot={leftFoot}, rightFoot={rightFoot}, feetCenter={feetCenter}, targetPos={targetPos}, playerForward={playerForward}, playerRight={playerRight}");
    }

    private void AlignFemaleHostTowardFeet()
    {
        if (NPC_Woman == null)
        {
            Debug.LogWarning("State_Greeting.AlignFemaleHostTowardFeet: NPC_Woman 为空。");
            return;
        }

        if (FemaleLeftFootTarget == null || FemaleRightFootTarget == null)
        {
            Debug.LogWarning("State_Greeting.AlignFemaleHostTowardFeet: 女性脚部 target 未绑定，无法精确朝向脚部。");
            return;
        }

        Vector3 feetCenter = (FemaleLeftFootTarget.position + FemaleRightFootTarget.position) * 0.5f;
        Vector3 lookDir = feetCenter - NPC_Woman.transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude <= 0.0001f)
        {
            Debug.LogWarning("State_Greeting.AlignFemaleHostTowardFeet: 女主持与脚部中心过近，跳过朝向调整。");
            return;
        }

        NPC_Woman.transform.rotation = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
        Debug.Log($"State_Greeting.AlignFemaleHostTowardFeet: leftFoot={FemaleLeftFootTarget.position}, rightFoot={FemaleRightFootTarget.position}, feetCenter={feetCenter}, npcPos={NPC_Woman.transform.position}, lookDir={lookDir}");
    }

    private void MoveHost(NPCNavController host, Animator animator, Transform target, string walkBoolName, string targetLabel, System.Action onArrived)
    {
        if (host == null || animator == null)
        {
            Debug.LogError($"State_Greeting.MoveHost: host或animator为空，targetLabel={targetLabel}");
            return;
        }

        if (target == null)
        {
            Debug.LogError($"State_Greeting.MoveHost: 目标点为空，targetLabel={targetLabel}");
            return;
        }

        Vector3 hostPos = host.transform.position;
        Vector3 targetPos = target.position;
        float distance = Vector3.Distance(hostPos, targetPos);
        Debug.Log($"State_Greeting.MoveHost: host={host.name}, targetLabel={targetLabel}, target={target.name}, hostPos={hostPos}, targetPos={targetPos}, distance={distance:F3}");

        if (distance <= 0.12f)
        {
            Debug.LogWarning($"State_Greeting.MoveHost: host={host.name} 到目标 {target.name} 的距离过近，直接视为到达，不走路。distance={distance:F3}");
            animator.SetBool(walkBoolName, false);
            onArrived?.Invoke();
            return;
        }

        animator.SetBool(walkBoolName, true);
        host.MoveToTarget(target, () => {
            animator.SetBool(walkBoolName, false);
            Debug.Log($"State_Greeting.MoveHost Arrived: host={host.name}, targetLabel={targetLabel}, target={target.name}");
            onArrived?.Invoke();
        });
    }

    private void OnDestroy()
    {
        if (Application.isPlaying)
        {
            if (maleFrontRuntimeTarget != null) Destroy(maleFrontRuntimeTarget);
            if (maleContactRuntimeTarget != null) Destroy(maleContactRuntimeTarget);
            if (femaleFrontRuntimeTarget != null) Destroy(femaleFrontRuntimeTarget);
            if (femaleFootRuntimeTarget != null) Destroy(femaleFootRuntimeTarget);
        }
    }
}
