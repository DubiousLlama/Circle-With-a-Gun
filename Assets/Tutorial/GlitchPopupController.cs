using UnityEngine;
using UnityEngine.UI;

public class GlitchPopupController : MonoBehaviour
{
    public Material glitchMat;
    public float speed = 1f;
    private float glitchValue = 0f;
    private bool showing = false;
    private bool hiding = false;

    public Image trap;

    void Update()
    {
        if (showing)
        {
            glitchValue = Mathf.MoveTowards(glitchValue, 0f, Time.deltaTime * speed);
        }
        if (hiding)
        {
            glitchValue = Mathf.MoveTowards(glitchValue, 1f, Time.deltaTime * speed * 2);
        }
        if (glitchValue == 0 && showing)
        {
            showing = false;
        }
        if (glitchValue == 1 && hiding)
        {
            trap.enabled = false;
            hiding = false;
        }

        glitchMat.SetFloat("_GlitchStrength", glitchValue);
    }

    public void TriggerShow(float time = 0f)
    {
        glitchValue = 1f;
        trap.enabled = true;
        Invoke("Show", time);
    }

    public void TriggerHide(float time = 0f)
    {
        glitchValue = 0f;
        Invoke("Hide", time);
    }

    private void Show() => showing = true;

    private void Hide() => hiding = true;
}
