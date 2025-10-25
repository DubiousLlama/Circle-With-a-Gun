using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dlcSteamPage : MonoBehaviour
{
    public void OpenDLCPage()
    {   // miss microtransaction steam store page
        Application.OpenURL("steam://store/4137050");
    }
}