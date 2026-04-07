using UnityEngine;

/// <summary>
/// 通用人形角色IK控制器（开关都带平滑过渡）
/// 挂载到带有Animator组件的Humanoid类型角色上使用
/// </summary>
[RequireComponent(typeof(Animator))]
public class UniversalIKController : MonoBehaviour
{
    [Header("IK全局设置")]
    public bool enableIK = true;
    [Tooltip("IK平滑过渡时长（秒）")]
    public float ikSmoothDuration = 0.25f;

    [Header("头部IK")]
    public bool enableHeadIK = false;
    public Transform headTarget;
    [Range(0, 1)] public float headWeight = 0.75f;
    [Range(0, 1)] public float lookAtBodyWeight = 0.1f;
    [Range(0, 1)] public float lookAtHeadWeight = 0.65f;
    [Range(0, 1)] public float lookAtEyesWeight = 0.2f;
    [Range(0, 1)] public float lookAtClampWeight = 0.5f;

    [Header("左手IK")]
    public bool enableLeftHandIK = false;
    public Transform leftHandTarget;
    [Range(0, 1)] public float leftHandPositionWeight = 1f;
    [Range(0, 1)] public float leftHandRotationWeight = 1f;

    [Header("右手IK")]
    public bool enableRightHandIK = false;
    public Transform rightHandTarget;
    [Range(0, 1)] public float rightHandPositionWeight = 1f;
    [Range(0, 1)] public float rightHandRotationWeight = 1f;

    [Header("左脚IK")]
    public bool enableLeftFootIK = false;
    public Transform leftFootTarget;
    [Range(0, 1)] public float leftFootPositionWeight = 1f;
    [Range(0, 1)] public float leftFootRotationWeight = 1f;

    [Header("右脚IK")]
    public bool enableRightFootIK = false;
    public Transform rightFootTarget;
    [Range(0, 1)] public float rightFootPositionWeight = 1f;
    [Range(0, 1)] public float rightFootRotationWeight = 1f;

    // 内部当前权重（用来做平滑）
    private float _currHeadWeight;
    private float _currLookAtBodyWeight;
    private float _currLookAtHeadWeight;
    private float _currLookAtEyesWeight;
    private float _currLookAtClampWeight;

    private float _currLeftHandPosWeight, _currLeftHandRotWeight;
    private float _currRightHandPosWeight, _currRightHandRotWeight;

    private float _currLeftFootPosWeight, _currLeftFootRotWeight;
    private float _currRightFootPosWeight, _currRightFootRotWeight;

    // 目标位置缓存（平滑移动）
    private Vector3 _leftHandIKPos, _rightHandIKPos;
    private Quaternion _leftHandIKRot, _rightHandIKRot;

