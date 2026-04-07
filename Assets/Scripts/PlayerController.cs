using System;
using UnityEngine;
using DG.Tweening; // 需导入DoTween，若不用则注释并启用原生协程

[DefaultExecutionOrder(-10)]
public class PlayerController : MonoSingleton<PlayerController>
{
    [Header("玩家姿态控制")]
    [Tooltip("可选：用于同步玩家角色根节点位置和动画姿态")]
    public PlayerPoseController poseController;
    [Tooltip("男性玩家仪式坐姿点")]
    public Transform playerMaleSeatPoint;
    [Tooltip("女性玩家地坐姿点")]
    public Transform playerFemaleFloorPoint;
    [Tooltip("玩家Rig根节点，不填则自动查找 XR Origin (XR Rig)")]
    public Transform playerRigRoot;
    [Tooltip("玩家动画模型根节点，不填则尝试查找名为 GameObject 的子节点")]
    public Transform playerVisualRoot;
    [Tooltip("玩家Animator，不填则自动查找")]
    public Animator playerAnimator;
    [Tooltip("玩家移动锁，不填则自动添加")]
    public PlayerLocomotionLock locomotionLock;
    [Tooltip("编辑器/桌面测试用的XR Simulator，仪式时关闭，站立时恢复")]
    public GameObject xrSimulator;

    [Header("核心配置：要控制的相机")]
    [Tooltip("拖拽赋值需要移动的相机对象（主相机/其他相机）")]
    public Transform controlledCamera;

    [Header("相机目标位置配置")]
    [Tooltip("站立时相机的目标位置")]
    public Transform standPosition;
    [Tooltip("男性玩家仪式坐姿相机点")]
    public Transform sitPosition;
    [Tooltip("女性玩家地坐姿相机点")]
    public Transform crouchPosition;
    [Tooltip("男性玩家仪式开始前站立平视点")]
    public Transform maleStandPosition;
    [Tooltip("女性玩家仪式开始前站立平视点")]
    public Transform femaleStandPosition;

    [Header("移动参数")]
    [Tooltip("相机移动到目标位置的时长（秒）")]
    public float moveDuration = 0.5f;
    [Tooltip("缓动曲线（控制移动顺滑度）")]
    public Ease moveEase = Ease.OutCubic;
    [Tooltip("XR模式下通常不应强制旋转头显相机，默认关闭")]
    public bool rotateCameraToTarget = false;
    [Tooltip("XR模式下优先使用姿态锚点定位，不再使用旧相机点二次修正")]
    public bool usePoseAnchorForXrPlacement = true;

    private bool isMoving = false;
    private Transform cameraTrans;

    private void Awake()
    {
        if (controlledCamera == null)
        {
            Debug.LogError("请在面板中拖拽赋值要控制的相机对象！");
            return;
        }

        cameraTrans = controlledCamera.transform;

        if (standPosition == null || sitPosition == null || crouchPosition == null)
        {
            Debug.LogError("请为相机的三个目标位置赋值！");
        }

        TrySetupPoseController();
    }

    private void Start()
    {
        TrySetupPoseController();
    }

    public void MoveCameraToPosition(string command)
    {
        MoveCameraToPosition(command, () => { });
    }

    public void MoveCameraToPosition(string command, Action callback)
    {
        MoveCameraToPosition(command, (string targetName) => callback?.Invoke());
    }

