using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "ScriptableObjects/Quest")]
public class Quest : ScriptableObject
{
    public int questCompletionThreshold;
    public bool isActive = true;
    public Character unlockCharacter;

    public virtual bool CheckCompletion()
    {
        return GetProgress() >= questCompletionThreshold;
    }

    public virtual void SetProgress(int progress)
    {
        if (!isActive) return;
        if (progress > questCompletionThreshold)
            progress = questCompletionThreshold;

        if (progress > GetProgress())
            SaveManager.instance.SetInt(unlockCharacter.prefName + "Quest", progress);
    }

    public virtual void IncrementProgress(int increment = 1)
    {
        if (!isActive) return;
        if (increment > questCompletionThreshold)
        {
            SetProgress(questCompletionThreshold);
        }
        if (GetProgress() + increment > questCompletionThreshold)
        {
            SetProgress(questCompletionThreshold);
            return;
        }
        SetProgress(GetProgress() + increment);
    }

    public int GetProgress()
    {
        return SaveManager.instance.GetInt(unlockCharacter.prefName + "Quest", 0);
    }
}