    private Vector3 _leftFootIKPos, _rightFootIKPos;
    private Quaternion _leftFootIKRot, _rightFootIKRot;

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        if (_animator.avatar == null || !_animator.avatar.isHuman)
        {
            Debug.LogError("角色不是Humanoid，IK无法工作", this);
            enableIK = false;
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (!enableIK || _animator == null) return;
        float dt = Time.deltaTime;
        float smooth = ikSmoothDuration <= 0 ? 1000 : dt / ikSmoothDuration;

        //==================== 头部 ====================
        float headTargetW = enableHeadIK && headTarget ? headWeight : 0f;
        _currHeadWeight = Mathf.Lerp(_currHeadWeight, headTargetW, smooth);
        _currLookAtBodyWeight = Mathf.Lerp(_currLookAtBodyWeight, enableHeadIK && headTarget ? lookAtBodyWeight : 0f, smooth);
        _currLookAtHeadWeight = Mathf.Lerp(_currLookAtHeadWeight, enableHeadIK && headTarget ? lookAtHeadWeight : 0f, smooth);
        _currLookAtEyesWeight = Mathf.Lerp(_currLookAtEyesWeight, enableHeadIK && headTarget ? lookAtEyesWeight : 0f, smooth);
        _currLookAtClampWeight = Mathf.Lerp(_currLookAtClampWeight, enableHeadIK && headTarget ? lookAtClampWeight : 1f, smooth);

        if (_currHeadWeight > 0.001f && headTarget)
        {
            _animator.SetLookAtWeight(
                _currHeadWeight,
                _currLookAtBodyWeight,
                _currLookAtHeadWeight,
                _currLookAtEyesWeight,
                _currLookAtClampWeight);
            _animator.SetLookAtPosition(headTarget.position);
        }
        else
        {
            _animator.SetLookAtWeight(0f, 0f, 0f, 0f, 1f);
        }

        //==================== 左手 ====================
        UpdateLimbIK(
            AvatarIKGoal.LeftHand,
            enableLeftHandIK && leftHandTarget,
            leftHandTarget,
            leftHandPositionWeight,
            leftHandRotationWeight,
            ref _currLeftHandPosWeight, ref _currLeftHandRotWeight,
            ref _leftHandIKPos, ref _leftHandIKRot,
            smooth
        );

        //==================== 右手 ====================
        UpdateLimbIK(
            AvatarIKGoal.RightHand,
            enableRightHandIK && rightHandTarget,
            rightHandTarget,
            rightHandPositionWeight,
            rightHandRotationWeight,
            ref _currRightHandPosWeight, ref _currRightHandRotWeight,
            ref _rightHandIKPos, ref _rightHandIKRot,
            smooth
        );

        //==================== 左脚 ====================
        UpdateLimbIK(
            AvatarIKGoal.LeftFoot,
            enableLeftFootIK && leftFootTarget,
            leftFootTarget,
            leftFootPositionWeight,
            leftFootRotationWeight,
            ref _currLeftFootPosWeight, ref _currLeftFootRotWeight,
            ref _leftFootIKPos, ref _leftFootIKRot,
            smooth
        );

        //==================== 右脚 ====================
        UpdateLimbIK(
            AvatarIKGoal.RightFoot,
            enableRightFootIK && rightFootTarget,
            rightFootTarget,
            rightFootPositionWeight,
            rightFootRotationWeight,
            ref _currRightFootPosWeight, ref _currRightFootRotWeight,
            ref _rightFootIKPos, ref _rightFootIKRot,
            smooth
        );
    }

    private void UpdateLimbIK(
        AvatarIKGoal goal,
        bool isEnabled,
        Transform target,
        float targetPosW,
        float targetRotW,
        ref float currPosW, ref float currRotW,
        ref Vector3 ikPos, ref Quaternion ikRot,
        float smooth
    )
    {
        float wantPosW = isEnabled ? targetPosW : 0f;
        float wantRotW = isEnabled ? targetRotW : 0f;

        currPosW = Mathf.Lerp(currPosW, wantPosW, smooth);
        currRotW = Mathf.Lerp(currRotW, wantRotW, smooth);

        if (currPosW > 0.001f && target)
        {
            ikPos = Vector3.Lerp(ikPos, target.position, smooth);
            ikRot = Quaternion.Lerp(ikRot, target.rotation, smooth);

            _animator.SetIKPosition(goal, ikPos);
            _animator.SetIKRotation(goal, ikRot);
        }

        _animator.SetIKPositionWeight(goal, currPosW);
        _animator.SetIKRotationWeight(goal, currRotW);
    }

    // ================== 外部调用 API ==================
    public void SetHandTarget(bool isLeft, Transform target, bool enable = true)
    {
        if (isLeft)
        {
            enableLeftHandIK = enable;
            leftHandTarget = target;
        }
        else
        {
            enableRightHandIK = enable;
            rightHandTarget = target;
        }
    }

    public void SetHeadTarget(Transform target, bool enable = true)
    {
        enableHeadIK = enable;
        headTarget = target;
    }

    public void SetFootTarget(bool isLeft, Transform target, bool enable = true)
    {
        if (isLeft)
        {
            enableLeftFootIK = enable;
            leftFootTarget = target;
        }
        else
        {
            enableRightFootIK = enable;
            rightFootTarget = target;
        }
    }

    public void DisableAllIK()
    {
        enableHeadIK = false;
        enableLeftHandIK = false;
        enableRightHandIK = false;
        enableLeftFootIK = false;
        enableRightFootIK = false;
    }
}