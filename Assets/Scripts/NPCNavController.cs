using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 适配Unity 6的NPC导航移动控制脚本
/// 挂载到需要导航的NPC对象上（需确保NPC有NavMeshAgent组件）
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class NPCNavController : MonoBehaviour
{
    // NavMesh导航代理组件
    private NavMeshAgent _navAgent;
    // 移动结束回调
    private Action _onMoveComplete;
    // 保存目标Transform（用于匹配最终旋转）
    private Transform _currentTarget;
    // 判定到达目标的距离阈值（可根据项目调整）
    [SerializeField] private float arriveDistance = 0.1f;
    // 导航网格采样半径（验证目标是否有效）
    [SerializeField] private float navSampleRadius = 1f;

    private void Awake()
    {
        // 获取NavMeshAgent组件（Unity 6中RequireComponent仍生效）
        _navAgent = GetComponent<NavMeshAgent>();
        // 禁用自动旋转（手动控制旋转匹配目标）
        _navAgent.updateRotation = true;
        // 启用自动更新位置
        _navAgent.updatePosition = true;

    }

    private void Update()
    {
        // Unity 6适配：增加remainingDistance非负判断，避免无效值
        if (_navAgent.isActiveAndEnabled &&
            !_navAgent.pathPending &&
            _navAgent.remainingDistance >= 0)
        {
            // 检测是否到达目标位置
            if (_navAgent.remainingDistance <= arriveDistance)
            {
                CompleteMove();
            }
        }
    }

    /// <summary>
    /// 调用接口：让NPC导航移动到目标位置（适配Unity 6）
    /// </summary>
    /// <param name="target">目标位置Transform</param>
    /// <param name="onComplete">移动结束后的回调</param>
    public void MoveToTarget(Transform target, Action onComplete = null)
    {
        if (target == null || _navAgent == null)
        {
            Debug.LogWarning("目标为空或NavMeshAgent组件缺失！");
            onComplete?.Invoke();
            return;
        }

        // Unity 6适配：验证目标位置是否在导航网格上
        if (!IsPositionOnNavMesh(target.position))
        {
            Debug.LogWarning($"目标位置{target.position}不在导航网格上，无法导航！");
            onComplete?.Invoke();
            return;
        }

        // 保存目标和回调
        _currentTarget = target;
        _onMoveComplete = onComplete;

        // 设置导航目标位置
        _navAgent.SetDestination(target.position);
        // 启用导航代理
        _navAgent.isStopped = false;
    }

    /// <summary>
    /// 调用接口：让NPC直接瞬移到目标位置（匹配位置和旋转）
    /// </summary>
    /// <param name="target">目标位置Transform</param>
    public void TeleportToTarget(Transform target)
    {
        if (target == null || _navAgent == null)
        {
            Debug.LogWarning("目标为空或NavMeshAgent组件缺失！");
            return;
        }

        // 停止导航
        _navAgent.isStopped = true;
        // Unity 6适配：使用ResetPath更安全
        _navAgent.ResetPath();

        // 瞬移到目标位置（Unity 6中Transform赋值逻辑无变化）
        transform.position = target.position;
        // 匹配目标旋转
        transform.rotation = target.rotation;
    }

    /// <summary>
    /// 完成移动：匹配目标旋转并触发回调（Unity 6适配）
    /// </summary>
    private void CompleteMove()
    {
        _navAgent.isStopped = true;
        _navAgent.ResetPath();

        // Unity 6适配：直接使用保存的目标Transform匹配旋转（更精准）
        if (_currentTarget != null)
        {
            // 完全匹配目标的位置和旋转（避免导航误差）
            transform.position = _currentTarget.position;
            transform.rotation = _currentTarget.rotation;
        }

        // 触发结束回调
        _onMoveComplete?.Invoke();
        // 清空回调和目标
        _onMoveComplete = null;
        _currentTarget = null;
    }

    /// <summary>
    /// Unity 6适配：验证位置是否在导航网格上
    /// </summary>
    /// <param name="position">待验证位置</param>
    /// <returns>是否在导航网格上</returns>
    private bool IsPositionOnNavMesh(Vector3 position)
    {
        NavMeshHit hit;
        // 采样导航网格，验证位置有效性
        return NavMesh.SamplePosition(position, out hit, navSampleRadius, NavMesh.AllAreas);
    }

    /// <summary>
    /// 手动停止NPC移动（可选扩展接口）
    /// </summary>
    public void StopMove()
    {
        if (_navAgent != null)
        {
            _navAgent.isStopped = true;
            _navAgent.ResetPath();
            _onMoveComplete = null;
            _currentTarget = null;
        }
    }
}