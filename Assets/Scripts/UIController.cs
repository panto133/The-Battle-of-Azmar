using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject keepUI;


    public void OpenKeepUI()
    {
        keepUI.SetActive(true);
    }
    public void CloseUI()
    {
        keepUI.SetActive(false);
    }
}
