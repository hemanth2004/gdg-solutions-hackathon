using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StartingScene : MonoBehaviour
{
    [SerializeField] private string nextSceneName;

    public void OnExperimentButtonClicked(int experimentIndex)
    {
        PlayerPrefs.SetInt("selected_experiment", experimentIndex);
        SceneManager.LoadScene(nextSceneName);
    }


}
