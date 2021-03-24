using shared;
using System;
using System.Collections.Generic;
using UnityEngine;

/**
 * This class implements a basic finite state machine, in other words a state switching mechanism.
 * 
 * States are implemented through child game objects that have a subclass of the abstract 
 * ApplicationState script attached to it (, for example LoginState, LobbyState, etc),
 * and can be switched on/off (just like any other gameobject).
 * 
 * On startup, the parent object with this ApplicationFSM script on it, 'collects' all those child objects, 
 * turns them off, and then enables just the starting state (which can be set in the Inspector). 
 * 
 * Each state has EnterState, (Update) and ExitState methods and access to the ApplicationFSM itself 
 * through which it can retrieve a reference to the main TcpMessageChannel and switch to another state.
 * 
 * The advantage of this approach is that each separate piece of functionality goes into its own state,
 * and each state can have its own set of active gameobjects if required (by making them a child of the state gameobject).
 * 
 * In a 'real' codebase this ApplicationFSM would have been filled with generics to make it reusable
 * for other purposes as well, but in this application it is very assignment specific. That said,
 * it would be better to split the FSM part from the 'I am a client class' part, but I will leave
 * that as an exercise to the reader (if your interested let me know and I can show you how that would work).
 * 
 * Anyhow the code is actually shorter than the explanation ;).
 */
public class ApplicationFSM : MonoBehaviour
{
	//we store all ApplicationState instances we find on our children in a dictionary, using their specific state class as key. 
	//For example _stateMap[LoginState] = <instance of LoginState script on specific child gameobject>
	//This way we know which game object to enable when a specific state is requested through ChangeState<LoginState>
	Dictionary<Type, ApplicationState> _stateMap = new Dictionary<Type, ApplicationState>();

	//we also store our current state so we know which state to disable when we switch
	private ApplicationState _currentState = null;

	//easy for testing when we can just jump into a specific state
	//note that in this case this doesn't work too well, since we would be skipping the Login state ;)
	[Tooltip("Drag in one of the child state game object here as a starting state.")]
	[SerializeField] private ApplicationState _startState = null;

	//single reference to a TcpMessageChannel to send/receive messages over throughout the life time of the application.
	public TcpMessageChannel channel { get; private set; }

	protected void Awake()
    {
		//make sure all Console.WriteLines end up in the Unity console as well.
        UnitySystemConsoleRedirector.Redirect();

        channel = new TcpMessageChannel();

		Debug.Log("Initializing FSM:" + this);

		//get all child objects that are an instance of ApplicationState
		ApplicationState[] states = GetComponentsInChildren<ApplicationState>(true);
		foreach (ApplicationState state in states)
		{
			//add them to our dictionary and initialize them with ourselves and a reference to the TcpMessageChannel
			_stateMap.Add(state.GetType(), state);
			state.Initialize(this);
		}
	}

	/**
	 * Change state to which state object was specified in the Inspector.
	 */
	private void Start()
    {
        ChangeState(_startState.GetType());
    }

	/**
	 * Search for a child instance which matches the requested state class,
	 * Enter it and Exit all others.
	 */
	public void ChangeState<T>() where T : ApplicationState
	{
		//delegates to a general change state method
		ChangeState(typeof(T));
	}


	/**
	 * Search for a child instance which matches the requested state type,
	 * Enter it and Exit all others.
	 */
	public void ChangeState(Type pType)
	{
		//if the request state is already active, ignore the request
		if (_currentState != null && _currentState.GetType() == pType) return;

		//exit current state if we have one
		if (_currentState != null)
		{
			_currentState.ExitState();
			_currentState = null;
		}

		//enter next state if we can find ot
		if (pType != null && _stateMap.ContainsKey(pType))
		{
			_currentState = _stateMap[pType];
			_currentState.EnterState();
		}
	}

	public void Update()
	{
		//Quick and dirty reset handler so that you do not have to reset the client all the time
		if (Input.GetKeyDown(KeyCode.F1))
		{
			channel.Close();
			ChangeState(_startState.GetType());
		}
	}

	public void OnApplicationQuit()
	{
		ChangeState(null);
		channel.Close();
	}


}
