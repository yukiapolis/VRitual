using System;
using UnityEngine;
using DG.Tweening;

public class PlayerPoseController : MonoBehaviour
{
    public enum PlayerPose
    {
        Stand,
        Sit,
        Crouch
    }

    [Header("角色根节点")]
    [SerializeField] private Transform playerRoot;

    [Header("玩家动画")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Transform trackedHead;

    [Header("姿态点位")]
    [SerializeField] private Transform standAnchor;
    [SerializeField] private Transform sitAnchor;
    [SerializeField] private Transform crouchAnchor;

    [Header("移动参数")]
    [SerializeField] private float alignDuration = 0.5f;
    [SerializeField] private Ease alignEase = Ease.OutCubic;
    [SerializeField] private bool alignXZOnly = true;
    [SerializeField] private bool keepCurrentYWhenAligning = true;
    [SerializeField] private bool alignYawToAnchor = false;
    [SerializeField] private bool snapToAnchorInstantly = true;

    [Header("Animator参数名")]
    [SerializeField] private string walkBoolName = "Walk";
    [SerializeField] private string sitBoolName = "Sit";
    [SerializeField] private string crouchBoolName = "DunXia";
    [SerializeField] private string standTriggerName = "TaiShou";

    private XRSeatAlignmentDebugger alignmentDebugger;
    private Tween moveTween;
    private Tween rotateTween;
    private bool isAligning;
    private PlayerPose currentPose = PlayerPose.Stand;

    public bool IsAligning => isAligning;
    public PlayerPose CurrentPose => currentPose;

    private void Awake()
    {
        if (playerRoot == null)
        {
            playerRoot = transform;
        }

        if (playerAnimator == null)
        {
            playerAnimator = GetComponentInChildren<Animator>();
        }

        if (trackedHead == null && Camera.main != null)
        {
            trackedHead = Camera.main.transform;
        }

        alignmentDebugger = FindObjectOfType<XRSeatAlignmentDebugger>();
    }

    public void Configure(Transform root, Animator animator, Transform head, Transform stand, Transform sit, Transform crouch)
    {
        playerRoot = root != null ? root : transform;
        playerAnimator = animator != null ? animator : playerAnimator;
        trackedHead = head != null ? head : trackedHead;
        standAnchor = stand;
        sitAnchor = sit;
        crouchAnchor = crouch;

        if (trackedHead == null && Camera.main != null)
        {
            trackedHead = Camera.main.transform;
        }
    }

    public void MoveToPose(string command, Action onComplete = null)
    {
        switch (command.ToLower())
        {
            case "stand":
            case "zl":
                MoveToPose(PlayerPose.Stand, standAnchor, onComplete);
                break;
            case "sit":
            case "zx":
                MoveToPose(PlayerPose.Sit, sitAnchor, onComplete);
                break;
            case "crouch":
            case "dx":
                MoveToPose(PlayerPose.Crouch, crouchAnchor, onComplete);
                break;
            default:
                Debug.LogWarning($"未知玩家姿态命令: {command}");
                onComplete?.Invoke();
                break;
        }
    }

    public void MoveToPose(PlayerPose pose, Transform anchor = null, Action onComplete = null)
    {
        Transform targetAnchor = anchor != null ? anchor : GetAnchorForPose(pose);
        Debug.Log($"PlayerPoseController.MoveToPose: pose={pose}, targetAnchor={(targetAnchor != null ? targetAnchor.name : "null")}, targetAnchorPos={(targetAnchor != null ? targetAnchor.position.ToString() : "null")}");

        if (targetAnchor == null)
        {
            ApplyPose(pose);
            onComplete?.Invoke();
            return;
        }

        AlignToAnchor(targetAnchor, () =>
        {
            ApplyPose(pose);
            onComplete?.Invoke();
        });
    }

    public void AlignHeightToAnchor(Transform anchor)
    {
        if (playerRoot == null || anchor == null)
        {
            return;
        }

        Vector3 position = playerRoot.position;
        position.y = anchor.position.y;
        playerRoot.position = position;
    }

    public void ApplyPose(PlayerPose pose)
    {
        currentPose = pose;
        Debug.Log($"PlayerPoseController.ApplyPose: pose={pose}, animator={(playerAnimator != null ? playerAnimator.name : "null")}, controller={(playerAnimator != null && playerAnimator.runtimeAnimatorController != null ? playerAnimator.runtimeAnimatorController.name : "null")}, avatar={(playerAnimator != null && playerAnimator.avatar != null ? playerAnimator.avatar.name : "null")}");

        if (playerAnimator == null)
        {
            Debug.LogWarning("PlayerPoseController.ApplyPose: playerAnimator 为 null，无法设置动画参数。");
            return;
        }

        SetAnimatorBool(walkBoolName, false);

        switch (pose)
        {
            case PlayerPose.Stand:
                Debug.Log($"PlayerPoseController.ApplyPose -> Stand: {sitBoolName}=false, {crouchBoolName}=false, trigger={standTriggerName}");
                SetAnimatorBool(sitBoolName, false);
                SetAnimatorBool(crouchBoolName, false);
                SetAnimatorTrigger(standTriggerName);
                break;
            case PlayerPose.Sit:
                Debug.Log($"PlayerPoseController.ApplyPose -> Sit: {crouchBoolName}=false, {sitBoolName}=true");
                SetAnimatorBool(crouchBoolName, false);
                SetAnimatorBool(sitBoolName, true);
                break;
            case PlayerPose.Crouch:
                Debug.Log($"PlayerPoseController.ApplyPose -> Crouch: {sitBoolName}=false, {crouchBoolName}=true");
                SetAnimatorBool(sitBoolName, false);
                SetAnimatorBool(crouchBoolName, true);
                break;
        }
    }

    public void AlignToAnchor(Transform anchor, Action onComplete = null)
    {
        if (playerRoot == null || anchor == null)
        {
            onComplete?.Invoke();
            return;
        }

        moveTween?.Kill();
        rotateTween?.Kill();

        alignmentDebugger ??= FindObjectOfType<XRSeatAlignmentDebugger>();
        alignmentDebugger?.LogState("Before AlignToAnchor", playerRoot, anchor);

        PlayerLocomotionLock locomotionLock = GetComponent<PlayerLocomotionLock>();
        locomotionLock?.SuspendPositionLock(true);

        isAligning = true;

        Vector3 targetPosition = anchor.position;
        if (trackedHead != null)
        {
            Vector3 headOffsetWorld = trackedHead.position - playerRoot.position;
            targetPosition = anchor.position - headOffsetWorld;
        }

        if (alignXZOnly)
        {
            targetPosition.x = targetPosition.x;
            targetPosition.z = targetPosition.z;
            targetPosition.y = keepCurrentYWhenAligning ? playerRoot.position.y : targetPosition.y;
        }

        bool shouldRotate = alignYawToAnchor;
        Vector3 targetEuler = playerRoot.eulerAngles;
        if (shouldRotate)
        {
            targetEuler.y = anchor.eulerAngles.y;
        }

        if (snapToAnchorInstantly)
        {
            playerRoot.position = targetPosition;
            if (shouldRotate)
            {
                playerRoot.eulerAngles = targetEuler;
            }

            locomotionLock?.SuspendPositionLock(false);
            alignmentDebugger?.LogState("After AlignToAnchor Snap", playerRoot, anchor);
            isAligning = false;
            onComplete?.Invoke();
            return;
        }

        moveTween = playerRoot.DOMove(targetPosition, alignDuration).SetEase(alignEase);

        if (shouldRotate)
        {
            rotateTween = playerRoot.DORotate(targetEuler, alignDuration, RotateMode.Fast).SetEase(alignEase).OnComplete(() =>
            {
                playerRoot.position = targetPosition;
                playerRoot.eulerAngles = targetEuler;
                locomotionLock?.SuspendPositionLock(false);
                alignmentDebugger?.LogState("After AlignToAnchor RotateTween", playerRoot, anchor);
                isAligning = false;
                onComplete?.Invoke();
            });
        }
        else
        {
            moveTween.OnComplete(() =>
            {
                playerRoot.position = targetPosition;
                locomotionLock?.SuspendPositionLock(false);
                alignmentDebugger?.LogState("After AlignToAnchor MoveTween", playerRoot, anchor);
                isAligning = false;
                onComplete?.Invoke();
            });
        }
    }

    private Transform GetAnchorForPose(PlayerPose pose)
    {
        switch (pose)
        {
            case PlayerPose.Sit:
                return sitAnchor;
            case PlayerPose.Crouch:
                return crouchAnchor;
            default:
                return standAnchor;
        }
    }

    private void SetAnimatorBool(string paramName, bool value)
    {
        if (playerAnimator == null || string.IsNullOrWhiteSpace(paramName))
        {
            Debug.LogWarning($"PlayerPoseController.SetAnimatorBool: animator或参数名无效。animator={(playerAnimator != null ? playerAnimator.name : "null")}, param={paramName}");
            return;
        }

        foreach (var param in playerAnimator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Bool && param.name == paramName)
            {
                AnimatorStateInfo beforeState = playerAnimator.GetCurrentAnimatorStateInfo(0);
                bool wasInTransition = playerAnimator.IsInTransition(0);
                Debug.Log($"PlayerPoseController.SetAnimatorBool BEFORE: animator={playerAnimator.name}, param={paramName}, value={value}, stateHash={beforeState.fullPathHash}, normalizedTime={beforeState.normalizedTime}, inTransition={wasInTransition}");
                playerAnimator.SetBool(paramName, value);
                AnimatorStateInfo afterState = playerAnimator.GetCurrentAnimatorStateInfo(0);
                bool isInTransition = playerAnimator.IsInTransition(0);
                Debug.Log($"PlayerPoseController.SetAnimatorBool AFTER: animator={playerAnimator.name}, param={paramName}, value={value}, stateHash={afterState.fullPathHash}, normalizedTime={afterState.normalizedTime}, inTransition={isInTransition}");
                return;
            }
        }

        Debug.LogWarning($"PlayerPoseController.SetAnimatorBool: Animator {playerAnimator.name} 中未找到 Bool 参数 {paramName}。现有参数: {string.Join(", ", Array.ConvertAll(playerAnimator.parameters, p => p.name + ":" + p.type))}");
    }

