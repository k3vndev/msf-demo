using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager I { get; private set; }

    void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;
    }
}