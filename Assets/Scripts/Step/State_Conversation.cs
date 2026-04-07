using System.Collections.Generic;
using UnityEngine;

public class State_Conversation : StepBase
{
    [Header("主持人")]
    public NPCNavController NPC_Male;
    public NPCNavController NPC_Woman;

    [Header("头部LookAt / IK")]
    public UniversalIKController MaleHeadIK;
    public UniversalIKController FemaleHeadIK;
    public bool enableContinuousHeadLookAt = true;
    public Vector3 headLookOffset = new Vector3(0f, 0.05f, 0f);

    [Header("非当前主持人中立姿态")]
    public bool useNeutralPoseForInactiveHost = true;
    public bool inactiveHostSlightBow = true;
    [Range(0f, 25f)] public float inactiveHostDownAngle = 8f;
    [Range(0f, 20f)] public float inactiveHostTurnSpeed = 5f;
    public Transform MaleNeutralForward;
    public Transform FemaleNeutralForward;

    [Header("注视目标")]
    public List<Transform> MaleLookTargets = new List<Transform>();
    public List<Transform> FemaleLookTargets = new List<Transform>();

    [Header("玩家视角")]
    public Transform PlayerView;

    [Header("参数")]
    public float maxLookAngle = 12f;
    public float requiredLookDurationPerTarget = 3f;
    public bool resetProgressOnLookAway = true;
    public bool showDebugLog = true;
    public float debugLogInterval = 0.25f;
    [Tooltip("启用该步骤前是否允许播放交谈提醒音。关闭后，只有步骤真的开始后才会有提醒音。")]
    public bool allowReminderOnlyWhenRunning = true;

    [Header("视线提醒")]
    public AudioClip reminderClip;
    public float reminderCooldown = 1.5f;
    public bool useTipAudioSource = true;

    [Header("Scene 可视化")]
    public bool showGizmos = true;
    public float gizmoRayLength = 1.5f;
    public Color gizmoViewRayColor = Color.cyan;
    public Color gizmoTargetLineColor = Color.yellow;
    public Color gizmoActiveTargetColor = Color.green;
    public Color gizmoInactiveTargetColor = Color.gray;
    public Color gizmoAngleLimitColor = Color.red;
    public float gizmoTargetSphereRadius = 0.05f;

    private readonly List<Transform> activeTargets = new List<Transform>();
    private int currentTargetIndex;
    private float currentLookTimer;
    private bool isRunning;
    private float debugLogTimer;
    private float reminderTimer;
    private GameObject maleRuntimeLookTarget;
    private GameObject femaleRuntimeLookTarget;
    private Quaternion maleNeutralRotation;
    private Quaternion femaleNeutralRotation;
    private bool neutralRotationInitialized;

    public override void OnStart()
    {
        EnsureRuntimeLookTargets();
        currentTargetIndex = 0;
        currentLookTimer = 0f;
        debugLogTimer = 0f;
        reminderTimer = reminderCooldown;
        isRunning = true;
        base.OnStart();
    }

    protected override void SetStep()
    {
        base.SetStep();

        if (StepIndex != 0)
        {
            return;
        }

        ResolvePlayerView();
        ResolveActiveTargets();
        CacheNeutralRotations();
        FaceHostsTowardPlayerInstant();

        if (activeTargets.Count == 0 || PlayerView == null)
        {
            Debug.LogWarning("交谈步骤缺少注视目标或玩家视角，直接进入下一步。");
            isRunning = false;
            OnEnd();
            return;
        }

        if (showDebugLog)
        {
            Debug.Log($"开始交谈注视检测，当前需要依次注视 {activeTargets.Count} 个目标。");
        }
    }

