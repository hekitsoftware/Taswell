using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class MenuScroller : MonoBehaviour
{
    [SerializeField] private RawImage _img;
    [SerializeField] private float _x, _y;

    private void Update()
    {
        // Scroll UV position
        Vector2 newPos = _img.uvRect.position + new Vector2(_x, _y) * Time.deltaTime;

        // Use Mathf.Repeat to ensure UV coordinates stay within [0, 1] range
        newPos.x = Mathf.Repeat(newPos.x, 1f); // Wrap x between 0 and 1
        newPos.y = Mathf.Repeat(newPos.y, 1f); // Wrap y between 0 and 1

        // Apply the updated UV position to the RawImage
        _img.uvRect = new Rect(newPos, _img.uvRect.size);
    }
}
