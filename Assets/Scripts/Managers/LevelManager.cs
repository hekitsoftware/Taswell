using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public Slider progressBar;
    public GameObject transContainer;

    private SceneTransitions[] transitions;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        transitions = transContainer.GetComponentsInChildren<SceneTransitions>();
    }

    public void LoadScene(string sceneName, string transitionName)
    {
        StartCoroutine(LoadSceneAsync(sceneName, transitionName));
    }

    private IEnumerator LoadSceneAsync(string sceneName, string transitionName)
    {
        SceneTransitions transition = transitions.First(t => t.name == transitionName);

        // Use UnityEngine.AsyncOperation explicitly to avoid ambiguity
        UnityEngine.AsyncOperation scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;

        yield return transition.AnimateTransitionIn();

        progressBar.gameObject.SetActive(true);

        do
        {
            progressBar.value = scene.progress;
            yield return null;
        } while (scene.progress < 0.9f);

        scene.allowSceneActivation = true;
        progressBar.gameObject.SetActive(false);

        yield return transition.AnimateTransitionOut();
    }

}
