
using System;
using UnityEngine;
using UnityEngine.UI;

public class DialogPanel : MonoSingleton<DialogPanel>
{
    public GameObject DialogGo;

    public enum DialogType
    {
        affirmAndcancel,
        affirm,
    }

    public Text TitleText;
    public Text ContentText;

    public Button AffirmBtn;
    public Button CancelBtn;

    public Button CloseBtn;

    private DialogType dialogType;

    private void Awake()
    {
        CloseBtn.onClick.AddListener((() =>
        {
            DialogGo.SetActive(false);
        }));
    }

    public void ShowDialog(string titleStr, string contentStr, Action<bool> callback, DialogType DT = DialogType.affirmAndcancel,AudioClip clip=null)
    {
      //  CloseBtn.gameObject.SetActive(true);
        DialogGo.SetActive(true);
        TitleText.text = titleStr;
        ContentText.text = contentStr;

        dialogType = DT;

        if (DT == DialogType.affirmAndcancel)
        {
            AffirmBtn.gameObject.SetActive(true);
            CancelBtn.gameObject.SetActive(true);
        }
        else
        {
            CancelBtn.gameObject.SetActive(false);
        }
        AffirmBtn.onClick.AddListener(() =>
        {
            DialogClose();
            AffirmBtn.onClick.RemoveAllListeners();
            CancelBtn.onClick.RemoveAllListeners();
            callback?.Invoke(true);
        });
        CancelBtn.onClick.AddListener(() =>
        {
            DialogClose();
            CancelBtn.onClick.RemoveAllListeners();
            AffirmBtn.onClick.RemoveAllListeners();
            callback?.Invoke(false);
        });
        if (clip) 
            AudioManager.Instance.ASPlayer(clip);
    }
    public void ShowDialog(string SubmitName,string titleStr, string contentStr, Action<bool> callback, DialogType DT = DialogType.affirmAndcancel)
    {
        AffirmBtn.GetComponentInChildren<Text>().text = SubmitName;
        DialogGo.SetActive(true);
        TitleText.text = titleStr;
        ContentText.text = contentStr;

        dialogType = DT;

        if (DT == DialogType.affirmAndcancel)
        {
            AffirmBtn.gameObject.SetActive(true);
            CancelBtn.gameObject.SetActive(true);
        }
        else
        {
            CancelBtn.gameObject.SetActive(false);
        }
        AffirmBtn.onClick.AddListener(() =>
        {
            callback?.Invoke(true);
            DialogClose();
            CancelBtn.onClick.RemoveAllListeners();
            AffirmBtn.onClick.RemoveAllListeners();
        });
        CancelBtn.onClick.AddListener(() =>
        {
            callback?.Invoke(false);
            DialogClose();
            CancelBtn.onClick.RemoveAllListeners();
            AffirmBtn.onClick.RemoveAllListeners();
        });
    }
    public void ShowDialog(string titleStr, string contentStr, Action<bool> callback, bool state = false, DialogType DT = DialogType.affirm)
    {
        CloseBtn.gameObject.SetActive(false);
        DialogGo.SetActive(true);
        TitleText.text = titleStr;
        ContentText.text = contentStr;

        dialogType = DT;

        if (DT == DialogType.affirmAndcancel)
        {
            AffirmBtn.gameObject.SetActive(true);
            CancelBtn.gameObject.SetActive(true);
        }
        else
        {
            CancelBtn.gameObject.SetActive(false);
        }
        AffirmBtn.onClick.AddListener(() =>
        {
            callback?.Invoke(true);
            DialogClose();
            CancelBtn.onClick.RemoveAllListeners();
            AffirmBtn.onClick.RemoveAllListeners();
        });
        CancelBtn.onClick.AddListener(() =>
        {
            callback?.Invoke(false);
            DialogClose();
            CancelBtn.onClick.RemoveAllListeners();
            AffirmBtn.onClick.RemoveAllListeners();
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (dialogType == DialogType.affirm)
            {
                AffirmBtn.onClick.Invoke();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
                DialogGo.SetActive(false);
        }
    }

    private void DialogClose()
    {
        DialogGo.SetActive(false);
        AffirmBtn.GetComponentInChildren<Text>().text = "确   认";
    }
}