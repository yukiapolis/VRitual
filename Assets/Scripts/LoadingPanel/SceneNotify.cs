/*******************************************************************
* Power By Donald
******************************************************************/

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNotify : Singleton<SceneNotify>
{
    /// <summary>
    /// 实训内容
    /// </summary>
    public SkillType SkillType;
    /// <summary>
    /// 实训角色
    /// </summary>
    public RoleType RoleType;

    public void LoadScene(string SceneName, bool affirm = false, Transform rooTransform = null)
    {
        if (rooTransform == null)
        {
            rooTransform = GameObject.Find("Canvas").transform;
        }

        var temp = Resources.Load("Prefabs/UI/LoadingPanel");
        var loadgo = (GameObject)UnityEngine.GameObject.Instantiate(temp, rooTransform);
        loadgo.GetComponent<LoadingPanel>().LoadScene(SceneName, affirm);
    }

}

/// <summary>
/// 实训案例
/// </summary>
public enum SkillType
{
    /// <summary>
    /// 值台
    /// </summary>
    ZT,
    /// <summary>
    /// 传菜
    /// </summary>
    CC,
    /// <summary>
    /// 散客迎宾
    /// </summary>
    SKYB,
    /// <summary>
    /// 团队迎宾
    /// </summary>
    TDYB,
    /// <summary>
    /// 散客电弧预订
    /// </summary>
    SKDHYD,
    /// <summary>
    /// 团队电话预订
    /// </summary>
    TDDHYU,
    /// <summary>
    /// 酒洒身上
    /// </summary>
    JSSS,
    /// <summary>
    /// 桌位已被预订
    /// </summary>
    ZWYBYD,
    /// <summary>
    /// 开餐时间未到
    /// </summary>
    KCSJWD,
    /// <summary>
    /// 客满
    /// </summary>
    KM,
    /// <summary>
    /// 客人洒了酒
    /// </summary>
    KRSLJ,
    /// <summary>
    /// 折扣
    /// </summary>
    ZK,
    /// <summary>
    /// 见经理
    /// </summary>
    JJL,
    /// <summary>
    /// 吃药
    /// </summary>
    CHIYAO,
    /// <summary>
    /// 头发
    /// </summary>
    TF,
    /// <summary>
    /// 所点菜肴没有
    /// </summary>
    SDCYMY,
    /// <summary>
    /// 换座位
    /// </summary>
    HZW,
    /// <summary>
    /// 菜肴时间长
    /// </summary>
    CYSJC,
    /// <summary>
    /// 抽烟
    /// </summary>
    CHOUYAN,
    /// <summary>
    /// 食品投诉
    /// </summary>
    SPTS,
    /// <summary>
    /// 独自等待
    /// </summary>
    DZDD,
    /// <summary>
    /// 钱包丢失
    /// </summary>
    QBDS,
    /// <summary>
    /// 孩子在餐厅跑动
    /// </summary>
    HZZCTPD,
    /// <summary>
    /// 客人滑倒
    /// </summary>
    KRHD,
    /// <summary>
    /// 带宠物到餐厅
    /// </summary>
    DCWDCT,
    /// <summary>
    /// 案例入口
    /// </summary>
    ExamSelect
}
public enum RoleType
{
ZhiTaiYuan,
YingBinYuan,
YuDingYuan,
ChuanCaiYuan,
LingBan,
JingLi

}
