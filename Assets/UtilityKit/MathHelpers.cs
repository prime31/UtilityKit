using UnityEngine;
using System.Collections;
using System.Collections.Generic;



namespace Prime31
{
	public static class MathHelpers
	{
		/// <summary>
		/// Maps a value from some arbitrary range to the 0 to 1 range
		/// </summary>
		/// <param name="value">Value.</param>
		/// <param name="min">Lminimum value.</param>
		/// <param name="max">maximum value</param>
		public static float map01( float value, float min, float max )
		{
			return ( value - min ) * 1f / ( max - min );
		}


		/// <summary>
		/// Maps a value from some arbitrary range to the 1 to 0 range. this is just the reverse of map01
		/// </summary>
		/// <param name="value">Value.</param>
		/// <param name="min">minimum value.</param>
		/// <param name="max">maximum value</param>
		public static float map10( float value, float min, float max )
		{
			return 1f - map01( value, min, max );
		}


		/// <summary>
		/// mapps value (which is in the range leftMin - leftMax) to a value in the range rightMin - rightMax
		/// </summary>
		/// <param name="value">Value.</param>
		/// <param name="leftMin">Left minimum.</param>
		/// <param name="leftMax">Left max.</param>
		/// <param name="rightMin">Right minimum.</param>
		/// <param name="rightMax">Right max.</param>
		public static float map( float value, float leftMin, float leftMax, float rightMin, float rightMax )
		{
			return rightMin + ( value - leftMin ) * ( rightMax - rightMin ) / ( leftMax - leftMin );
		}
	
	
		/// <summary>
		/// rounds value to the nearest number in steps of roundToNearest. Ex: found 127 to nearest 5 results in 125
		/// </summary>
		/// <returns>The to nearest.</returns>
		/// <param name="value">Value.</param>
		/// <param name="roundToNearest">Round to nearest.</param>
		public static float roundToNearest( float value, float roundToNearest )
		{
			return Mathf.Round( value / roundToNearest ) * roundToNearest;
		}
	}
}