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

        // Debugging the transitions
        if (transitions.Length == 0)
        {
            Debug.LogError("No SceneTransitions found in the transContainer.");
        }
        else
        {
            foreach (var t in transitions)
            {
                Debug.Log($"Found SceneTransition: {t.name}");
            }
        }
    }

    public void LoadScene(string sceneName, string transitionName)
    {
        StartCoroutine(LoadSceneAsync(sceneName, transitionName));
    }

    private IEnumerator LoadSceneAsync(string sceneName, string transitionName)
    {
        // Use FirstOrDefault to avoid throwing an exception if no match is found
        SceneTransitions transition = transitions.FirstOrDefault(t => t.name == transitionName);

        if (transition == null)
        {
            Debug.LogError($"No SceneTransition found with the name '{transitionName}'!");
            yield break; // Exit if no transition is found
        }

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
