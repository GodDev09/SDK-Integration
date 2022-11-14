using System;
using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneAutoLoad : MonoBehaviour
{
    [SerializeField] private SceneName sceneNameToLoad;
    public float minLoadingTime = 6;
    public float delayLoadingTime;
    public bool skipMinLoadingTime = false;
    public bool allowReload = false;
    public bool setActiveAfterLoad = false;
    public bool autoUnload = true;

    private bool isLoaded = false;

    public UnityEvent<float> onLoadingProgress = new UnityEvent<float>();
    public UnityEvent onLoadSuccess = new UnityEvent();
    
    public void Start()
    {
        Observable.Timer(TimeSpan.FromSeconds(0.15f + delayLoadingTime)).Take(1).Subscribe(_ =>
        {
            //verify if the scene is already open to avoid opening a scene twice
            if (SceneManager.sceneCount > 0)
            {
                for (int i = 0; i < SceneManager.sceneCount; ++i)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    if (scene.name == sceneNameToLoad.ToString())
                    {
                        isLoaded = true;
                    }
                }
            }

            LoadScene();
        });
    }

    private void LoadScene()
    {
        var sceneName = sceneNameToLoad.ToString();
        
        if (allowReload && isLoaded && autoUnload)
            SceneManager.UnloadSceneAsync(sceneName);

        if (!isLoaded || allowReload)
        {
            //Loading the scene, using the gameobject name as it's the same as the name of the scene to load
            AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            //We set it to true to avoid loading the scene twice
            isLoaded = true;

            if (setActiveAfterLoad)
            {
                StartCoroutine(ActiveAfterLoad(async));
            }
        }

        // Scene sceneToLoad = SceneManager.GetSceneByName(gameObject.name);
        // if (sceneToLoad.isLoaded && setActiveAfterLoad)
        //     SceneManager.SetActiveScene(sceneToLoad);
    }
    
    IEnumerator ActiveAfterLoad(AsyncOperation async)
    {
        float loadTime = 0;
        async.allowSceneActivation = false;
        while (!async.isDone)
        {
            // Check if the load has finished
            if (async.progress >= 0.9f && (loadTime >= minLoadingTime || skipMinLoadingTime))
            {
                // loadTime = minLoadingTime;
                // yield return new WaitForSeconds(0.5f);
                async.allowSceneActivation = true;
                onLoadSuccess.Invoke();
            }
            loadTime += Time.deltaTime;
            
            onLoadingProgress.Invoke(loadTime / minLoadingTime);

            yield return null;
        }

        //SceneManager.SetActiveScene(SceneManager.GetSceneByName(gameObject.name));
    }
}

public enum SceneName
{
    Preload,
    Main,
    Menu
}
