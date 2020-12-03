using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class UIProgressBar : MonoBehaviour
{
    [SerializeField] RectTransform mask = null;
    [SerializeField]
    [Range(0.0f, 1.0f)]
    float progress = 0.0f;

    public float Progress
    {
        get => progress;
        set
        {
            progress = Mathf.Clamp( value, 0.0f, 1.0f );
            mask.sizeDelta = new Vector2( progress * ( transform as RectTransform ).sizeDelta.x, mask.sizeDelta.y );
        }
    }

    void OnValidate()
    {
        Progress = progress;
    }
}