    private void LateUpdate()
    {
        bool isMalePlayer = PublicAtribuite.Instance != null && PublicAtribuite.Instance.CurrentGender == Gender.Male;

        if (!IsCurrentConversationStep())
        {
            DisableHeadIK();
            return;
        }

        if (!isRunning)
        {
            DisableHeadIK();
            return;
        }

        if (!enableContinuousHeadLookAt)
        {
            DisableHeadIK();
            UpdateInactiveHostNeutralPose(isMalePlayer);
            return;
        }

        ResolvePlayerView();

        UpdateHostHeadLookAt(MaleHeadIK, maleRuntimeLookTarget, isMalePlayer);
        UpdateHostHeadLookAt(FemaleHeadIK, femaleRuntimeLookTarget, !isMalePlayer);
        UpdateInactiveHostNeutralPose(isMalePlayer);
    }

    private void Update()
    {
        if (!IsCurrentConversationStep() || !isRunning || StepIndex != 0 || PlayerView == null || currentTargetIndex >= activeTargets.Count)
        {
            return;
        }

        Transform currentLookTarget = activeTargets[currentTargetIndex];
        if (currentLookTarget == null)
        {
            AdvanceToNextTarget();
            return;
        }

        Vector3 toTarget = (currentLookTarget.position - PlayerView.position).normalized;
        float angle = Vector3.Angle(PlayerView.forward, toTarget);
        bool isLooking = angle <= maxLookAngle;

        debugLogTimer += Time.deltaTime;
        reminderTimer += Time.deltaTime;

        if (isLooking)
        {
            currentLookTimer += Time.deltaTime;

            if (showDebugLog && debugLogTimer >= debugLogInterval)
            {
                Debug.Log($"交谈检测：正在看 {currentLookTarget.name}，第 {currentTargetIndex + 1}/{activeTargets.Count} 个目标，累计 {currentLookTimer:F2} 秒，角度 {angle:F2}°");
                debugLogTimer = 0f;
            }

            if (currentLookTimer >= requiredLookDurationPerTarget)
            {
                AdvanceToNextTarget();
            }
        }
        else
        {
            if (showDebugLog && debugLogTimer >= debugLogInterval)
            {
                Debug.Log($"交谈检测：视线移开 {currentLookTarget.name}，第 {currentTargetIndex + 1}/{activeTargets.Count} 个目标，当前角度 {angle:F2}°，累计 {currentLookTimer:F2} 秒");
                debugLogTimer = 0f;
            }

            if (reminderTimer >= reminderCooldown)
            {
                reminderTimer = 0f;
                PlayReminder(currentLookTarget);
            }

            if (resetProgressOnLookAway)
            {
                currentLookTimer = 0f;
            }
            else
            {
                currentLookTimer = Mathf.Max(0f, currentLookTimer - Time.deltaTime);
            }
        }
    }

    private void ResolvePlayerView()
    {
        if (PlayerView == null && PlayerController.Instance != null && PlayerController.Instance.controlledCamera != null)
        {
            PlayerView = PlayerController.Instance.controlledCamera;
        }
    }

    private void ResolveActiveTargets()
    {
        activeTargets.Clear();

        List<Transform> sourceTargets = PublicAtribuite.Instance.CurrentGender == Gender.Male ? MaleLookTargets : FemaleLookTargets;
        for (int i = 0; i < sourceTargets.Count; i++)
        {
            if (sourceTargets[i] != null)
            {
                activeTargets.Add(sourceTargets[i]);
            }
        }
    }

    private void FaceHostsTowardPlayerInstant()
    {
        if (PlayerView == null)
        {
            return;
        }

        RotateHostBodyTowardPlayer(NPC_Male);
        RotateHostBodyTowardPlayer(NPC_Woman);
    }

