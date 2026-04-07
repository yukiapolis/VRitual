using System.Collections.Generic;
using UnityEngine;

public class PlayerVisualFollower : MonoBehaviour
{
    [Header("跟随目标")]
    [SerializeField] private Transform rigRoot;
    [SerializeField] private Transform trackedHead;

    [Header("身体偏移")]
    [SerializeField] private float bodyHeightOffset = -1.25f;
    [SerializeField] private float forwardOffset = -0.28f;
    [SerializeField] private float sideOffset = 0f;
    [SerializeField] private float yawRotationOffset = 0f;
    [SerializeField] private bool followYawOnly = true;
    [SerializeField] private bool updatePosition = true;
    [SerializeField] private bool updateRotation = true;
    [SerializeField] private bool logBindingOnStart = true;
    [SerializeField] private bool forceRuntimePreset = true;

    [Header("姿态偏移")]
    [SerializeField] private float sitBodyHeightOffset = -1.25f;
    [SerializeField] private float sitForwardOffset = -0.20f;
    [SerializeField] private float crouchBodyHeightOffset = -1.55f;
    [SerializeField] private float crouchForwardOffset = -0.18f;

    [Header("第一人称可视性")]
    [SerializeField] private bool hideHeadRenderersOnStart = true;
    [SerializeField] private bool hideHeadBoneHierarchyFirst = true;
    [SerializeField] private string[] hiddenNameKeywords = new[] { "head", "face", "eye", "teeth", "tongue", "hair" };

    private readonly List<Renderer> hiddenRenderers = new List<Renderer>();
    private bool runtimeUpdatePosition = true;
    private bool runtimeUpdateRotation = true;
    private float runtimeBodyHeightOffset;
    private float runtimeForwardOffset;
    private float runtimeSideOffset;
    private float runtimeYawRotationOffset;

    private void Start()
    {
        AutoAssign();

        if (forceRuntimePreset)
        {
            bodyHeightOffset = -1.25f;
            forwardOffset = -0.28f;
            sideOffset = 0f;
            yawRotationOffset = 0f;
            Debug.Log($"PlayerVisualFollower.Start: 已强制应用运行时预设 bodyHeightOffset={bodyHeightOffset}, forwardOffset={forwardOffset}, sideOffset={sideOffset}, yawRotationOffset={yawRotationOffset}");
        }

        ApplyRuntimeOffsets(bodyHeightOffset, forwardOffset, sideOffset, yawRotationOffset, "StartBase");

        if (logBindingOnStart)
        {
            Debug.Log($"PlayerVisualFollower.Start: self={name}, rigRoot={(rigRoot != null ? rigRoot.name : "null")}, trackedHead={(trackedHead != null ? trackedHead.name : "null")}, bodyHeightOffset={bodyHeightOffset}, forwardOffset={forwardOffset}, sideOffset={sideOffset}, yawRotationOffset={yawRotationOffset}");
        }

        if (hideHeadRenderersOnStart)
        {
            HideHeadRenderers();
        }
    }

    private void LateUpdate()
    {
        if (rigRoot == null || trackedHead == null)
        {
            AutoAssign();
            if (rigRoot == null || trackedHead == null)
            {
                return;
            }
        }

        Vector3 flattenedForward = trackedHead.forward;
        flattenedForward.y = 0f;
        if (flattenedForward.sqrMagnitude < 0.0001f)
        {
            flattenedForward = rigRoot.forward;
            flattenedForward.y = 0f;
        }
        if (flattenedForward.sqrMagnitude < 0.0001f)
        {
            flattenedForward = Vector3.forward;
        }
        flattenedForward.Normalize();

        Vector3 flattenedRight = Vector3.Cross(Vector3.up, flattenedForward).normalized;
        if (flattenedRight.sqrMagnitude < 0.0001f)
        {
            flattenedRight = Vector3.right;
        }

        if (runtimeUpdatePosition && updatePosition)
        {
            Vector3 targetPosition = trackedHead.position;
            targetPosition += flattenedForward * runtimeForwardOffset;
            targetPosition += flattenedRight * runtimeSideOffset;
            targetPosition.y += runtimeBodyHeightOffset;
            transform.position = targetPosition;
        }

        if (runtimeUpdateRotation && updateRotation)
        {
            if (followYawOnly)
            {
                Vector3 euler = transform.eulerAngles;
                euler.y = Quaternion.LookRotation(flattenedForward, Vector3.up).eulerAngles.y + runtimeYawRotationOffset;
                transform.eulerAngles = euler;
            }
            else
            {
                transform.rotation = rigRoot.rotation;
            }
        }
    }

