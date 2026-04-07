using UnityEngine;

public class MaleSeatCubeController : MonoBehaviour
{
    [SerializeField] private Transform targetPoint;
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 0.2f, 0f);
    [SerializeField] private Vector3 cubeScale = new Vector3(0.45f, 0.4f, 0.45f);
    [SerializeField] private float backwardOffset = 0.22f;
    [SerializeField] private string cubeName = "MalePlayerSeatCube";

    private GameObject seatCube;

    public void ShowForCurrentPlayer()
    {
        bool isMalePlayer = PublicAtribuite.Instance != null && PublicAtribuite.Instance.CurrentGender == Gender.Male;
        if (!isMalePlayer)
        {
            Hide();
            return;
        }

        if (targetPoint == null)
        {
            targetPoint = GameObject.Find("Player_MaleSeatPoint")?.transform;
        }

        if (targetPoint == null)
        {
            Debug.LogWarning("未找到 Player_MaleSeatPoint，无法创建男性座位Cube。");
            return;
        }

        if (seatCube == null)
        {
            seatCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seatCube.name = cubeName;
            DestroyColliderIfExists(seatCube);
        }

        seatCube.transform.SetParent(null, true);
        seatCube.transform.rotation = targetPoint.rotation;

        Vector3 worldPosition = targetPoint.position;
        worldPosition += targetPoint.TransformDirection(localOffset);
        worldPosition -= targetPoint.forward * backwardOffset;
        worldPosition.y = targetPoint.position.y + (cubeScale.y * 0.5f);

        seatCube.transform.position = worldPosition;
        seatCube.transform.localScale = cubeScale;
        seatCube.SetActive(true);
    }

    public void Hide()
    {
        if (seatCube != null)
        {
            seatCube.SetActive(false);
        }
    }

    private void DestroyColliderIfExists(GameObject target)
    {
        Collider col = target.GetComponent<Collider>();
        if (col != null)
        {
            Destroy(col);
        }
    }
}
