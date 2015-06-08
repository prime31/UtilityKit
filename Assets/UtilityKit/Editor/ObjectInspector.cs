using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using Prime31;


// when implementing this in your MonoBehaviours, wrap your using UnityEditor and
// OnInspectorGUI/OnSceneGUI methods in #if UNITY_EDITOR/#endif



namespace Prime31Editor
{
	/// <summary>
	/// for fields to work with the Vector3 inspector they must either be public or marked with SerializeField and have the Vector3Inspectable
	/// attribute.
	/// </summary>
	[CustomEditor( typeof( UnityEngine.Object ), true )]
	[CanEditMultipleObjects]
	public class ObjectInspector : Editor
	{
		MethodInfo _onInspectorGuiMethod;
		MethodInfo _onSceneGuiMethod;
		List<MethodInfo> _buttonMethods = new List<MethodInfo>();

		// Vector3 editor
		bool _hasVector3Fields = false;
		IEnumerable<FieldInfo> _fields;


		public void OnEnable()
		{
			var type = target.GetType();
			if( !typeof( IObjectInspectable ).IsAssignableFrom( type ) )
				return;

			_onInspectorGuiMethod = target.GetType().GetMethod( "OnInspectorGUI", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );
			_onSceneGuiMethod = target.GetType().GetMethod( "OnSceneGUI", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic );

			var meths = type.GetMethods( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
				.Where( m => m.IsDefined( typeof( MakeButtonAttribute ), false ) );
			foreach( var meth in meths )
			{
				_buttonMethods.Add( meth );
			}

			// the vector3 editor needs to find any fields with the Vector3Inspectable attribute and validate them
			_fields = type.GetFields( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
				.Where( f => f.IsDefined( typeof( Vector3Inspectable ), false ) )
				.Where( f => f.IsPublic || f.IsDefined( typeof( SerializeField ), false ) );
			_hasVector3Fields = _fields.Count() > 0;
		}


		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if( _onInspectorGuiMethod != null )
			{
				foreach( var eachTarget in targets )
					_onInspectorGuiMethod.Invoke( eachTarget, new object[0] );
			}


			foreach( var meth in _buttonMethods )
			{
				if( GUILayout.Button( CultureInfo.InvariantCulture.TextInfo.ToTitleCase( Regex.Replace( meth.Name, "(\\B[A-Z])", " $1" ) ) ) )
					foreach( var eachTarget in targets )
						meth.Invoke( eachTarget, new object[0] );
			}
		}


		protected virtual void OnSceneGUI()
		{
			if( _onSceneGuiMethod != null )
				_onSceneGuiMethod.Invoke( target, new object[0] );

			if( _hasVector3Fields )
				vector3OnSceneGUI();
		}


		#region Vector3 editor

		void vector3OnSceneGUI()
		{
			foreach( var field in _fields )
			{
				var value = field.GetValue( target );
				if( value is Vector3 )
				{
					Handles.Label( (Vector3)value, field.Name );
					var newValue = Handles.PositionHandle( (Vector3)value, Quaternion.identity );
					if( GUI.changed )
					{
						GUI.changed = false;
						field.SetValue( target, newValue );
					}
				}
				else if( value is List<Vector3> )
				{
					var list = value as List<Vector3>;
					var label = field.Name + ": ";

					for( var i = 0; i < list.Count; i++ )
					{
						Handles.Label( list[i], label + i );
						list[i] = Handles.PositionHandle( list[i], Quaternion.identity );
					}
					Handles.DrawPolyLine( list.ToArray() );
				}
				else if( value is Vector3[] )
				{
					var list = value as Vector3[];
					var label = field.Name + ": ";

					for( var i = 0; i < list.Length; i++ )
					{
						Handles.Label( list[i], label + i );
						list[i] = Handles.PositionHandle( list[i], Quaternion.identity );
					}
					Handles.DrawPolyLine( list );
				}
			}
		}

		#endregion

	}
}