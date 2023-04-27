using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public MenuManager;

public class LeaveMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Quit()
    {
        Application.Quit();
    }

    // Update is called once per frame
    void Return()
    {
        MenuManager.Toggle();
    }
}
