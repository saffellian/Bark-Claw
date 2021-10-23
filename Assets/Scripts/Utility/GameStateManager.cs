using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    public enum GameStateProcessResult {
        SUCCESS,
        FAILURE
    }

    [System.Serializable]
    public struct ObjectKey {
        public int gameObjectId;
        public string className;
    }

    [System.Serializable]
    public struct SceneObject {
        public string name;
        public int buildIndex;
        public Dictionary<string, string> saveableObjects;
    }

    private Dictionary<string, string> extraData = new Dictionary<string, string>();

    [Header("DEBUG")]
    [Tooltip("Do not load object states from file on game start")]
    public bool skipStartupLoad = false;

    private bool loadStateTrigger = false;
    [SerializeField] private string nextLoadId = null;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// Called every time a new scene is loaded
    /// </summary>
    /// <param name="scene">Scene that has been loaded</param>
    /// <param name="mode">The mode in which the scene was loaded</param>
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance != this)
            return;
            
        if (!skipStartupLoad)
            loadStateTrigger = true;

        if (!string.IsNullOrEmpty(nextLoadId) && nextLoadId.Contains(GetSceneId()))
        {
            StartCoroutine(WaitBeforeLoad());
        }
    }

    private IEnumerator WaitBeforeLoad()
    {
        yield return new WaitForEndOfFrame();
        LoadGameState(nextLoadId);
        nextLoadId = null;
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        // Loads game state during first frame
        if (loadStateTrigger)
        {
            LoadGameState();
            loadStateTrigger = false;
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            SaveGameState();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            LoadGameState();
        }
    }

    /// <summary>
    /// Get all saveable objects and write their data to file in JSON format.
    /// </summary>
    public GameStateProcessResult SaveGameState(string saveId = null, bool setNextLoad = false)
    {
        try
        {
            string storageId = GetSceneId();

            if (setNextLoad)
            {
                nextLoadId = $"{storageId}_{saveId}.json";
            }

            string path = string.IsNullOrEmpty(saveId) ? $"{storageId}.json" : nextLoadId;
            string savePath = Path.Combine(Application.persistentDataPath, path);
            string jsonString = string.Empty;
            Debug.Log(savePath);

            Saveable[] saveables = GameObject.FindObjectsOfType<Saveable>().ToArray();
            
            SceneObject sceneObject = new SceneObject();
            sceneObject.saveableObjects = new Dictionary<string, string>();
            sceneObject.name = SceneManager.GetActiveScene().name;
            sceneObject.buildIndex = SceneManager.GetActiveScene().buildIndex;
            
            SaveableData data;
            foreach (var saveable in saveables)
            {
                data = saveable.GetObjectState();
                sceneObject.saveableObjects.Add(data.dictionaryKey, data.jsonContent);
            }

            foreach (var value in extraData)
            {
                sceneObject.saveableObjects.Add(value.Key, value.Value);
            }

            var converter = new SceneObjectJsonConverter(typeof(SceneObject));
            jsonString = JsonConvert.SerializeObject(sceneObject, converter);
            File.WriteAllText(savePath, jsonString, System.Text.Encoding.UTF8);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return GameStateProcessResult.FAILURE;
        }
        
        return GameStateProcessResult.SUCCESS;
    }

    public GameStateProcessResult LoadGameState(string saveId = null)
    {
        try
        {
            string storageId = GetSceneId();
            string path = string.IsNullOrEmpty(saveId) ? $"{storageId}.json" : saveId;
            string savePath = Path.Combine(Application.persistentDataPath, path);
            Debug.Log(savePath);
            
            string jsonString = File.ReadAllText(savePath);
            var converter = new SceneObjectJsonConverter(typeof(SceneObject));
            SceneObject sceneObject = JsonConvert.DeserializeObject<SceneObject>(jsonString, converter);
            
            Saveable[] saveables = GameObject.FindObjectsOfType<Saveable>().ToArray();

            foreach(var item in saveables)
            {
                Debug.Log(item.GetDictionaryKey());
                if (sceneObject.saveableObjects.ContainsKey(item.GetDictionaryKey()))
                {
                    item.ApplyObjectState(sceneObject.saveableObjects[item.GetDictionaryKey()]);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return GameStateProcessResult.FAILURE;
        }

        return GameStateProcessResult.SUCCESS;
    }

    public void StoreSaveData(string dictionaryKey, string jsonContent)
    {
        extraData.Add(dictionaryKey, jsonContent);
    }

    private string GetSceneId()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        return $"{activeScene.name}_{activeScene.buildIndex}";
    }
}
