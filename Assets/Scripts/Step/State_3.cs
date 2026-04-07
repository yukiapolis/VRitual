using UnityEngine;

public class State3 : StepBase
{
    public NPCNavController NPC_Male, NPC_Woman;
    private Animator maleAni, womanAni;
    public Transform  Woman_InitTra, Woman_Tra1, Woman_Tr2,TableTra;
    public GameObject Woman_Pen, Table_Pen, XiShouIK,CaShouIK,AnTouIK;
    private void Awake()
    {
        maleAni = NPC_Male.GetComponent<Animator>();
        womanAni = NPC_Woman.GetComponent<Animator>();
        Woman_Pen.SetActive(false);
        Table_Pen.SetActive(true);
        XiShouIK.SetActive(false);
        CaShouIK.SetActive(false);
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
                    //---------女性走到桌子边端盆（播放端盆IK）------------
                    womanAni.SetBool("Walk", true);
                    NPC_Woman.MoveToTarget(TableTra, () => {
                        womanAni.SetBool("Walk", false);
                        //--------------拿起盆
                        Woman_Pen.SetActive(true);
                        Table_Pen.SetActive(false);
                        //女性端盆走到男性玩家旁边
                        womanAni.SetBool("Walk", true);
                        DelayedTrigger(0.3f,()=> {
                            NPC_Woman.MoveToTarget(Woman_Tra1, () => {
                                womanAni.SetBool("Walk", false);
                                //maleAni.SetBool("Sit",false);
                                //--------------播放男性洗手擦手IK动画
                                XiShouIK.SetActive(true);

                                //等待洗手结束
                                DelayedTrigger(3f, () => {
                                    XiShouIK.SetActive(false);
                                    CaShouIK.SetActive(true);
                                    //擦手
                                    DelayedTrigger(3f, () => {
                                        CaShouIK.SetActive(false);
                                        //maleAni.SetBool("Sit", true);
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
                {
                    //放下盆
                    womanAni.SetBool("Walk", true);
                    NPC_Woman.MoveToTarget(TableTra, () => {
                        womanAni.SetBool("Walk", false);
                        //---------关闭女性端盆IK动画------------
                        Woman_Pen.SetActive(false);
                        Table_Pen.SetActive(true);
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
                    
                }
                else
                {
                    //如果是玩家，则端盆到男玩家位置
                    womanAni.SetBool("Walk", true);
                    //女性走到男性玩家旁边
                    NPC_Woman.MoveToTarget(Woman_Tr2, () => {
                        womanAni.SetBool("Walk", false);
                        //----男玩家洗手（看是否要加触发）
                        RegisterStepReminder(() => {
                            DelayedTrigger(3f, () => {
                                //男玩家洗完手，女性回到原来位置
                                //放下盆
                                womanAni.SetBool("Walk", true);
                                NPC_Woman.MoveToTarget(TableTra, () => {
                                    womanAni.SetBool("Walk", false);
                                    //---------关闭女性端盆IK动画------------
                                    Woman_Pen.SetActive(false);
                                    Table_Pen.SetActive(true);
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
                            });
                        });
                       
                    });
                }
                break;
            case 2:
                //------------男性按女性头三下(---IK动画)----------
                AnTouIK.SetActive(true);
                womanAni.SetTrigger("WanYao");
                DelayedTrigger(2f,()=> {
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
