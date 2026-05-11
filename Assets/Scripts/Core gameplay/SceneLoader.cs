using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void SceneLoad(int index)
       {
           SceneManager.LoadScene(index);
       }
}
