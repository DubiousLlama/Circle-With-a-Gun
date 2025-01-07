using System.Collections;
using UnityEngine;

public class MobileOnly : MonoBehaviour
{
    void Awake()
    {
        if (!Platform.IsMobile())
        {
            gameObject.SetActive(false);
        }
    }
}
