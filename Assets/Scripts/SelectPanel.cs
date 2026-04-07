using UnityEngine;
using UnityEngine.UI;

public class SelectPanel : MonoBehaviour
{
    public Button MaleBtn, WomanBtn;
    private void Awake()
    {
        MaleBtn.onClick.AddListener(()=> {
            PublicAtribuite.Instance.CurrentGender = Gender.Male;
            SceneNotify.Instance.LoadScene("Main");
        });
        WomanBtn.onClick.AddListener(() => {
            PublicAtribuite.Instance.CurrentGender = Gender.Woman;
            SceneNotify.Instance.LoadScene("Main");
        });


    }
}
