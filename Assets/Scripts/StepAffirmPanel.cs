using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StepAffirmPanel : MonoSingleton<StepAffirmPanel>
{
    public Button AffirmBtn;
    private Action callback;
    public Transform TargetTra;
    private TextMeshProUGUI buttonText;
    private void Awake()
    {
        if (AffirmBtn != null)
        {
            buttonText = AffirmBtn.GetComponentInChildren<TextMeshProUGUI>();
            AffirmBtn.gameObject.SetActive(false);
        }
    }

    public void Register(string contentStr,Action action)
    {
        if (AffirmBtn == null)
        {
            return;
        }

        AffirmBtn.gameObject.SetActive(true);
        callback = action;
        if (buttonText == null)
        {
            buttonText = AffirmBtn.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (buttonText != null)
        {
            buttonText.text = string.IsNullOrWhiteSpace(contentStr) ? "下一步" : contentStr;
        }

        AffirmBtn.onClick.RemoveAllListeners();
        AffirmBtn.onClick.AddListener(()=> {
            AffirmBtn.gameObject.SetActive(false);
            callback?.Invoke();
        });
    }

    public void Hide()
    {
        if (AffirmBtn == null)
        {
            return;
        }

        callback = null;
        AffirmBtn.onClick.RemoveAllListeners();
        AffirmBtn.gameObject.SetActive(false);
    }
    private void FixedUpdate()
    {
      //  transform.parent.transform.position = new Vector3(transform.parent.transform.position.x, TargetTra.position.y, transform.parent.transform.position.y);
    }
}
