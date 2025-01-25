using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
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
