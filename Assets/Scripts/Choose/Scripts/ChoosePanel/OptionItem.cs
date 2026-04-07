using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionItem : MonoBehaviour
{
    private Toggle Tog;
    public TextMeshProUGUI OptionText;
    public TextMeshProUGUI ChooseText;
    public string OptionStr, ContentStr;
    private void Awake()
    {
        Tog = GetComponent<Toggle>();
        Tog.onValueChanged.AddListener((arg)=> {
            if (arg)
            {
                OptionText.color = Color.white;
            }
            else
            {
                OptionText.color = new Color(0.6f,0.82f,0.94f,1); 
            }
        });
    }
    public void SetInfo(string str)
    {
        OptionStr= str.Split('.')[0];
        ContentStr= str.Split('.')[1];
        OptionText.text = str.Split('.')[0];
        ChooseText.text = str.Split('.')[1];
    }
    public bool ToggleState() 
    {
        return Tog.isOn;
    }
}
