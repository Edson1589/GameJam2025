using UnityEngine;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    [System.Serializable]
    public class KeyBinding
    {
        public string actionName;
        public KeyCode defaultKey;
        public KeyCode currentKey;
    }

    public List<KeyBinding> keyBindings = new List<KeyBinding>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDefaultKeys();
            LoadKeyBindings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDefaultKeys()
    {
        keyBindings.Add(new KeyBinding { actionName = "Jump", defaultKey = KeyCode.Space, currentKey = KeyCode.Space });
        keyBindings.Add(new KeyBinding { actionName = "Interact", defaultKey = KeyCode.E, currentKey = KeyCode.E });
        keyBindings.Add(new KeyBinding { actionName = "Dash", defaultKey = KeyCode.LeftShift, currentKey = KeyCode.LeftShift });
        keyBindings.Add(new KeyBinding { actionName = "Flashlight", defaultKey = KeyCode.F, currentKey = KeyCode.F });
    }

    public KeyCode GetKeyForAction(string actionName)
    {
        KeyBinding binding = keyBindings.Find(k => k.actionName == actionName);
        return binding != null ? binding.currentKey : KeyCode.None;
    }

    public void SetKeyForAction(string actionName, KeyCode newKey)
    {
        KeyBinding binding = keyBindings.Find(k => k.actionName == actionName);
        if (binding != null)
        {
            binding.currentKey = newKey;
        }
    }

    public void ResetToDefaults()
    {
        foreach (KeyBinding binding in keyBindings)
        {
            binding.currentKey = binding.defaultKey;
        }
        SaveKeyBindings();
    }

    public void SaveKeyBindings()
    {
        foreach (KeyBinding binding in keyBindings)
        {
            PlayerPrefs.SetString("Key_" + binding.actionName, binding.currentKey.ToString());
        }
        PlayerPrefs.Save();
    }

    public void LoadKeyBindings()
    {
        foreach (KeyBinding binding in keyBindings)
        {
            string savedKey = PlayerPrefs.GetString("Key_" + binding.actionName, binding.defaultKey.ToString());
            binding.currentKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), savedKey);
        }
    }

    // Métodos helper para usar en tu código de juego
    public bool GetKeyDown(string actionName)
    {
        return Input.GetKeyDown(GetKeyForAction(actionName));
    }

    public bool GetKey(string actionName)
    {
        return Input.GetKey(GetKeyForAction(actionName));
    }

    public bool GetKeyUp(string actionName)
    {
        return Input.GetKeyUp(GetKeyForAction(actionName));
    }
}