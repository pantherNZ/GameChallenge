using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UtilityComponent : MonoBehaviour
{
    public void SetTextColour( Color colour )
    {
        GetComponent<Text>().color = colour;
    }
}
