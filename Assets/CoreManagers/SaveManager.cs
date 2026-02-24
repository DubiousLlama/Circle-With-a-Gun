using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

/// <summary>
/// A file-based replacement for PlayerPrefs that saves data to a single JSON file.
/// This file is stored in Application.persistentDataPath, making it compatible
/// with Steam Auto-Cloud.
///
/// It mimics the PlayerPrefs API (SetInt, GetInt, etc.) by using dictionaries
/// which are properly serialized to/from JSON.
/// </summary>
public class SaveManager : MonoBehaviour
{

    
    // --- Singleton Pattern ---
    private static SaveManager _instance;
    public static SaveManager instance
    {
        get
        {
            if (_instance == null)
            {
                // Check if an instance exists in the scene
                _instance = FindAnyObjectByType<SaveManager>();

                // If not, create a new GameObject and add the component
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject("SaveManager");
                    _instance = singletonObject.AddComponent<SaveManager>();
                }
            }
            _instance.EnsureInitialized();
            return _instance;
        }
    }

    // --- Public Properties ---
    public string saveFileName = "save.json";

    // --- Private Fields ---
    private SaveData _saveData;
    private string _savePath;

    // --- Public Events ---
    public static event Action OnSaveDataReady;

    // --- Unity Methods ---

    private void Awake()
    {
        // Enforce singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        // Ensure this object persists across scene loads
        DontDestroyOnLoad(gameObject);

        EnsureInitialized();
    }

    /// <summary>
    /// Ensures _savePath and _saveData are initialized. Safe to call multiple times.
    /// Handles the case where SaveManager is used before Awake (e.g. on a disabled GameObject).
    /// </summary>
    private void EnsureInitialized()
    {
        if (_saveData != null)
            return;

        if (string.IsNullOrEmpty(_savePath))
            _savePath = Path.Combine(Application.persistentDataPath, saveFileName);

        LoadGame();
        OnSaveDataReady?.Invoke();
    }

    private void OnApplicationQuit()
    {
        // Auto-save when the application quits
        SaveGame();
    }

    // --- Public API (Mimics PlayerPrefs) ---

    public void SetInt(string key, int value)
    {
        EnsureInitialized();
        _saveData.intData[key] = value;
    }

    public int GetInt(string key, int defaultValue = 0)
    {
        EnsureInitialized();
        if (_saveData.intData.TryGetValue(key, out int value))
        {
            return value;
        }
        return defaultValue;
    }

    public void SetString(string key, string value)
    {
        EnsureInitialized();
        _saveData.stringData[key] = value;
    }

    public string GetString(string key, string defaultValue = "")
    {
        EnsureInitialized();
        if (_saveData.stringData.TryGetValue(key, out string value))
        {
            return value;
        }
        return defaultValue;
    }

    public void SetFloat(string key, float value)
    {
        EnsureInitialized();
        _saveData.floatData[key] = value;
    }

    public float GetFloat(string key, float defaultValue = 0.0f)
    {
        EnsureInitialized();
        Debug.Log($"Getting float for key: {key}");
        if (_saveData.floatData.TryGetValue(key, out float value))
        {
            return value;
        }
        return defaultValue;
    }

    public bool HasKey(string key)
    {
        EnsureInitialized();
        return _saveData.intData.ContainsKey(key) ||
               _saveData.stringData.ContainsKey(key) ||
               _saveData.floatData.ContainsKey(key);
    }

    public void DeleteKey(string key)
    {
        EnsureInitialized();
        _saveData.intData.Remove(key);
        _saveData.stringData.Remove(key);
        _saveData.floatData.Remove(key);
    }

    public void DeleteAll()
    {
        EnsureInitialized();
        _saveData = new SaveData();
        Debug.Log("All save data deleted from memory. Call Save() to commit changes to disk.");
    }

    /// <summary>
    /// Manually triggers a save to the JSON file.
    /// </summary>
    public void Save()
    {
        SaveGame();
    }

    // --- Private File I/O Methods ---

    private void LoadGame()
    {
        if (File.Exists(_savePath))
        {
            try
            {
                // Read the Base64 encoded string from the file
                string base64String = File.ReadAllText(_savePath);

                // Convert from Base64 back to bytes
                byte[] jsonBytes = System.Convert.FromBase64String(base64String);

                // Convert bytes back to the JSON string
                string json = System.Text.Encoding.UTF8.GetString(jsonBytes);

                _saveData = JsonUtility.FromJson<SaveData>(json);

                if (_saveData == null)
                {
                    Debug.LogWarning($"Failed to parse save file at {_savePath}. Creating new save data.");
                    _saveData = new SaveData();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading save file: {e.Message}. Creating new save data.");
                _saveData = new SaveData();
            }
        }
        else
        {
            Debug.Log("No save file found. Creating new save data.");
            _saveData = new SaveData();
        }
    }

    private void SaveGame()
    {
        if (_saveData == null)
            return;
        try
        {
            string json = JsonUtility.ToJson(_saveData, true); // 'true' for pretty print

            // --- Obfuscation Step ---
            // Convert the JSON string to bytes
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);

            // Convert the bytes to a Base64 string to obfuscate it
            string base64String = System.Convert.ToBase64String(jsonBytes);

            // Write the obfuscated string to the file
            File.WriteAllText(_savePath, base64String);

            Debug.Log($"Game saved to {_savePath}");

            // Invoke the event when save data is ready
            OnSaveDataReady?.Invoke();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving game file: {e.Message}");
        }
    }
}


