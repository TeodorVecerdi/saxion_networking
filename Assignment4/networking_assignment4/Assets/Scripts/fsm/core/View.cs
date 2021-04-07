using UnityEngine;

/**
 * The idea of any View subclass is that it wraps all (well most anyway) details of the underlying components
 * from the rest of the application. You create a view class, specify the components it requires to function,
 * hook those up in the inspector and the rest of the application simply accesses the specific view instance 
 * to do it's work.
 * 
 * See LoginView etc for examples.
 */
public abstract class View : MonoBehaviour
{
    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
}
