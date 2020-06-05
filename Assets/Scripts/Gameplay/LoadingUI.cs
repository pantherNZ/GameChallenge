using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
    public Image image;
    public int numCircles = 5;
    public int numCirclesForSpacing = 5;
    public float speed = 1.0f;
    public float scalePerCircle = 1.1f;
    List<Image> images = new List<Image>();
    new RectTransform transform;

    // Start is called before the first frame update
    void Start()
    {
        transform = base.transform as RectTransform;
        float scale = 1.0f;
        for( int i = 0; i < numCircles; ++i )
        {
            images.Add( Instantiate( image, transform ) );
            ( images.Back().transform as RectTransform ).localScale = new Vector3( scale, scale, scale );
            scale *= scalePerCircle;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for( int i = 0; i < images.Count; ++i )
        {
            float angle = Mathf.PI * 2.0f / numCirclesForSpacing * i + Time.time * speed;
            var offset = new Vector3( Mathf.Cos( angle ), Mathf.Sin( angle ), transform.position.z );
            ( images[i].transform as RectTransform ).anchoredPosition = transform.position + offset * transform.rect.width / 2.0f;
        }
    }
}