/// <summary>
/// The main data container that holds all save data.
/// It uses ISerializationCallbackReceiver to convert non-serializable
/// dictionaries into serializable lists before Unity's JSON utility
/// runs, and converts them back after.
/// </summary>
[System.Serializable]
public class SaveData : ISerializationCallbackReceiver
{
    // Dictionaries are not serialized by Unity.
    // These are the runtime data structures.
    public Dictionary<string, int> intData = new Dictionary<string, int>();
    public Dictionary<string, string> stringData = new Dictionary<string, string>();
    public Dictionary<string, float> floatData = new Dictionary<string, float>();

    // --- SERIALIZATION FIELDS ---
    // Unity *can* serialize lists of custom serializable types.
    // We will convert our dictionaries to these lists before serialization.
    [SerializeField] private List<SerializableKeyValuePair<string, int>> _intDataList = new List<SerializableKeyValuePair<string, int>>();
    [SerializeField] private List<SerializableKeyValuePair<string, string>> _stringDataList = new List<SerializableKeyValuePair<string, string>>();
    [SerializeField] private List<SerializableKeyValuePair<string, float>> _floatDataList = new List<SerializableKeyValuePair<string, float>>();

    /// <summary>
    /// Called by Unity *before* serialization (saving to JSON).
    /// We convert our dictionaries into the serializable lists.
    /// </summary>
    public void OnBeforeSerialize()
    {
        _intDataList.Clear();
        foreach (var pair in intData)
        {
            _intDataList.Add(new SerializableKeyValuePair<string, int>(pair.Key, pair.Value));
        }

        _stringDataList.Clear();
        foreach (var pair in stringData)
        {
            _stringDataList.Add(new SerializableKeyValuePair<string, string>(pair.Key, pair.Value));
        }

        _floatDataList.Clear();
        foreach (var pair in floatData)
        {
            _floatDataList.Add(new SerializableKeyValuePair<string, float>(pair.Key, pair.Value));
        }
    }

    /// <summary>
    /// Called by Unity *after* deserialization (loading from JSON).
    /// We convert our serializable lists back into the runtime dictionaries.
    /// </summary>
    public void OnAfterDeserialize()
    {
        intData.Clear();
        foreach (var pair in _intDataList)
        {
            intData[pair.key] = pair.value;
        }

        stringData.Clear();
        foreach (var pair in _stringDataList)
        {
            stringData[pair.key] = pair.value;
        }

        floatData.Clear();
        foreach (var pair in _floatDataList)
        {
            floatData[pair.key] = pair.value;
        }
    }
}

/// <summary>
/// A helper class to make a generic key-value pair serializable by Unity.
/// </summary>
[System.Serializable]
public struct SerializableKeyValuePair<TKey, TValue>
{
    public TKey key;
    public TValue value;

    public SerializableKeyValuePair(TKey key, TValue value)
    {
        this.key = key;
        this.value = value;
    }
}