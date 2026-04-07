using System;
using UnityEngine;

public class State_1 : StepBase
{
    public NPCNavController NPC_Male, NPC_Woman;
    private Animator maleAni, womanAni;
    public Transform Male_InitTra, Woman_InitTra;
    [Header("玩家开始站位")]
    public Transform Male_PlayerStandTra;
    public Transform Female_PlayerStandTra;

    private bool maleHostReady;
    private bool femaleHostReady;
    private bool playerReady;
    private bool hasAdvanced;
    private bool playerFinalPoseStarted;
    private bool initialPlacementDone;
    private bool isActiveStep;
    private MaleSeatCubeController maleSeatCubeController;

    private void Awake()
    {
        maleAni = NPC_Male.GetComponent<Animator>();
        womanAni = NPC_Woman.GetComponent<Animator>();
        maleSeatCubeController = FindObjectOfType<MaleSeatCubeController>();
    }
    public override void OnStart()
    {
        isActiveStep = true;
        base.OnStart();
    }

    public override void OnEnd()
    {
        isActiveStep = false;
        base.OnEnd();
    }

    protected override void SetStep()
    {
        base.SetStep();
        switch (StepIndex) 
        {
            case 0:
                if (!isActiveStep)
                {
                    return;
                }

                if (initialPlacementDone && (playerFinalPoseStarted || hasAdvanced))
                {
                    Debug.Log("State_1 case 0 被重入，跳过重复的开场站位和坐姿流程。");
                    return;
                }

                bool isMalePlayer = PublicAtribuite.Instance.CurrentGender == Gender.Male;
                Transform playerStandTra = isMalePlayer ? Male_PlayerStandTra : Female_PlayerStandTra;

                if (!initialPlacementDone)
                {
                    maleHostReady = false;
                    femaleHostReady = false;
                    playerReady = false;
                    hasAdvanced = false;
                    playerFinalPoseStarted = false;

                    maleSeatCubeController?.Hide();

                    if (PlayerController.Instance != null && PlayerController.Instance.poseController != null)
                    {
                        if (playerStandTra != null)
                        {
                            PlayerController.Instance.poseController.AlignHeightToAnchor(playerStandTra);
                        }
                        PlayerController.Instance.poseController.ApplyPose(PlayerPoseController.PlayerPose.Stand);
                    }
                    else if (PlayerController.Instance == null)
                    {
                        playerReady = true;
                    }

                    initialPlacementDone = true;
                }

                //男性走到座位旁坐下
                maleAni.SetBool("Walk", true);
                NPC_Male.MoveToTarget(Male_InitTra,()=> {
                    maleAni.SetBool("Walk", false);
                    maleAni.SetBool("Sit", true);

                    DelayedTrigger(0.6f, () => {
                        if (!isActiveStep || hasAdvanced) return;
                        maleHostReady = true;
                        Debug.Log($"State_1: maleHostReady = true, femaleHostReady = {femaleHostReady}, playerReady = {playerReady}");

                        if (isMalePlayer && PlayerController.Instance != null)
                        {
                            StartPlayerFinalPoseOnce(PlayerPoseController.PlayerPose.Sit, PlayerController.Instance.playerMaleSeatPoint, "ZX");
                        }
                        else
                        {
                            TryAdvanceToNextStep();
                        }
                    });
                });
                //女性走到座位旁坐地上
                DelayedTrigger(0.5f,()=> {
                    if (!isActiveStep || hasAdvanced) return;
                    womanAni.SetBool("Walk", true);
                    NPC_Woman.MoveToTarget(Woman_InitTra, () => {
                        if (!isActiveStep || hasAdvanced) return;
                        womanAni.SetBool("Walk", false);
                        womanAni.SetBool("SitDown", true);

                        DelayedTrigger(0.8f, () => {
                            if (!isActiveStep || hasAdvanced) return;
                            femaleHostReady = true;
                            Debug.Log($"State_1: femaleHostReady = true, maleHostReady = {maleHostReady}, playerReady = {playerReady}");

                            if (!isMalePlayer && PlayerController.Instance != null)
                            {
                                Debug.Log($"State_1 FemaleFlow: 准备触发女性玩家蹲地。anchor={(PlayerController.Instance.playerFemaleFloorPoint != null ? PlayerController.Instance.playerFemaleFloorPoint.name : "null")}, anchorPos={(PlayerController.Instance.playerFemaleFloorPoint != null ? PlayerController.Instance.playerFemaleFloorPoint.position.ToString() : "null")}");

                                if (PlayerController.Instance.playerAnimator != null)
                                {
                                    PlayerController.Instance.playerAnimator.SetBool("Sit", false);
                                    PlayerController.Instance.playerAnimator.SetBool("DunXia", false);
                                    Debug.Log("State_1 FemaleFlow: 已重置玩家 Animator 的 Sit=false, DunXia=false");
                                }
                                else
                                {
                                    Debug.LogWarning("State_1 FemaleFlow: PlayerController.Instance.playerAnimator 为 null");
                                }

                                StartPlayerFinalPoseOnce(PlayerPoseController.PlayerPose.Crouch, PlayerController.Instance.playerFemaleFloorPoint, "DX");
                            }
                            else
                            {
                                TryAdvanceToNextStep();
                            }
                        });
                    });
                });
                break;
            case 1:
                break;
            case 2:
                break;
        }
    }

