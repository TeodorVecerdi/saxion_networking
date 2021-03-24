using UnityEngine;

/**
 * To keep this application 'simple' we are not using a UIManager (which could also be an FSM), 
 * instead an application state can be connected directly to piece of the UI, which we 
 * enable/disable as this state becomes active/inactive.
 * 
 * I added this indirection so that every state subclass has direct access to a Typed view and
 * we cannot connect the wrong view to it in the Inspector.
 * 
 * All this class does, it enabling that view connection and managing it.
 *
 * Why not put this directly in ApplicationState<T>? 
 * Mainly because generics are a b....tch to store in dictionaries.
 * 
 * In short advantages of this approach:
 *  - After doing something like GameState<GameView> all you can connect to the GameState in the Inspector IS a GameView
 *  - GameState has access to a typed GameView without any casting.
 */
public abstract class ApplicationStateWithView<T> : ApplicationState where T : View
{
	//make sure you connect a view of the correct type to a given state in the inspector
    [SerializeField] private T _view = null;
    protected T view { get { return _view; } }

	public override void Initialize(ApplicationFSM pApplicationFSM)
	{
		base.Initialize(pApplicationFSM);
		view?.Hide();

		Debug.Log("Initialized state " + this.name + " (linked to view:"+view?.name+")");
	}

	public override void EnterState()
	{
		base.EnterState();
		view?.Show();
	}

	public override void ExitState()
	{
		base.ExitState();
		view?.Hide();
	}
}
