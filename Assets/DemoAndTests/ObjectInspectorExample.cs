using UnityEngine;
using System.Collections.Generic;
using Prime31;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class ObjectInspectorExample : MonoBehaviour, IObjectInspectable
{
	[System.Flags]
	public enum SomeEnum
	{
		Poop = 1,
		Pizza,
		Meatballs,
		Dog,
		Rice
	}


	// this attribute will add handles for your Vector3s so that you can drag them around in the scene view
	[Vector3Inspectable]
	[SerializeField]
	List<Vector3> someListOfVectors;

	[SerializeField]
	[BitMaskAttribute]
	SomeEnum anEnumWithInspector;
  
  
	// this method will appear as a button in the inspector
	[MakeButton]
	public void someVoidMethodClickToDumpLog()
	{
		Debug.Log( "I'm a log from a button in the Inspector!" );
	}
  
  
#if UNITY_EDITOR
	void OnInspectorGUI()
	{
		// do inspector stuff
		GUILayout.Label( "This is a label from the ObjectInspectorExample class" );
		GUILayout.Button( "I'm a Button from the same class" );
	}

	
	void OnSceneGUI()
	{
		// do scene stuff
		Handles.BeginGUI();
		GUILayout.BeginArea( new Rect( 5f, 5f, 170f, 50f ) );

		if( GUILayout.Button( "Create GameObject" ) )
		{
			var go = new GameObject( "Made in the Scene View" );
			go.transform.position = new Vector3( 0f, 10f, 0f );
		}

		GUILayout.EndArea();
		Handles.EndGUI();
	}
#endif

}