    private void StartPlayerFinalPoseOnce(PlayerPoseController.PlayerPose pose, Transform poseAnchor, string cameraCommand)
    {
        if (!isActiveStep || playerFinalPoseStarted)
        {
            return;
        }

        playerFinalPoseStarted = true;
        Debug.Log($"State_1.StartPlayerFinalPoseOnce: pose={pose}, anchor={(poseAnchor != null ? poseAnchor.name : "null")}, anchorPos={(poseAnchor != null ? poseAnchor.position.ToString() : "null")}, cameraCommand={cameraCommand}");
        MovePlayerToFinalPose(pose, poseAnchor, cameraCommand, () => {
            if (!isActiveStep || hasAdvanced) return;
            playerReady = true;
            Debug.Log($"State_1: playerReady = true, maleHostReady = {maleHostReady}, femaleHostReady = {femaleHostReady}");
            TryAdvanceToNextStep();
        });
    }

    private void MovePlayerToFinalPose(PlayerPoseController.PlayerPose pose, Transform poseAnchor, string cameraCommand, Action onComplete)
    {
        if (PlayerController.Instance == null)
        {
            onComplete?.Invoke();
            return;
        }

        if (PlayerController.Instance.poseController != null)
        {
            Debug.Log($"State_1.MovePlayerToFinalPose: pose={pose}, anchor={(poseAnchor != null ? poseAnchor.name : "null")}, anchorPos={(poseAnchor != null ? poseAnchor.position.ToString() : "null")}, playerCurrentPos={PlayerController.Instance.playerRigRoot.position}");
            PlayerController.Instance.poseController.MoveToPose(pose, poseAnchor, () => {
                Debug.Log($"State_1.MovePlayerToFinalPose Completed: pose={pose}, playerCurrentPos={PlayerController.Instance.playerRigRoot.position}");

                if (pose == PlayerPoseController.PlayerPose.Sit)
                {
                    maleSeatCubeController?.ShowForCurrentPlayer();
                }
                else
                {
                    maleSeatCubeController?.Hide();
                }

                float settleDelay = pose == PlayerPoseController.PlayerPose.Crouch ? 1.1f : 0.8f;
                DelayedTrigger(settleDelay, () => {
                    Debug.Log($"State_1.MovePlayerToFinalPose SettleDone: pose={pose}");
                    onComplete?.Invoke();
                });
            });
            return;
        }

        PlayerController.Instance.MoveCameraOnly(cameraCommand, () => {
            DelayedTrigger(0.8f, () => {
                onComplete?.Invoke();
            });
        });
    }

    private void TryAdvanceToNextStep()
    {
        Debug.Log($"State_1.TryAdvanceToNextStep -> maleHostReady={maleHostReady}, femaleHostReady={femaleHostReady}, playerReady={playerReady}, hasAdvanced={hasAdvanced}");

        if (hasAdvanced)
        {
            return;
        }

        if (!maleHostReady || !femaleHostReady || !playerReady)
        {
            return;
        }

        hasAdvanced = true;
        Debug.Log("第一步结束");
        StepController.Instance.NextStep();
    }

}
