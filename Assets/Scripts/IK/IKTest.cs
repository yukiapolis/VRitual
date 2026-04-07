// 示例：在其他脚本中动态控制IK
using UnityEngine;

public class IKTest : MonoBehaviour
{
    public UniversalIKController ikController;
    public Transform newRightHandTarget;
    public Transform newHeadTarget;

    void Update()
    {
        // 按下1键：启用右手IK并切换目标
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ikController.SetHandTarget(false, newRightHandTarget, true);
        }

        // 按下2键：启用头部IK看向目标
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ikController.SetHeadTarget(newHeadTarget, true);
        }

        // 按下3键：禁用所有IK
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ikController.DisableAllIK();
        }
    }
}