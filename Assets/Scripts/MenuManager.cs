using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public Menu;

public class MenuManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Toggle(Menu)
    {
        if (Menu.isActive())
        {
            Menu.SetActive(false);
        }
        else
        {
            MenuManager.SetActive(true);
        }
    }
}
