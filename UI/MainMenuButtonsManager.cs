using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.FPS.UI
{
    public class MainMenuButtonsManager : MonoBehaviour
    {
        public int MainSceneIndex = 1;
        public void StartGame()
        {
            SceneManager.LoadScene(MainSceneIndex);
        }

        public void QuitGame()
        {
            Application.Quit(0);
        }
    }
}