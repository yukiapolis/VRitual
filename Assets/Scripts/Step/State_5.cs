using UnityEngine;

public class State_5 : StepBase
{
    public NPCNavController NPC_Male, NPC_Woman;
    private Animator maleAni, womanAni;
    public Transform Woman_InitTra, Woman_Tra1, Woman_Tra2, TableTra;
    public GameObject Table_ShuiBei, Woman_ShuiBei, AnTouIK;
    public GrabItem ShuiBei_Grab;
    private void Awake()
    {
        maleAni = NPC_Male.GetComponent<Animator>();
        womanAni = NPC_Woman.GetComponent<Animator>();
        Table_ShuiBei.SetActive(true);
        Woman_ShuiBei.SetActive(false);
        AnTouIK.SetActive(false);
    }
    protected override void SetStep()
    {
        base.SetStep();
        switch (StepIndex)
        {
            case 0:

                //女性站起
                womanAni.SetBool("SitDown", false);
                DelayedTrigger(1.5f, () => {
                    //---------拿碗和勺子（IK动画）------------

                    womanAni.SetBool("Walk", true);
                    //女性走到男性旁边
                    NPC_Woman.MoveToTarget(TableTra, () => {
                        womanAni.SetBool("Walk", false);
                        //--------------拿取碗动画IK
                        Table_ShuiBei.SetActive(false);
                        Woman_ShuiBei.SetActive(true);

                        //女性拿碗走到男性玩家旁边
                        womanAni.SetBool("Walk", true);
                        DelayedTrigger(0.3f, () => {
                            NPC_Woman.MoveToTarget(Woman_Tra1, () => {
                                womanAni.SetBool("Walk", false);
                                //maleAni.SetBool("Sit",false);
                                //--------------播放喂男性食物IK动画
                                Woman_ShuiBei.GetComponent<Animator>().SetBool("Eat", true);

                                //等待喂食物
                                DelayedTrigger(6f, () => {
                                    Woman_ShuiBei.GetComponent<Animator>().SetBool("Eat", false);
                                    DelayedTrigger(1f, () => {
                                        SetStep();
                                    });
                                });
                            });
                        });
                    });

                });

                break;
            case 1:
                if (PublicAtribuite.Instance.CurrentGender == Gender.Woman)
                { //---------拿碗和勺子（IK动画）------------

                    womanAni.SetBool("Walk", true);
                    //女性走到男性旁边
                    NPC_Woman.MoveToTarget(Woman_Tra2, () => {
                        womanAni.SetBool("Walk", false);
                        //--------------女性玩家自己拿碗来吃
                        ShuiBei_Grab.Open(() => {
                            Woman_ShuiBei.GetComponent<Basin>().enabled = false;
                        }, () => {
                            Woman_ShuiBei.GetComponent<Basin>().enabled = true;
                            SetStep();
                        });
                    });
                }
                else
                {
                    //---------拿碗和勺子（IK动画）------------

                    womanAni.SetBool("Walk", true);
                    //女性走到男性旁边
                    NPC_Woman.MoveToTarget(Woman_Tra2, () => {
                        womanAni.SetBool("Walk", false);
                        //--------------播放喂食物IK动画
                        RegisterStepReminder(() => {
                            Woman_ShuiBei.GetComponent<Animator>().SetBool("Eat", true);

                            //等待喂食物
                            DelayedTrigger(6f, () =>
                            {
                                Woman_ShuiBei.GetComponent<Animator>().SetBool("Eat", false);
                                SetStep();
                            });
                        });
                    //--------------播放喂男性食物IK动画
                   
                    });
                }
                break;
            case 2:
                //放下盆
                womanAni.SetBool("Walk", true);
                NPC_Woman.MoveToTarget(TableTra, () => {
                    womanAni.SetBool("Walk", false);
                    //---------关闭女性拿碗IK动画------------

                    Table_ShuiBei.SetActive(true);
                    Woman_ShuiBei.SetActive(false);
                    DelayedTrigger(0.5f, () => {

                        //如果是女玩家，则回到原来位置
                        womanAni.SetBool("Walk", true);
                        NPC_Woman.MoveToTarget(Woman_InitTra, () => {
                            womanAni.SetBool("Walk", false);
                            //女性坐地上
                            womanAni.SetBool("SitDown", true);

                            DelayedTrigger(2f, () => {
                                SetStep();
                            });
                        });
                    });

                });
                break;
            case 3:
                //------------男性按女性头三下(---IK动画)----------
                AnTouIK.SetActive(true);
                womanAni.SetTrigger("WanYao");
                DelayedTrigger(2f, () => {
                    womanAni.SetTrigger("WanYao");
                    DelayedTrigger(2f, () => {
                        womanAni.SetTrigger("WanYao");

                        DelayedTrigger(2f, () => {
                            AnTouIK.SetActive(false);
                            OnEnd();
                        });
                    });
                });
                break;
        }
    }
}

