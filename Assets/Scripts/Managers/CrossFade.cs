using UnityEngine;
using System.Collections;
using DG.Tweening;

public class CrossFade : MonoBehaviour
{
    public CanvasGroup crossFade;

    // Removed 'override' here
    public IEnumerator AnimateTransitionIn()
    {
        var tweener = crossFade.DOFade(1f, 1f);
        yield return tweener.WaitForCompletion();
    }

    // Removed 'override' here
    public IEnumerator AnimateTransitionOut()
    {
        var tweener = crossFade.DOFade(0f, 1f);
        yield return tweener.WaitForCompletion();
    }
}