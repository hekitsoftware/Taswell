using UnityEngine;
using System.Collections;
using DG.Tweening;

public class CrossFade : SceneTransitions
{
    public CanvasGroup crossFade;

    public override IEnumerator AnimateTransitionIn()
    {
        var tweener = crossFade.DOFade(1f, 3f);
        yield return tweener.WaitForCompletion();
    }

    public override IEnumerator AnimateTransitionOut()
    {
        var tweener = crossFade.DOFade(0f, 3f);
        yield return tweener.WaitForCompletion();
    }
}