    public void MoveCameraToPosition(string command, Action<string> callback)
    {
        if (cameraTrans == null) return;
        if (isMoving) return;

        Transform targetTrans = null;
        string targetName = "";
        bool isXrPoseDriven = poseController != null && playerRigRoot != null && usePoseAnchorForXrPlacement;
        PlayerPoseController.PlayerPose pose = GetPoseByCommand(command);
        Transform poseAnchor = GetPoseAnchorByCommand(command);
        switch (command.ToLower())
        {
            case "stand":
            case "zl":
                targetTrans = GetStandTargetByGender();
                targetName = "ZL";
                if (poseAnchor == null)
                {
                    poseAnchor = GetStandRigAnchorByGender();
                }
                break;
            case "sit":
            case "zx":
                targetTrans = sitPosition;
                targetName = "ZX";
                break;
            case "crouch":
            case "dx":
                targetTrans = crouchPosition;
                targetName = "DX";
                break;
            default:
                Debug.LogError($"未知命令：{command}，支持的命令：stand/站立、sit/坐下、crouch/蹲下");
                return;
        }

        UpdateLocomotionState(command);

        if (isXrPoseDriven)
        {
            if (poseAnchor == null)
            {
                Debug.LogWarning($"XR姿态驱动缺少锚点，命令 {command} 无法完成定位。");
                callback?.Invoke(targetName);
                return;
            }

            isMoving = true;
            poseController.MoveToPose(pose, poseAnchor, () =>
            {
                isMoving = false;
                callback?.Invoke(targetName);
            });
            return;
        }

        if (poseController != null)
        {
            poseController.MoveToPose(pose, poseAnchor, () => MoveToTarget(targetTrans, targetName, callback));
            return;
        }

        MoveToTarget(targetTrans, targetName, callback);
    }

    private void MoveToTarget(Transform target, string targetName, Action<string> callback)
    {
        callback?.Invoke(targetName);
    }

    public void ResetCameraToStand(Action callback = null)
    {
        MoveCameraOnly(GetStandTargetByGender(), "ZL", callback);
    }

    public void MoveCameraOnly(string command, Action callback = null)
    {
        Transform targetTrans = null;
        string targetName = "";

        switch (command.ToLower())
        {
            case "stand":
            case "zl":
                targetTrans = GetStandTargetByGender();
                targetName = "ZL";
                break;
            case "sit":
            case "zx":
                targetTrans = sitPosition;
                targetName = "ZX";
                break;
            case "crouch":
            case "dx":
                targetTrans = crouchPosition;
                targetName = "DX";
                break;
            default:
                Debug.LogError($"未知相机命令：{command}");
                callback?.Invoke();
                return;
        }

        MoveCameraOnly(targetTrans, targetName, callback);
    }

    public void MoveCameraOnly(Transform target, string targetName, Action callback = null)
    {
        callback?.Invoke();
    }

    private void TrySetupPoseController()
    {
        if (poseController == null)
        {
            poseController = GetComponent<PlayerPoseController>();
            if (poseController == null)
            {
                poseController = gameObject.AddComponent<PlayerPoseController>();
            }
        }

        if (playerRigRoot == null)
        {
            Transform rig = transform.Find("XR Origin (XR Rig)");
            if (rig != null)
            {
                playerRigRoot = rig;
            }
        }

        if (locomotionLock == null)
        {
            locomotionLock = GetComponent<PlayerLocomotionLock>();
            if (locomotionLock == null)
            {
                locomotionLock = gameObject.AddComponent<PlayerLocomotionLock>();
            }
        }

        if (playerVisualRoot == null)
        {
            Transform visual = transform.Find("PlayerCharacter");
            if (visual == null)
            {
                visual = transform.Find("GameObject");
            }
            if (visual == null)
            {
                visual = transform.Find("PlayerVisual");
            }
            if (visual != null)
            {
                playerVisualRoot = visual;
                Debug.Log($"PlayerController.TrySetupPoseController: 自动绑定 playerVisualRoot = {playerVisualRoot.name}");
            }
            else
            {
                Debug.LogWarning("PlayerController.TrySetupPoseController: 未自动找到 PlayerCharacter/GameObject/PlayerVisual，请手动绑定 playerVisualRoot。");
            }
        }

        if (playerAnimator == null)
        {
            if (playerVisualRoot != null)
            {
                playerAnimator = playerVisualRoot.GetComponentInChildren<Animator>(true);
                Debug.Log($"PlayerController.TrySetupPoseController: 从 playerVisualRoot 查找 Animator，结果={(playerAnimator != null ? playerAnimator.name : "null")}");
            }

            if (playerAnimator == null)
            {
                playerAnimator = GetComponentInChildren<Animator>(true);
                Debug.Log($"PlayerController.TrySetupPoseController: 从 Player 子树查找 Animator，结果={(playerAnimator != null ? playerAnimator.name : "null")}");
            }
        }

        if (playerAnimator == null)
        {
            Debug.LogError("PlayerController.TrySetupPoseController: 未找到玩家 Animator，姿态动画将无法播放。");
        }
        else
        {
            Debug.Log($"PlayerController.TrySetupPoseController: 最终使用 Animator = {playerAnimator.name}, gameObject = {playerAnimator.gameObject.name}");
        }

        if (playerMaleSeatPoint == null)
        {
            playerMaleSeatPoint = FindChildOrSceneTransform("Player_MaleSeatPoint");
        }

        if (playerFemaleFloorPoint == null)
        {
            playerFemaleFloorPoint = FindChildOrSceneTransform("Player_FemaleFloorPoint");
        }

        if (poseController != null)
        {
            if (controlledCamera == playerRigRoot)
            {
                Debug.LogError("PlayerController 的 controlledCamera 不能绑定 XR Rig 本身，必须绑定 Main Camera。");
            }

            Debug.Log($"PlayerController.TrySetupPoseController: 配置 PoseController，playerVisualRoot={(playerVisualRoot != null ? playerVisualRoot.name : "null")}, playerAnimator={(playerAnimator != null ? playerAnimator.name : "null")}, controller={(playerAnimator != null && playerAnimator.runtimeAnimatorController != null ? playerAnimator.runtimeAnimatorController.name : "null")}, avatar={(playerAnimator != null && playerAnimator.avatar != null ? playerAnimator.avatar.name : "null")}");
            poseController.Configure(playerRigRoot != null ? playerRigRoot : transform, playerAnimator, controlledCamera, null, playerMaleSeatPoint, playerFemaleFloorPoint);
        }
    }

