using UnityEngine;
using System.Collections;
using UnityEditor;


namespace Prime31 {

public class AutoSnap : EditorWindow
{
	private static bool _addedUpdateDelegate;

	private Vector3 prevPosition;
	private Vector3 prevRotation;
	private bool doSnap = true;
	private bool doRotateSnap = true;
	private float snapValue = 1f;
	private float snapOffset = 0f;
	private float snapRotateValue = 15f;


	[MenuItem( "Edit/Auto Snap %_l" )]
	static void Init()
	{
		var window = (AutoSnap)EditorWindow.GetWindow( typeof( AutoSnap ) );
		window.maxSize = new Vector2( 200, 100 );
	}


	public void OnGUI()
	{
		doSnap = EditorGUILayout.Toggle( "Auto Snap", doSnap );
		doRotateSnap = EditorGUILayout.Toggle ("Auto Snap Rotation", doRotateSnap );
		snapValue = EditorGUILayout.FloatField( "Snap Value", snapValue );
		snapOffset = EditorGUILayout.FloatField( "Snap Offset", snapOffset );
		snapRotateValue = EditorGUILayout.FloatField( "Rotation Snap Value", snapRotateValue );

		if( !doSnap && !doRotateSnap )
			OnDisable();
	}


	public void OnEnable()
	{
		if( !_addedUpdateDelegate )
		{
			_addedUpdateDelegate = true;
			EditorApplication.update += Update;
		}
	}


	public void OnDisable()
	{
		if( _addedUpdateDelegate )
		{
			_addedUpdateDelegate = false;
			EditorApplication.update -= Update;
		}
	}


	public void Update()
	{
		if ( doSnap
		    && !EditorApplication.isPlaying
		    && Selection.transforms.Length > 0
		    && Selection.transforms[0].position != prevPosition )
		{
			snapPositionForSelectedTransforms();
			prevPosition = Selection.transforms[0].position;
		}

		if ( doRotateSnap
		    && !EditorApplication.isPlaying
		    && Selection.transforms.Length > 0
		    && Selection.transforms[0].eulerAngles != prevRotation )
		{
			snapRotationForSelectedTransforms();
			prevRotation = Selection.transforms[0].eulerAngles;
		}
	}


	private void snapPositionForSelectedTransforms()
	{
		foreach( var transform in Selection.transforms )
		{
			var t = transform.position;
			t.x = snapAndRound( t.x, snapValue ) + snapOffset;
			t.y = snapAndRound( t.y, snapValue ) + snapOffset;
			t.z = snapAndRound( t.z, snapValue ) + snapOffset;
			transform.position = t;
		}
	}


	private void snapRotationForSelectedTransforms()
	{
		foreach( var transform in Selection.transforms )
		{
			var r = transform.eulerAngles;
			r.x = snapAndRound( r.x, snapRotateValue );
			r.y = snapAndRound( r.y, snapRotateValue );
			r.z = snapAndRound( r.z, snapRotateValue );
			transform.eulerAngles = r;
		}
	}

	private float snapAndRound( float input, float snap )
	{
		return snap * Mathf.RoundToInt( ( input / snap ) );
	}

}}