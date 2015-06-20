using UnityEngine;
using System.Collections;


namespace Prime31 {

/// <summary>
/// stick this sucker as the type for a property drawer on a bitmask enum field like so:
/// [CustomPropertyDrawer( typeof( BitMaskAttribute ) )]
/// </summary>
public class BitMaskAttribute : PropertyAttribute
{
}}