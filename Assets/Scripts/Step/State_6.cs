using System.Collections.Generic;
using UnityEngine;

public class State_6 : StepBase
{
    public NPCNavController NPC_Male, NPC_Woman;
    private Animator maleAni, womanAni;
    public Transform Male_InitTra, MaleTra1, Woman_InitTra, Woman_Tra1, Leave_Male,Leave_Woman, TableTra;
    public UniversalIKController MaleIK;
    public Transform Head, LeftJB, RightJB;
    public List<GameObject> HideList;
    public GameObject Woman_Pen;
    private void Awake()
    {
        maleAni = NPC_Male.GetComponent<Animator>();
        womanAni = NPC_Woman.GetComponent<Animator>();
    }
    protected override void SetStep()
    {
        base.SetStep();
        switch (StepIndex)
        {
            case 0:
                if (PublicAtribuite.Instance.CurrentGender == Gender.Woman)
                {
                    //女性站起
                    womanAni.SetBool("SitDown", false);
                    DelayedTrigger(1.5f, () => {
                        //女性走到玩家旁
                        womanAni.SetBool("Walk", true);
                        NPC_Woman.MoveToTarget(Woman_Tra1, () => {
                            womanAni.SetBool("Walk", false);
                            //女性引导玩家起身
                            womanAni.SetTrigger("TaiShou");
                            DelayedTrigger(1, () => {
                                PlayerController.Instance.MoveCameraToPosition("ZL", () => {
                                    DelayedTrigger(1, () => {
                                        //女性触脚动作
                                        womanAni.SetBool("DunXia", true);
                                        DelayedTrigger(2, () => {
                                            womanAni.SetBool("DunXia", false);
                                            //玩家蹲下
                                            PlayerController.Instance.MoveCameraToPosition("DX");
                                            SetStep();
                                        });
                                    });
                                });
                            });

                        });
                    });
                }
                else
                {
                    //男性站起来走到玩家前面
                    maleAni.SetBool("Sit", false);
                    DelayedTrigger(1f, () => {
                        maleAni.SetBool("Walk", true);
                        NPC_Male.MoveToTarget(MaleTra1, () => {
                            maleAni.SetBool("Walk", false);
                            maleAni.SetTrigger("TaiShou");
                            DelayedTrigger(0.5f, () =>
                            {
                                PlayerController.Instance.MoveCameraToPosition("ZL");
                            });
                        DelayedTrigger(1f, () => {
                            //--------------播放IK肩搭肩头对头动作------------
                            MaleIK.SetHeadTarget(Head);
                            MaleIK.SetHandTarget(true, RightJB);
                            MaleIK.SetHandTarget(false, LeftJB);
                            DelayedTrigger(3, () =>
                            {
                                //------------关闭IK肩搭肩头对头动作------------
                                MaleIK.SetHeadTarget(Head, false);
                                MaleIK.SetHandTarget(true, RightJB, false);
                                MaleIK.SetHandTarget(false, LeftJB, false);
                                maleAni.SetTrigger("TaiShou");
                                DelayedTrigger(1.8f, () => {
                                    PlayerController.Instance.MoveCameraToPosition("ZX");
                                    SetStep();
                                });
                            });
                        });
                           
                        });
                    });
                }
                break;
            case 1:
                DelayedTrigger(2, () => {
                    //女性玩家回到位置坐地上
                    if (PublicAtribuite.Instance.CurrentGender == Gender.Woman)
                    {
                        womanAni.SetBool("Walk", true);
                        NPC_Woman.MoveToTarget(Woman_InitTra, () =>
                        {
                            womanAni.SetBool("Walk", false);
                            womanAni.SetBool("SitDown", true);
                            DelayedTrigger(3f, () => { SetStep(); });
                        });
                    }
                    else
                    {
                        //男性玩家回到位置坐下
                        maleAni.SetBool("Walk", true);
                        NPC_Male.MoveToTarget(Male_InitTra, () => {
                            maleAni.SetBool("Walk", false);
                            maleAni.SetBool("Sit", true);
                            DelayedTrigger(3f, () => { SetStep(); });
                        });
                    }
                });
                break;
            case 2:
                Debug.Log("执行离场动作"); 
                //男性先站起来离开
                maleAni.SetBool("Sit", false);
                DelayedTrigger(1f, () =>
                {
                    maleAni.SetBool("Walk", true);
                    NPC_Male.MoveToTarget(Leave_Male, () =>
                    {
                        maleAni.SetBool("Walk", false);
                    });
                });
                //女性拿物品后离开
                //女性站起
                womanAni.SetBool("SitDown", false);
                DelayedTrigger(2f, () => {
                    //---------女性走到桌子边端盆（播放端盆IK）------------
                    womanAni.SetBool("Walk", true);
                    NPC_Woman.MoveToTarget(TableTra, () => {
                        womanAni.SetBool("Walk", false);
                        //--------------拿起盆
                        Woman_Pen.SetActive(true);
                        foreach (var go in HideList) 
                        {
                            go.SetActive(false);
                        }
                        //女性端盆走到男性玩家旁边
                        womanAni.SetBool("Walk", true);
                        DelayedTrigger(0.3f, () => {
                            NPC_Woman.MoveToTarget(Leave_Woman, () => {
                                womanAni.SetBool("Walk", false);
                                SetStep();
                            });
                        });

                    });


                });
                break;
            case 3:
                SceneNotify.Instance.LoadScene("Select");
                break;
        }
    }
}