    public void SetFollowEnabled(bool positionEnabled, bool rotationEnabled)
    {
        runtimeUpdatePosition = positionEnabled;
        runtimeUpdateRotation = rotationEnabled;
        Debug.Log($"PlayerVisualFollower.SetFollowEnabled: self={name}, positionEnabled={positionEnabled}, rotationEnabled={rotationEnabled}");
    }

    public void ApplyStandProfile()
    {
        ApplyRuntimeOffsets(bodyHeightOffset, forwardOffset, sideOffset, yawRotationOffset, "Stand");
    }

    public void ApplySitProfile()
    {
        ApplyRuntimeOffsets(sitBodyHeightOffset, sitForwardOffset, sideOffset, yawRotationOffset, "Sit");
    }

    public void ApplyCrouchProfile()
    {
        ApplyRuntimeOffsets(crouchBodyHeightOffset, crouchForwardOffset, sideOffset, yawRotationOffset, "Crouch");
    }

    [ContextMenu("Log Current Follower State")]
    public void LogCurrentState()
    {
        AutoAssign();
        Debug.Log($"PlayerVisualFollower.LogCurrentState: self={name}, selfPos={transform.position}, rigRoot={(rigRoot != null ? rigRoot.name : "null")}:{(rigRoot != null ? rigRoot.position.ToString() : "null")}, trackedHead={(trackedHead != null ? trackedHead.name : "null")}:{(trackedHead != null ? trackedHead.position.ToString() : "null")}");
    }

    [ContextMenu("Hide Head Renderers")]
    public void HideHeadRenderers()
    {
        hiddenRenderers.Clear();

        if (hideHeadBoneHierarchyFirst)
        {
            Animator animator = GetComponent<Animator>();
            if (animator != null && animator.isHuman)
            {
                Transform headBone = animator.GetBoneTransform(HumanBodyBones.Head);
                if (headBone != null)
                {
                    foreach (Renderer renderer in headBone.GetComponentsInChildren<Renderer>(true))
                    {
                        renderer.enabled = false;
                        hiddenRenderers.Add(renderer);
                        Debug.Log($"PlayerVisualFollower.HideHeadRenderers: disabled head-bone renderer on {renderer.gameObject.name}");
                    }
                }
                else
                {
                    Debug.LogWarning("PlayerVisualFollower.HideHeadRenderers: 未找到 Head bone，改用名称关键词隐藏。");
                }

                Transform neckBone = animator.GetBoneTransform(HumanBodyBones.Neck);
                if (neckBone != null)
                {
                    foreach (Renderer renderer in neckBone.GetComponentsInChildren<Renderer>(true))
                    {
                        if (hiddenRenderers.Contains(renderer))
                        {
                            continue;
                        }

                        renderer.enabled = false;
                        hiddenRenderers.Add(renderer);
                        Debug.Log($"PlayerVisualFollower.HideHeadRenderers: disabled neck-bone renderer on {renderer.gameObject.name}");
                    }
                }
            }
        }

        foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
        {
            if (hiddenRenderers.Contains(renderer))
            {
                continue;
            }

            string lowerName = renderer.gameObject.name.ToLowerInvariant();
            for (int i = 0; i < hiddenNameKeywords.Length; i++)
            {
                string keyword = hiddenNameKeywords[i].ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(keyword) && lowerName.Contains(keyword))
                {
                    renderer.enabled = false;
                    hiddenRenderers.Add(renderer);
                    Debug.Log($"PlayerVisualFollower.HideHeadRenderers: disabled keyword renderer on {renderer.gameObject.name}");
                    break;
                }
            }
        }

        Debug.Log($"PlayerVisualFollower.HideHeadRenderers: total disabled = {hiddenRenderers.Count}");
    }

    private void ApplyRuntimeOffsets(float height, float forward, float side, float yaw, string source)
    {
        runtimeBodyHeightOffset = height;
        runtimeForwardOffset = forward;
        runtimeSideOffset = side;
        runtimeYawRotationOffset = yaw;
        Debug.Log($"PlayerVisualFollower.ApplyRuntimeOffsets: self={name}, source={source}, bodyHeightOffset={runtimeBodyHeightOffset}, forwardOffset={runtimeForwardOffset}, sideOffset={runtimeSideOffset}, yawRotationOffset={runtimeYawRotationOffset}");
    }

    private void AutoAssign()
    {
        if (rigRoot == null)
        {
            rigRoot = GameObject.Find("XR Origin (XR Rig)")?.transform;
        }

        if (trackedHead == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                trackedHead = mainCam.transform;
            }
            else
            {
                trackedHead = GameObject.Find("Main Camera")?.transform;
            }
        }
    }
}
