using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

public class M_GameManager : MonoBehaviour
{
    public NavMeshAgent Npc_AiNan;
    public Transform Pos_NpcNanSit;

    public NavMeshAgent Npc_AiNv;
    public Transform Pos_NpcNvSit;
    bool IsNvFinishMove;

    public GameObject Npc_AiNv2;
    // Start is called before the first frame update
    void Start()
    {
        Npc_AiNan.gameObject.GetComponent<Animator>().SetBool("IsWalk", true);
        Npc_AiNan.SetDestination(Pos_NpcNanSit.position);

        Npc_AiNv.gameObject.GetComponent<Animator>().SetBool("IsWalk", true);
        Npc_AiNv.SetDestination(Pos_NpcNvSit.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (Npc_AiNan.remainingDistance <= Npc_AiNan.stoppingDistance+0.05f)
        {
            Npc_AiNan.gameObject.GetComponent<Animator>().SetBool("IsWalk", false);
            Npc_AiNan.enabled = false;
            Npc_AiNan.transform.DORotate(new Vector3(0, 0, 0), 1).OnComplete(() =>
            {
                Npc_AiNan.gameObject.GetComponent<Animator>().SetTrigger("sit");
            });
           
        }

        if (Npc_AiNv.remainingDistance <= Npc_AiNv.stoppingDistance + 0.05f&&!IsNvFinishMove)
        {
            Npc_AiNv.gameObject.GetComponent<Animator>().SetBool("IsWalk", false);
            Npc_AiNv.enabled = false;
            Npc_AiNv.gameObject.GetComponent<Animator>().SetTrigger("sit");
            IsNvFinishMove = true;
            Invoke("NvStartFeed",1);
            //Npc_AiNan.transform.DORotate(new Vector3(0, 0, 0), 1).OnComplete(() =>
            //{

            //});

        }
    }

    void NvStartFeed()
    {
        Npc_AiNv.gameObject.GetComponent<Animator>().SetTrigger("SitToIdle");
        Invoke("ShowFeedModel", 3.5f);
    }

    void ShowFeedModel()
    {
        Npc_AiNv.gameObject.SetActive(false);
        Npc_AiNv2.gameObject.SetActive(true);
    }
}
