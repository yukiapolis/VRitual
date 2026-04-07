using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<ChooseInfos>().StartAnswer(0,()=> {
            GetComponent<ChooseInfos>().StartAnswer(1, () => {
                Debug.Log("所有答题已经完成了啊！");
            });
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
