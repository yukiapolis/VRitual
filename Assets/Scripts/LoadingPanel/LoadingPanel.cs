/*******************************************************************
* Power By Donald
******************************************************************/

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingPanel : MonoSingleton<LoadingPanel>
{
    public Slider slider;
    public Text ProgressText;
    public Text TipText;

    private float progressValue;
    private AsyncOperation async = null;

    public void LoadScene(string nextSceneName, bool affirm = true)
    {

        async = null;
        StartCoroutine(ILoadScene(nextSceneName,affirm));
    }

    private IEnumerator ILoadScene(string nextSceneName, bool affirm = true)
    {
        async = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Single);
        async.allowSceneActivation = false;
        while (!async.isDone)
        {
            if (async.progress < 0.9f)
                progressValue = async.progress;
            else
                progressValue = 1.0f;

            slider.value = progressValue;
            ProgressText.text = (int)(slider.value * 100) + " %";

            if (progressValue >= 0.95f)
            {
                if (affirm)
                {
                    TipText.text = "按任意键继续";
                    if (Input.anyKeyDown)
                    {
                        async.allowSceneActivation = true;
                    }
                }
                else
                {
                    async.allowSceneActivation = true;
                }
            }
            yield return null;
        }
    }
}