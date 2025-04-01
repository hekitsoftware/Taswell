using UnityEngine;
using System.Collections;

public abstract class SceneTransitions : MonoBehaviour
{
    public abstract IEnumerator AnimateTransitionIn();
    public abstract IEnumerator AnimateTransitionOut();
}
