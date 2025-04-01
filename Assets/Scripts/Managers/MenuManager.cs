using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public AudioSource m_AudioSource;
    
    void Start()
    {
        m_AudioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            NewGame();
        }
    }

    public void NewGame()
    {
        m_AudioSource.Play();
        LevelManager.Instance.LoadScene("TestScene", "CrossFade");
    }
}