    private void SetAnimatorTrigger(string paramName)
    {
        if (playerAnimator == null || string.IsNullOrWhiteSpace(paramName))
        {
            Debug.LogWarning($"PlayerPoseController.SetAnimatorTrigger: animator或参数名无效。animator={(playerAnimator != null ? playerAnimator.name : "null")}, param={paramName}");
            return;
        }

        foreach (var param in playerAnimator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger && param.name == paramName)
            {
                AnimatorStateInfo beforeState = playerAnimator.GetCurrentAnimatorStateInfo(0);
                bool wasInTransition = playerAnimator.IsInTransition(0);
                Debug.Log($"PlayerPoseController.SetAnimatorTrigger BEFORE: animator={playerAnimator.name}, param={paramName}, stateHash={beforeState.fullPathHash}, normalizedTime={beforeState.normalizedTime}, inTransition={wasInTransition}");
                playerAnimator.SetTrigger(paramName);
                AnimatorStateInfo afterState = playerAnimator.GetCurrentAnimatorStateInfo(0);
                bool isInTransition = playerAnimator.IsInTransition(0);
                Debug.Log($"PlayerPoseController.SetAnimatorTrigger AFTER: animator={playerAnimator.name}, param={paramName}, stateHash={afterState.fullPathHash}, normalizedTime={afterState.normalizedTime}, inTransition={isInTransition}");
                return;
            }
        }

        Debug.LogWarning($"PlayerPoseController.SetAnimatorTrigger: Animator {playerAnimator.name} 中未找到 Trigger 参数 {paramName}");
    }
}
