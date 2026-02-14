using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Aniamtionscene : MonoBehaviour
{
    public float Numberchange = 6.5f;
    public string homeSceneName = "HomeScene";
    void Start()
    {
        StartCoroutine(SceneChange());
    }
    IEnumerator SceneChange()
    {
        yield return new WaitForSeconds(Numberchange);
        SceneManager.LoadScene(homeSceneName);
    }
}