    private void RotateHostBodyTowardPlayer(NPCNavController host)
    {
        if (host == null)
        {
            return;
        }

        Vector3 lookDirection = PlayerView.position - host.transform.position;
        lookDirection.y = 0f;
        if (lookDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        host.transform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
    }

    private void CacheNeutralRotations()
    {
        if (neutralRotationInitialized)
        {
            return;
        }

        maleNeutralRotation = ResolveNeutralRotation(NPC_Male, MaleNeutralForward);
        femaleNeutralRotation = ResolveNeutralRotation(NPC_Woman, FemaleNeutralForward);
        neutralRotationInitialized = true;
    }

    private Quaternion ResolveNeutralRotation(NPCNavController host, Transform neutralForward)
    {
        if (neutralForward != null)
        {
            return neutralForward.rotation;
        }

        if (host != null)
        {
            return host.transform.rotation;
        }

        return Quaternion.identity;
    }

    private void UpdateHostHeadLookAt(UniversalIKController ikController, GameObject runtimeTarget, bool shouldLookAtPlayer)
    {
        if (ikController == null)
        {
            return;
        }

        if (!shouldLookAtPlayer || runtimeTarget == null || PlayerView == null)
        {
            ikController.SetHeadTarget(null, false);
            return;
        }

        runtimeTarget.transform.position = PlayerView.position + headLookOffset;
        ikController.enableIK = true;
        ikController.SetHeadTarget(runtimeTarget.transform, true);
    }

    private void UpdateInactiveHostNeutralPose(bool isMalePlayer)
    {
        if (!useNeutralPoseForInactiveHost)
        {
            return;
        }

        NPCNavController inactiveHost = isMalePlayer ? NPC_Woman : NPC_Male;
        Quaternion targetRotation = isMalePlayer ? femaleNeutralRotation : maleNeutralRotation;
        ApplyNeutralPose(inactiveHost, targetRotation);
    }

    private void ApplyNeutralPose(NPCNavController host, Quaternion targetRotation)
    {
        if (host == null)
        {
            return;
        }

        Quaternion desiredRotation = targetRotation;
        if (inactiveHostSlightBow)
        {
            desiredRotation *= Quaternion.Euler(inactiveHostDownAngle, 0f, 0f);
        }

        host.transform.rotation = Quaternion.Slerp(host.transform.rotation, desiredRotation, Time.deltaTime * inactiveHostTurnSpeed);
    }

    private void AdvanceToNextTarget()
    {
        currentLookTimer = 0f;
        reminderTimer = reminderCooldown;
        currentTargetIndex++;

        if (currentTargetIndex >= activeTargets.Count)
        {
            isRunning = false;
            if (showDebugLog)
            {
                Debug.Log("交谈注视完成，进入下一步。");
            }
            OnEnd();
            return;
        }

        if (showDebugLog)
        {
            Debug.Log($"交谈进入下一个注视目标：{activeTargets[currentTargetIndex].name}");
        }
    }

    private void PlayReminder(Transform currentLookTarget)
    {
        if (showDebugLog)
        {
            Debug.Log($"交谈提醒：请把视线看回 {currentLookTarget.name}");
        }

        if (!IsCurrentConversationStep())
        {
            Debug.Log("State_Conversation.PlayReminder: 当前不是激活中的交谈步骤，跳过提醒音。");
            return;
        }

        if (allowReminderOnlyWhenRunning && !isRunning)
        {
            Debug.Log("State_Conversation.PlayReminder: 当前交谈步骤未运行，跳过提醒音。");
            return;
        }

        if (reminderClip == null || AudioManager.Instance == null)
        {
            return;
        }

        if (useTipAudioSource && AudioManager.Instance.Tip_AS != null)
        {
            AudioManager.Instance.Tip_AS.PlayOneShot(reminderClip);
            return;
        }

        AudioManager.Instance.ASPlayer(reminderClip);
    }

    private void OnDisable()
    {
        DisableHeadIK();
    }

    private bool IsCurrentConversationStep()
    {
        if (StepController.Instance == null)
        {
            Debug.LogWarning("State_Conversation.IsCurrentConversationStep: StepController.Instance 为 null，默认按未激活处理。");
            return false;
        }

        bool isCurrent = StepController.Instance.CurrentStep == this;
        if (!isCurrent && showDebugLog)
        {
            Debug.Log($"State_Conversation.IsCurrentConversationStep: 当前激活步骤不是自己。current={(StepController.Instance.CurrentStep != null ? StepController.Instance.CurrentStep.name : "null")}, self={name}");
        }
        return isCurrent;
    }

    private void OnDestroy()
    {
        DisableHeadIK();

        if (Application.isPlaying)
        {
            DestroyRuntimeLookTarget(ref maleRuntimeLookTarget);
            DestroyRuntimeLookTarget(ref femaleRuntimeLookTarget);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos)
        {
            return;
        }

        ResolvePlayerView();
        DrawPlayerViewGizmos();
        DrawTargetGizmos(MaleLookTargets);
        DrawTargetGizmos(FemaleLookTargets);
        DrawHeadLookGizmos(MaleHeadIK);
        DrawHeadLookGizmos(FemaleHeadIK);
    }

    private void DrawPlayerViewGizmos()
    {
        if (PlayerView == null)
        {
            return;
        }

        Gizmos.color = gizmoViewRayColor;
        Gizmos.DrawRay(PlayerView.position, PlayerView.forward * gizmoRayLength);

        Vector3 leftBoundary = Quaternion.AngleAxis(-maxLookAngle, Vector3.up) * PlayerView.forward;
        Vector3 rightBoundary = Quaternion.AngleAxis(maxLookAngle, Vector3.up) * PlayerView.forward;
        Gizmos.color = gizmoAngleLimitColor;
        Gizmos.DrawRay(PlayerView.position, leftBoundary.normalized * gizmoRayLength);
        Gizmos.DrawRay(PlayerView.position, rightBoundary.normalized * gizmoRayLength);
    }

    private void DrawTargetGizmos(List<Transform> targets)
    {
        if (targets == null)
        {
            return;
        }

        for (int i = 0; i < targets.Count; i++)
        {
            Transform target = targets[i];
            if (target == null)
            {
                continue;
            }

            bool isActive = i == currentTargetIndex;
            Gizmos.color = isActive ? gizmoActiveTargetColor : gizmoInactiveTargetColor;
            Gizmos.DrawSphere(target.position, gizmoTargetSphereRadius);

            if (PlayerView != null)
            {
                Gizmos.color = gizmoTargetLineColor;
                Gizmos.DrawLine(PlayerView.position, target.position);
            }
        }
    }

    private void DrawHeadLookGizmos(UniversalIKController ikController)
    {
        if (ikController == null || PlayerView == null)
        {
            return;
        }

        Animator animator = ikController.GetComponent<Animator>();
        if (animator == null || !animator.isHuman)
        {
            return;
        }

        Transform headBone = animator.GetBoneTransform(HumanBodyBones.Head);
        if (headBone == null)
        {
            return;
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(headBone.position, PlayerView.position + headLookOffset);
    }

    private void EnsureRuntimeLookTargets()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (maleRuntimeLookTarget == null)
        {
            maleRuntimeLookTarget = new GameObject("MaleConversationLookTarget_Runtime");
            maleRuntimeLookTarget.hideFlags = HideFlags.HideInHierarchy;
        }

        if (femaleRuntimeLookTarget == null)
        {
            femaleRuntimeLookTarget = new GameObject("FemaleConversationLookTarget_Runtime");
            femaleRuntimeLookTarget.hideFlags = HideFlags.HideInHierarchy;
        }
    }

    private void DisableHeadIK()
    {
        if (MaleHeadIK != null)
        {
            MaleHeadIK.SetHeadTarget(null, false);
        }

        if (FemaleHeadIK != null)
        {
            FemaleHeadIK.SetHeadTarget(null, false);
        }
    }

    private void DestroyRuntimeLookTarget(ref GameObject target)
    {
        if (target == null)
        {
            return;
        }

        Destroy(target);
        target = null;
    }
}
