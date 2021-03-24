using UnityEditor;

/**
 * This script makes sure only one state is ever active in the inspector.
 * If you don't want that, set enabled to false.
 */
[CustomEditor(typeof(ApplicationState), true)]
public class StateIsolator : Isolator<ApplicationState>
{
    protected override bool enabled => false;
}