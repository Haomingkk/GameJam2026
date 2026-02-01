using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameJam26.UI
{
    public class StartScreenController : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void StartGame()
        {
            if (SceneManager.sceneCountInBuildSettings >= 1)
            {
                // make the main game scene index = 1
                SceneManager.LoadScene(1);
            }

        }

        public void ExitGame()
        {
            // this won't work on webgl
            Application.Quit();
        }
    }

}
