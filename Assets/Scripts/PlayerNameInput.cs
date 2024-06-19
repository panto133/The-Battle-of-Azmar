using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerNameInput : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;

    public static string DisplayName { get; private set; } = null;

    public void SetPlayerName()
    {
        DisplayName = nameInputField.text;
    }
}
