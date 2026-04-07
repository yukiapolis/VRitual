using UnityEngine;

public class DebugSeatCube : MonoBehaviour
{
    [SerializeField] private Transform targetPoint;
    [SerializeField] private Vector3 localOffset = Vector3.zero;
    [SerializeField] private Vector3 cubeScale = new Vector3(0.45f, 0.55f, 0.45f);
    [SerializeField] private bool createOnStart = false;

    private GameObject debugCube;

    private void Start()
    {
        if (createOnStart)
        {
            CreateCube();
        }
    }

    [ContextMenu("Create Debug Seat Cube")]
    public void CreateCube()
    {
        if (targetPoint == null)
        {
            targetPoint = GameObject.Find("Player_MaleSeatPoint")?.transform;
        }

        if (targetPoint == null)
        {
            Debug.LogWarning("未找到 Player_MaleSeatPoint，无法创建调试Cube。");
            return;
        }

        if (debugCube == null)
        {
            debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            debugCube.name = "Debug_PlayerMaleSeatCube";
        }

        debugCube.transform.SetParent(targetPoint, false);
        Vector3 adjustedOffset = localOffset;
        adjustedOffset.y += cubeScale.y * 0.5f;
        debugCube.transform.localPosition = adjustedOffset;
        debugCube.transform.localRotation = Quaternion.identity;
        debugCube.transform.localScale = cubeScale;
    }
}
