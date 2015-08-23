#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Prime31;


namespace Prime31Editor
{
	[CustomPropertyDrawer( typeof( BitMaskAttribute ) )]
	public class BitMaskPropertyDrawer : PropertyDrawer
	{
		public static int drawBitMaskField( Rect rect, int mask, System.Type type, GUIContent label )
		{
			var itemNames = System.Enum.GetNames( type );
			var itemValues = System.Enum.GetValues( type ) as int[];

			var val = mask;
			var maskVal = 0;
			for( var i = 0; i < itemValues.Length; i++ )
			{
				if( itemValues[i] != 0 )
				{
					if( ( val & itemValues[i] ) == itemValues[i] )
						maskVal |= 1 << i;
				}
				else if( val == 0 )
				{
					maskVal |= 1 << i;
				}
			}

			//EditorGUI.EnumMaskField( rect, label, mask );
			var newMaskVal = EditorGUI.MaskField( rect, label, maskVal, itemNames );
			var changes = maskVal ^ newMaskVal;

			for( var i = 0; i < itemValues.Length; i++ )
			{
				if( ( changes & ( 1 << i ) ) != 0 )            // has this list item changed?
				{
					if( ( newMaskVal & ( 1 << i ) ) != 0 )     // has it been set?
					{
						if( itemValues[i] == 0 )           // special case: if "0" is set, just set the val to 0
						{
							val = 0;
							break;
						}
						else
							val |= itemValues[i];
					}
					else                                  // it has been reset
					{
						val &= ~itemValues[i];
					}
				}
			}

			return val;
		}


		public override void OnGUI( Rect position, SerializedProperty prop, GUIContent label )
		{
			label.text = label.text;
			prop.intValue = drawBitMaskField( position, prop.intValue, fieldInfo.FieldType, label );
		}
	}
}

#endif