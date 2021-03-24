using UnityEditor;
using UnityEngine;

/**
 * Simple editor script to make sure only the selected child of a parent is active
 * and all other siblings are not.
 */
public abstract class Isolator<T> : Editor where T : Component
{
    //cache last selection to prevent double enable bug in Unity
    private static Object _last = null;

    //(de)activate this script
    abstract protected bool enabled { get; }

    static Isolator()
    {
        //make sure we reset after we change play mode
        EditorApplication.playModeStateChanged += (mode) => _last = null;
    }

    private void OnEnable()
    {
        if (!enabled || Application.isPlaying || _last == target) return;
        Debug.Log("EditorScript Isolator: you selected " + target);
        Transform current = (target as T).transform;
        foreach (Transform sibling in current.parent) sibling.gameObject.SetActive(false);
        current.gameObject.SetActive(true);
        _last = target;
    }

}