    private PlayerPoseController.PlayerPose GetPoseByCommand(string command)
    {
        switch (command.ToLower())
        {
            case "sit":
            case "zx":
                return PlayerPoseController.PlayerPose.Sit;
            case "crouch":
            case "dx":
                return PlayerPoseController.PlayerPose.Crouch;
            default:
                return PlayerPoseController.PlayerPose.Stand;
        }
    }

    private Transform GetPoseAnchorByCommand(string command)
    {
        switch (command.ToLower())
        {
            case "stand":
            case "zl":
                return null;
            case "sit":
            case "zx":
                return playerMaleSeatPoint;
            case "crouch":
            case "dx":
                return playerFemaleFloorPoint;
            default:
                return null;
        }
    }

    private Transform GetStandTargetByGender()
    {
        bool isMalePlayer = PublicAtribuite.Instance != null && PublicAtribuite.Instance.CurrentGender == Gender.Male;

        if (isMalePlayer && maleStandPosition != null)
        {
            return maleStandPosition;
        }

        if (!isMalePlayer && femaleStandPosition != null)
        {
            return femaleStandPosition;
        }

        return standPosition;
    }

    private Transform GetStandRigAnchorByGender()
    {
        bool isMalePlayer = PublicAtribuite.Instance != null && PublicAtribuite.Instance.CurrentGender == Gender.Male;

        if (isMalePlayer)
        {
            Transform maleStandAnchor = FindChildOrSceneTransform("Male_PlayerStandTra");
            if (maleStandAnchor != null)
            {
                return maleStandAnchor;
            }
        }
        else
        {
            Transform femaleStandAnchor = FindChildOrSceneTransform("Female_PlayerStandTra");
            if (femaleStandAnchor != null)
            {
                return femaleStandAnchor;
            }
        }

        return null;
    }

    private Transform FindChildOrSceneTransform(string targetName)
    {
        Transform child = transform.Find(targetName);
        if (child != null)
        {
            return child;
        }

        GameObject sceneObject = GameObject.Find(targetName);
        return sceneObject != null ? sceneObject.transform : null;
    }

    private void UpdateLocomotionState(string command)
    {
        string lowerCommand = command.ToLower();
        bool shouldLock = lowerCommand == "sit" || lowerCommand == "zx" || lowerCommand == "crouch" || lowerCommand == "dx";

        if (xrSimulator != null)
        {
            xrSimulator.SetActive(!shouldLock);
        }

        if (locomotionLock != null)
        {
            locomotionLock.SetMovementLocked(shouldLock);
        }
    }
}
