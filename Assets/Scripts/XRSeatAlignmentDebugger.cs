using UnityEngine;

public class XRSeatAlignmentDebugger : MonoBehaviour
{
    [SerializeField] private Transform rigRoot;
    [SerializeField] private Transform trackedHead;
    [SerializeField] private Transform maleSeatAnchor;
    [SerializeField] private Transform femaleFloorAnchor;
    [SerializeField] private bool logOnStart = true;

    private void Start()
    {
        AutoAssign();

        if (logOnStart)
        {
            LogState("Start");
        }
    }

    [ContextMenu("Log XR Seat State")]
    public void LogCurrentState()
    {
        AutoAssign();
        LogState("Manual");
    }

    public void LogState(string label)
    {
        AutoAssign();

        Transform activeAnchor = GetActiveAnchor();
        string anchorName = activeAnchor != null ? activeAnchor.name : "null";
        Vector3 rigPos = rigRoot != null ? rigRoot.position : Vector3.zero;
        Vector3 headWorldPos = trackedHead != null ? trackedHead.position : Vector3.zero;
        Vector3 headLocalPos = trackedHead != null ? trackedHead.localPosition : Vector3.zero;
        Vector3 anchorPos = activeAnchor != null ? activeAnchor.position : Vector3.zero;
        Vector3 headOffset = (rigRoot != null && trackedHead != null) ? trackedHead.position - rigRoot.position : Vector3.zero;

        Debug.Log($"[XRSeatAlignmentDebugger] {label} | gender={(PublicAtribuite.Instance != null ? PublicAtribuite.Instance.CurrentGender.ToString() : "Unknown")} | activeAnchor={anchorName} | rigPos={rigPos} | headWorld={headWorldPos} | headLocal={headLocalPos} | anchorPos={anchorPos} | headOffset={headOffset}");
    }

    private Transform GetActiveAnchor()
    {
        bool isMalePlayer = PublicAtribuite.Instance != null && PublicAtribuite.Instance.CurrentGender == Gender.Male;
        return isMalePlayer ? maleSeatAnchor : femaleFloorAnchor;
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

        if (maleSeatAnchor == null)
        {
            maleSeatAnchor = GameObject.Find("Player_MaleSeatPoint")?.transform;
        }

        if (femaleFloorAnchor == null)
        {
            femaleFloorAnchor = GameObject.Find("Player_FemaleFloorPoint")?.transform;
        }
    }

    public void LogState(string label, Transform callerRoot, Transform actualAnchor)
    {
        AutoAssign();

        Transform activeAnchor = actualAnchor != null ? actualAnchor : GetActiveAnchor();
        string anchorName = activeAnchor != null ? activeAnchor.name : "null";
        string callerName = callerRoot != null ? callerRoot.name : "null";
        Vector3 rigPos = rigRoot != null ? rigRoot.position : Vector3.zero;
        Vector3 headWorldPos = trackedHead != null ? trackedHead.position : Vector3.zero;
        Vector3 headLocalPos = trackedHead != null ? trackedHead.localPosition : Vector3.zero;
        Vector3 anchorPos = activeAnchor != null ? activeAnchor.position : Vector3.zero;
        Vector3 headOffset = (rigRoot != null && trackedHead != null) ? trackedHead.position - rigRoot.position : Vector3.zero;

        Debug.Log($"[XRSeatAlignmentDebugger] {label} | caller={callerName} | trackedHead={(trackedHead != null ? trackedHead.name : "null")} | rigRoot={(rigRoot != null ? rigRoot.name : "null")} | gender={(PublicAtribuite.Instance != null ? PublicAtribuite.Instance.CurrentGender.ToString() : "Unknown")} | activeAnchor={anchorName} | rigPos={rigPos} | headWorld={headWorldPos} | headLocal={headLocalPos} | anchorPos={anchorPos} | headOffset={headOffset}");
    }
}
