using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Platform;

[CreateAssetMenu(fileName = "EditorConfig", menuName = "ScriptableObjects/EditorConfig")]
public class EditorConfig : ScriptableObject
{
    public PlatformType platform;
}
