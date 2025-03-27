using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StartingScene : MonoBehaviour
{
    public TMP_Dropdown dropdown;

    [SerializeField] private string nextSceneName;
    public void OnDropdownValueChanged()
    {
        PlayerPrefs.SetInt("selected_experiment", dropdown.value);
    }

    public void OnStartButtonClicked()
    {
        PlayerPrefs.SetInt("selected_experiment", dropdown.value);
        SceneManager.LoadScene(nextSceneName);
    }


}
