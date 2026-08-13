using System;
using System.Collections;
using System.Collections.Generic;
using DesignPatterns.CreationalPatterns;
using EMILtools.Design_Patterns.Creational_Patterns.CreationalPatterns;
using EMILtools.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;


public class Loader : PersistantReplacerSingleton<Loader>
{
    [SerializeField] float minTimeLoading = 1f;
    [SerializeField] public FadeSettings loadingFade;

    public int loadScreenIndx;
    [SerializeField] [ListDrawerSettings(ShowIndexLabels = true)] 
    public List<IntList> scenesToLoad = new();

    [Serializable]
    public class IntList
    {
        [HideLabel] public string name;
        [Indent(2)] public List<int> values = new();
    }

    public void LoadSceneFadeScreenToOpaque(Object fadeTarget, int indx)
    {
        this.FadeToOpaque(loadingFade, fadeTarget, Load);
        void Load() => StartCoroutine(this.Load(indx, true));
    }
    
    public void LoadSceneImmediate(int indx) => StartCoroutine(Load(indx, false));
    public void LoadSceneAdditiveDisableCurrent(int sceneIndex) => StartCoroutine(C_LoadSceneAdditiveDisableCurrent(sceneIndex));

    public GameObject[] LoadingScreenObjects;
    public GameObject[] disables;


    IEnumerator Load(int indx, bool unlockMouse = true)
    {
        LoadingScreenObjects.SetAllActive(true);
        disables.SetAllActive(false);
        yield return null;
        StartCoroutine(LoadSceneAsync(indx));
    }
    IEnumerator LoadSceneAsync(int indx, bool unlockMouse = true)
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(loadScreenIndx, LoadSceneMode.Additive);
        
        while(!loadOp.isDone)
            yield return null;
        
        
        yield return new WaitForSeconds(minTimeLoading);
        
        List<AsyncOperation> otherSceneOps = new();
        AsyncOperation mainOp = SceneManager.LoadSceneAsync(scenesToLoad[indx].values[0], LoadSceneMode.Single);
        mainOp.allowSceneActivation = false;
        for (int i = 1; i < scenesToLoad[indx].values.Count; i++)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(scenesToLoad[indx].values[i], LoadSceneMode.Additive);
            op.allowSceneActivation = false;
            otherSceneOps.Add(op);
        }
        

        while(mainOp.progress < 0.8f && otherSceneOps.TrueForAll(op => op.progress < 0.8f))
            yield return null;
        
        mainOp.allowSceneActivation = true;
        otherSceneOps.ForEach(op => op.allowSceneActivation = true);

        if (unlockMouse)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    public IEnumerator C_LoadSceneAdditiveDisableCurrent(int indx)
    {
        var currentScene = SceneManager.GetActiveScene();
        List<AsyncOperation> loadOperations = new();

        // Load all scenes in this group additively
        foreach (int sceneIndex in scenesToLoad[indx].values)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
            loadOperations.Add(op);
        }

        // Wait for all scenes to finish loading
        foreach (AsyncOperation op in loadOperations)
            while (!op.isDone) yield return null;



        // Disable current scene
        foreach (GameObject obj in currentScene.GetRootGameObjects()) obj.SetActive(false);
        
        // Set first loaded scene as active
        Scene newActiveScene = SceneManager.GetSceneByBuildIndex(scenesToLoad[indx].values[0]);
        SceneManager.SetActiveScene(newActiveScene);
    }
}
