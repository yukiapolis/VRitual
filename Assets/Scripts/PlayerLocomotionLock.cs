using System.Collections.Generic;
using UnityEngine;

public class PlayerLocomotionLock : MonoBehaviour
{
    [Header("Rig根节点")]
    [SerializeField] private Transform rigRoot;

    [Header("Locomotion根节点")]
    [SerializeField] private Transform locomotionRoot;

    [Header("运行时缓存")]
    [SerializeField] private List<Behaviour> lockedBehaviours = new List<Behaviour>();

    private readonly List<Behaviour> cachedBehaviours = new List<Behaviour>();
    private CharacterController rigCharacterController;
    private Vector3 lockedPosition;
    private bool lockMovement;
    private bool suspendPositionLock;

    private void Awake()
    {
        if (rigRoot == null)
        {
            rigRoot = transform.Find("XR Origin (XR Rig)");
        }

        if (locomotionRoot == null && rigRoot != null)
        {
            locomotionRoot = rigRoot.Find("Locomotion");
        }

        if (rigRoot != null)
        {
            rigCharacterController = rigRoot.GetComponent<CharacterController>();
        }

        CacheBehaviours();
    }

    private void LateUpdate()
    {
        if (!lockMovement || suspendPositionLock || rigRoot == null)
        {
            return;
        }

        Vector3 currentPosition = rigRoot.position;
        rigRoot.position = new Vector3(lockedPosition.x, currentPosition.y, lockedPosition.z);
    }

    public void SetMovementLocked(bool locked)
    {
        CacheBehaviours();

        lockMovement = locked;
        suspendPositionLock = false;
        if (rigRoot != null && locked)
        {
            lockedPosition = rigRoot.position;
        }

        for (int i = 0; i < cachedBehaviours.Count; i++)
        {
            Behaviour behaviour = cachedBehaviours[i];
            if (behaviour == null)
            {
                continue;
            }

            behaviour.enabled = !locked;
        }

        if (rigCharacterController != null)
        {
            rigCharacterController.enabled = !locked;
        }
    }

    public void SuspendPositionLock(bool suspend)
    {
        suspendPositionLock = suspend;

        if (!suspend && rigRoot != null && lockMovement)
        {
            lockedPosition = rigRoot.position;
        }
    }

    private void CacheBehaviours()
    {
        cachedBehaviours.Clear();

        if (locomotionRoot == null)
        {
            lockedBehaviours = new List<Behaviour>();
            return;
        }

        AddBehavioursFromNamedChild("Move");
        AddBehavioursFromNamedChild("Turn");
        AddBehavioursFromNamedChild("Grab Move");
        AddBehavioursFromNamedChild("Teleportation");
        AddBehavioursFromNamedChild("Climb");
        AddBehavioursFromNamedChild("Gravity");
        AddBehavioursFromNamedChild("Jump");

        foreach (var behaviour in locomotionRoot.GetComponentsInChildren<Behaviour>(true))
        {
            if (ShouldLockBehaviour(behaviour))
            {
                AddBehaviour(behaviour);
            }
        }

        lockedBehaviours = new List<Behaviour>(cachedBehaviours);
    }

    private void AddBehavioursFromNamedChild(string childName)
    {
        if (locomotionRoot == null)
        {
            return;
        }

        Transform child = locomotionRoot.Find(childName);
        if (child == null)
        {
            return;
        }

        foreach (var behaviour in child.GetComponents<Behaviour>())
        {
            AddBehaviour(behaviour);
        }

        foreach (var behaviour in child.GetComponentsInChildren<Behaviour>(true))
        {
            AddBehaviour(behaviour);
        }
    }

    private void AddBehaviour(Behaviour behaviour)
    {
        if (behaviour == null || behaviour == this)
        {
            return;
        }

        if (!cachedBehaviours.Contains(behaviour))
        {
            cachedBehaviours.Add(behaviour);
        }
    }

    private bool ShouldLockBehaviour(Behaviour behaviour)
    {
        if (behaviour == null || behaviour == this)
        {
            return false;
        }

        string typeName = behaviour.GetType().Name;
        return typeName.Contains("MoveProvider")
            || typeName.Contains("TurnProvider")
            || typeName.Contains("TeleportationProvider")
            || typeName.Contains("ClimbProvider")
            || typeName.Contains("GrabMoveProvider")
            || typeName.Contains("LocomotionMediator")
            || typeName.Contains("LocomotionSystem")
            || typeName.Contains("BodyPositionEvaluator")
            || typeName.Contains("BodyGroundPosition")
            || typeName.Contains("UnderCameraBodyPosition")
            || typeName.Contains("JumpProvider")
            || typeName.Contains("DynamicMoveProvider");
    }
}
