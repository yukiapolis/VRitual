using UnityEngine;

public class Basin : MonoBehaviour
{
    public UniversalIKController WomanIK;
    public Transform LeftTra, RightTra;
    private void OnEnable()
    {
        if(LeftTra!=null)
            WomanIK.SetHandTarget(true, LeftTra);
        if (RightTra != null)
            WomanIK.SetHandTarget(false, RightTra);
    }
    private void OnDisable()
    {
        if (LeftTra != null)
            WomanIK.SetHandTarget(true, LeftTra,false);
        if (RightTra != null)
            WomanIK.SetHandTarget(false, RightTra,false);
    }
}
