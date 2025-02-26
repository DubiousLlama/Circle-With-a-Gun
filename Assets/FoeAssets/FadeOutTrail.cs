using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOutTrail : MonoBehaviour
{
   void Start()
    {
        StartCoroutine(FadeOut(gameObject));
    }

    private IEnumerator FadeOut(GameObject obj)
    {
        SpriteRenderer sprite = obj.GetComponent<SpriteRenderer>();
        Color color = sprite.color;

        while (color.a > 0)
        {
            color.a -= 0.1f;
            sprite.color = color;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
