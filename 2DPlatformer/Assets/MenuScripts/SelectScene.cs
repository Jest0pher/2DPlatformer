using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
public class SelectScene : MonoBehaviour
{
    public string nextScene;
    AsyncOperation asyncOp;
    bool canSwitch;
    [SerializeField] bool fade;
    [SerializeField] Volume volume;
    [SerializeField] float fadeTime = 1;
    private void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }
    public void NextScene() {
        //SceneManager.LoadScene(nextScene);
        canSwitch = true;
    }

    IEnumerator LoadSceneAsync() { 
        asyncOp = SceneManager.LoadSceneAsync(nextScene, LoadSceneMode.Single);
        asyncOp.allowSceneActivation = false;
        while (!asyncOp.isDone) {
            if (asyncOp.progress >= .9f && canSwitch) {
                if (fade)
                {
                    StartCoroutine(FadeToBlack());
                }
                else {
                    asyncOp.allowSceneActivation = true;
                }
                break;
            }
            yield return null;
        }
        
    }

    IEnumerator FadeToBlack() {
        float timer = 0;
        while (volume.weight < 1)
        {
            volume.weight = timer / fadeTime;
            yield return null;
            timer += Time.deltaTime;
        }
        volume.weight = 1;
        yield return new WaitForSeconds(.1f);
        asyncOp.allowSceneActivation = true;
    }

}
