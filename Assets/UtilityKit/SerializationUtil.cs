using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Reflection;



namespace Prime31
{
	public static class SerializationUtil
	{
		static SurrogateSelector _surrogateSelector;

		static SurrogateSelector surrogateSelector
		{
			get
			{
				if( _surrogateSelector == null )
				{
					var vectorSurrogate = new VectorSerializationSurrogate();
					_surrogateSelector = new SurrogateSelector();
					_surrogateSelector.AddSurrogate( typeof( Vector2 ), new StreamingContext( StreamingContextStates.All ), vectorSurrogate );
					_surrogateSelector.AddSurrogate( typeof( Vector3 ), new StreamingContext( StreamingContextStates.All ), vectorSurrogate );
				}

				return _surrogateSelector;
			}
		}


		static SerializationUtil()
		{
			// why? http://answers.unity3d.com/questions/30930/why-did-my-binaryserialzer-stop-working.html?sort=oldest
			Environment.SetEnvironmentVariable( "MONO_REFLECTION_SERIALIZER", "yes" );
		}


		/// <summary>
		/// deserializes the object using a BinaryFormatter
		/// </summary>
		/// <returns>The object from file.</returns>
		/// <param name="filename">Filename.</param>
		/// <param name="folder">Folder. defaults to Application.persistantDataPath</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T deserializeObjectFromFile<T>( string filename, string folder = null ) where T : class
		{
			folder = folder ?? Application.persistentDataPath;
			var filepath = Path.Combine( folder, filename );

			if( !File.Exists( filepath ) )
				return null;

			using( var fileStream = File.Open( filepath, FileMode.Open ) )
			{
				var bf = new BinaryFormatter();
				bf.Binder = new VersionSerializationBinder();
				bf.SurrogateSelector = surrogateSelector;

				return bf.Deserialize( fileStream ) as T;	
			}
		}


		/// <summary>
		/// Serializes the object to file using a BinaryFormatter
		/// </summary>
		/// <param name="obj">Object. must be Serializeable!</param>
		/// <param name="filename">Filename.</param>
		/// <param name="folder">Folder. defaults to Application.persistantDataPath</param>
		public static void serializeObjectToFile( object obj, string filename, string folder = null )
		{
			folder = folder ?? Application.persistentDataPath;
			var filepath = Path.Combine( folder, filename );

			using( var fileStream = File.Open( filepath, FileMode.Create ) )
			{
				var bf = new BinaryFormatter();
				bf.Binder = new VersionSerializationBinder();
				bf.SurrogateSelector = surrogateSelector;

				bf.Serialize( fileStream, obj );
			}
		}


		/// <summary>
		/// creates a new object from the byte[] using a BinaryFormatter
		/// </summary>
		/// <returns>The byte array.</returns>
		/// <param name="data">Data.</param>
		public static T createFromByteArray<T>( byte[] data ) where T : class
		{
			using( var memoryStream = new MemoryStream( data ) )
			{
				var bf = new BinaryFormatter();
				bf.Binder = new VersionSerializationBinder();
				bf.SurrogateSelector = surrogateSelector;

				return bf.Deserialize( memoryStream ) as T;
			}
		}


		/// <summary>
		/// serializes the object to a byte[] using a BinaryFormatter
		/// </summary>
		/// <returns>The compressed byte array.</returns>
		public static byte[] toByteArray( object obj )
		{
			using( var memoryStream = new MemoryStream() )
			{
				var bf = new BinaryFormatter();
				bf.Binder = new VersionSerializationBinder();
				bf.SurrogateSelector = surrogateSelector;
				bf.Serialize( memoryStream, obj );

				return memoryStream.GetBuffer();
			}
		}


		#region BinaryFormatter Helper classes

		/// <summary>
		/// Helper class to serialize Vector2 and Vector3 values
		/// </summary>
		sealed class VectorSerializationSurrogate : ISerializationSurrogate
		{
			public void GetObjectData( object obj, SerializationInfo info, StreamingContext context )
			{
				// vector 2 and 3 can be cast to vector3
				if( obj is Vector2 )
				{
					var vec2 = (Vector2)obj;
					info.AddValue( "x", vec2.x );
					info.AddValue( "y", vec2.y );
				}
				else
				{
					var vec3 = (Vector3)obj;
					info.AddValue( "x", vec3.x );
					info.AddValue( "y", vec3.y );
					info.AddValue( "z", vec3.z );
				}
			}


			public object SetObjectData( object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector )
			{
				// vector 2 and 3 can be cast to vector3
				if( obj is Vector2 )
				{
					var vec2 = (Vector2)obj;
					vec2.x = (float)info.GetValue( "x", typeof( float ) );
					vec2.y = (float)info.GetValue( "y", typeof( float ) );
					return vec2;
				}
				else
				{
					var vec3 = (Vector3)obj;
					vec3.x = (float)info.GetValue( "x", typeof( float ) );
					vec3.y = (float)info.GetValue( "y", typeof( float ) );
					vec3.z = (float)info.GetValue( "z", typeof( float ) );
					return vec3;
				}
			}
		}


		// why? http://answers.unity3d.com/questions/8480/how-to-scrip-a-saveload-game-option.html
		sealed class VersionSerializationBinder : SerializationBinder
		{
			public override Type BindToType( string assemblyName, string typeName )
			{ 
				if( !string.IsNullOrEmpty( assemblyName ) && !string.IsNullOrEmpty( typeName ) )
				{ 
					assemblyName = Assembly.GetExecutingAssembly().FullName;
					return Type.GetType( String.Format( "{0}, {1}", typeName, assemblyName ) ); 
				} 

				return null; 
			}
		}

		#endregion

	}
}
