using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScroller : MonoBehaviour
{
    [SerializeField] RectTransform verticalLayout = null;
    [SerializeField] Button btnUp = null;
    [SerializeField] Button btnDown = null;
    [SerializeField] List<string> data = new List<string>();
    [SerializeField] float maxHeight = 0.0f;
    [SerializeField] float minHeight = 0.0f;

    void Start()
    {
        float centreHeight = ( btnUp.transform.localPosition.y + btnDown.transform.localPosition.y ) / 2.0f;

        btnUp.onClick.AddListener( () =>
        {
            if( verticalLayout.localPosition.y >= minHeight )
                verticalLayout.localPosition = verticalLayout.localPosition.SetY( verticalLayout.localPosition.y + ( centreHeight - btnUp.transform.localPosition.y ) );
        } );

        btnDown.onClick.AddListener( () =>
        {
            if( verticalLayout.localPosition.y <= maxHeight )
                verticalLayout.localPosition = verticalLayout.localPosition.SetY( verticalLayout.localPosition.y + ( centreHeight - btnDown.transform.localPosition.y ) );
        } );
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
