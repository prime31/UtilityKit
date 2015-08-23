using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;


namespace Prime31Editor
{
	public class AutoSnap : EditorWindow
	{
		static bool _didAddUpdateDelegate;
		const string _kUseUtilityWindowPrefsKey = "auto-snap-use-utility-window";
		const string _kShouldSnapPositionKey = "auto-snap-should-snap-position";
		const string _kSnapValueKey = "auto-snap-position-value";
		const string _kSnapOffsetKey = "auto-snap-position-offset";

		Transform _lastTransform;
		Vector3 _lastPosition;
		Vector3 _prevRotation;
		bool _shouldSnapPosition = true;
		bool _shouldSnapRotation = false;
		float _snapPositionValue = 1f;
		float _snapPositionOffset = 0f;
		float _snapRotateValue = 15f;
		bool _useUtilityWindowType = false;


		[MenuItem( "Edit/Auto Snap %_l" )]
		static void Init()
		{
			var useUtilityWindowType = EditorPrefs.GetBool( _kUseUtilityWindowPrefsKey, false );
			var window = EditorWindow.GetWindow<AutoSnap>( useUtilityWindowType );
			window._useUtilityWindowType = useUtilityWindowType;

			window.maxSize = new Vector2( 200, useUtilityWindowType ? 140 : 130 );
			window.minSize = window.maxSize;
		}


		void OnGUI()
		{
			EditorGUI.BeginChangeCheck();
			_shouldSnapPosition = EditorGUILayout.Toggle( "Snap Position", _shouldSnapPosition );
			if( EditorGUI.EndChangeCheck() )
				EditorPrefs.SetBool( _kShouldSnapPositionKey, _shouldSnapPosition );

			EditorGUI.BeginChangeCheck();
			_snapPositionValue = EditorGUILayout.FloatField( "Snap Value", _snapPositionValue );
			if( EditorGUI.EndChangeCheck() )
				EditorPrefs.SetFloat( _kSnapValueKey, _snapPositionValue );

			EditorGUI.BeginChangeCheck();
			_snapPositionOffset = EditorGUILayout.FloatField( "Snap Offset", _snapPositionOffset );
			if( EditorGUI.EndChangeCheck() )
				EditorPrefs.SetFloat( _kSnapOffsetKey, _snapPositionOffset );

			EditorGUILayout.Separator();

			_shouldSnapRotation = EditorGUILayout.Toggle( "Snap Rotation (not implemented)", _shouldSnapRotation );
			_snapRotateValue = EditorGUILayout.FloatField( "Rotation Snap Value", _snapRotateValue );

			EditorGUILayout.Separator();

			EditorGUI.BeginChangeCheck();
			_useUtilityWindowType = EditorGUILayout.Toggle( "Use Floating Window", _useUtilityWindowType );
			if( EditorGUI.EndChangeCheck() )
			{
				EditorPrefs.SetBool( _kUseUtilityWindowPrefsKey, _useUtilityWindowType );
				EditorUtility.DisplayDialog( "Auto Snap Window Type Change", "Window type changes will not take affect until you close and reopen the window", "Got It" );
			}
		}


		void OnEnable()
		{
			if( !_didAddUpdateDelegate )
			{
				_didAddUpdateDelegate = true;
				SceneView.onSceneGUIDelegate += onSceneGUI;
			}

			// load up prefs
			_shouldSnapPosition = EditorPrefs.GetBool( _kShouldSnapPositionKey, _shouldSnapPosition );
			_snapPositionValue = EditorPrefs.GetFloat( _kSnapValueKey, _snapPositionValue );
			_snapPositionOffset = EditorPrefs.GetFloat( _kSnapOffsetKey, _snapPositionOffset );
		}


		void OnDisable()
		{
			if( _didAddUpdateDelegate )
			{
				_didAddUpdateDelegate = false;
				SceneView.onSceneGUIDelegate -= onSceneGUI;
			}
		}
			

		void onSceneGUI( SceneView sceneView )
		{
			// don't snap in play mode or if snap isnt enabled
			if( !_shouldSnapPosition || EditorApplication.isPlayingOrWillChangePlaymode )
				return;

			// Always keep track of the selection
			if( !Selection.transforms.Contains( _lastTransform ) )
			{
				if( Selection.activeTransform )
				{
					_lastTransform = Selection.activeTransform;
					_lastPosition = Selection.activeTransform.position;
				}
			}

			// do the actual snapping
			if( Selection.activeTransform )
			{		
				if( _lastTransform.position != _lastPosition )
				{
					Transform selected = _lastTransform;

					var oldPosition = selected.position;
					selected.position = snapValue( oldPosition );

					// offset all selected transforms
					var offset = selected.position - oldPosition;
					foreach( var trans in Selection.transforms )
					{
						if( trans != selected )
							trans.position += offset;
					}


					_lastPosition = selected.position;
				}
			}
		}


		Vector3 snapValue( Vector3 vec )
		{
			return new Vector3
			(
				_snapPositionValue * Mathf.Round( vec.x / _snapPositionValue ) + _snapPositionOffset,
				_snapPositionValue * Mathf.Round( vec.y / _snapPositionValue ) + _snapPositionOffset,
				_snapPositionValue * Mathf.Round( vec.z / _snapPositionValue ) + _snapPositionOffset
			);
		}


		float snap( float val )
		{
			return _snapPositionValue * Mathf.Round( val / _snapPositionValue ) + _snapPositionOffset;
		}

	}
}