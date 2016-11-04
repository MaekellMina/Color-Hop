using UnityEngine; 
using System.Collections; 
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.IO;

/// < Step to use this class in the game >
/// Create string startTimeStamp for the naming of the file
/// Create a list of TrackEvent for adding of event
/// Call gameEvent.Add( TrackingDataClass.AddEvent( ) ) for adding of Event

public enum TrackEventType
{
	// 0000 - 0100 system event type
	NIL 			= 0000 , // Nil
	STARTGAME 		= 0001 , // Start game
	GAMEOVER 		= 0002 , // game Over
	NUMCORRECT		= 0003 , // number of Correct actions
	NUMWRONG		= 0004 , // number of Wrong actions
	CLICKDOWN_1		= 0005 , // Click Down 1
	CLICKUP_1		= 0006 , // Click Up 1 
	CLICKDOWN_2		= 0007 , // Click Down 2
	CLICKUP_2		= 0008 , // Click Up 2
	CORRECTANS		= 0009 , // Correct action
	WRONGANS		= 0010 , // Wrong action
	PAUSE			= 0012 , // pause screen activated
	RESUME			= 0013 , // resume button being activated
	RESTART			= 0014 , // restart button being activated
	HELP			= 0015 , // help button being activated
	QUIT			= 0016 , // quit button being activated
	SESSIONSTART	= 0017 , // start of game session
	SESSIONEND		= 0018 , // end of a game session
	TIMETAKEN		= 0020 , // time taken to complete a game session
	GAMESCORE		= 0021 , // the score of the whole game
	
}

public enum TaskID
{
	TOP 	= 0 ,
	BOTTOM 	= 1 ,
}

public enum TaskResult
{
	COMPLETED 	= 0 ,
	FAILED		= 1 ,
	TIMEOUT		= 2 ,
}

public enum Orientation
{
	NORMAL 			= 0 ,
	FLIPHORIZONTAL 	= 1 ,
	FLIPVERTICAL 	= 2 ,
	ROTATE180		= 3 , 
	ANTIROTATE90	= 4 , 
	ROTATE90		= 5 , 
}

public enum QuestionType
{
	MISSINGPERSON 	= 0 , 
	MISSINGNAME		= 1 ,
	FINDPERSON		= 2 ,
}

public enum BoardDir
{
	NORMAL 			= 0 ,
	CLOCKWISE 		= 1 ,
	ANTICLOCKWISE 	= 2	,
	HALFCIRCLE	 	= 3	,
}

[System.Serializable]
public class TrackEvent
{
    public string userID;
	public uint c_time = 0;
	public TrackEventType c_event;
	public uint value_1 = 0; 
	public short[] multiValue;
	public bool b_singleInput;
}

public class TrackingDataClass : MonoBehaviour {
	/*
	public static int SYSEVENT = 100;
	public static int GAMEEVENT = 200;
	public static int GAMEEVENT_FLOAT = 300;
	public static int GAMEEVENT_VECTOR2 = 400;
	public static int GAMEEVENT_VECTOR3 = 500;
	*/
	#if !UNITY_EDITOR
	public static string PATH;
	#else
	public static string PATH;
	#endif
	
	// These variables to be removed once everything is done
	/*
	public TrackEvent[] eventList_1;
	public TrackEvent[] eventList_2;
	public TrackEvent[] eventList_3;
	public TrackEvent[] eventList_4;
	*/
	
	public static List<byte> eventBuffer = new List<byte>();
	public static List<byte> headerBuffer = new List<byte>();
	
	public static long length = 0;
	public static bool b_createdHeader = false; 
	// Use this for initialization
	public static TrackingDataClass instance;
	public static Thread t;
	public static bool b_threadAlive = false;
	public static bool b_threadExit = false;
	// for adding in another thread~
	public static List<TrackEvent> addEvent = new List<TrackEvent> ();
	public static bool b_singleInput = false;
	public static string save_path = "";
	public static string gameName;
	public static string save_data;
	public static int save_numOfEvent;

	public static bool upload = false;
	public static bool uploaded = false;

//	public EEGHandler testeeg;
	
	void TrackingHandling ()
	{
		while (b_threadAlive) 
		{    
			//#if !UNITY_EDITOR
			if ( eventBuffer.Count>50 && gameName !="" )	// if it reach a certain buffer it will write into~
				EditSaveFile(PATH + "/" + gameName + ".nrox" );
			else if (addEvent.Count>0)	// save event to buffer
			{ 
				if (addEvent[0].b_singleInput)
					AddEventToBuffer(addEvent[0]);
				else
					AddMultiValueEventToBuffer(addEvent[0]);
				addEvent.RemoveAt(0);
			}  
			else if (save_path!="")
			{
				//WHAT THE HECK IS THIS! THERE IS A RACE CONDITION WITH THE UPLOADING. - ISAAC
				/*upload = true;
				uploaded = false;*/

				Debug.Log("DUMB THREAD PATH IS " + PATH);
				Debug.Log("DUMB THREAD GAMENAME IS " + gameName);

				ThreadSaveFile(PATH + "/" + gameName + ".nrox" ,"",save_data,save_numOfEvent); 
				save_path = ""; 
				ClearBuffer(); 
				//Awake(); THIS IS ANOTHER FREAKING RACE CONDITION!
			}
			else
				//#endif
				Thread.Sleep(100);
		}
		print ("Exit");
	}
    
	
	void OnDisable ()
	{ 
		#if UNITY_EDITOR
		//b_threadAlive = false;  
		#endif
	}
	
	void OnApplicationQuit ()
	{
		b_threadAlive = false;  
	}
	
	void OnDestroy ()
	{
		if (instance == this) 
		{ 
			b_threadAlive = false;  
		}
	}
	
	void Awake ()
	{  
		#if !UNITY_EDITOR
		PATH = Application.persistentDataPath+"/";
		#else
		PATH = Application.dataPath + "/SaveFile/";
		#endif 
		// reset everything here~
		b_createdHeader = false;
		save_path = "";
		gameName = "";
		length = 0;

		//testeeg = GameObject.Find("MainGame").GetComponent<EEGHandler>();
	}

	void resetValues()
	{
		#if !UNITY_EDITOR
		PATH = Application.persistentDataPath+"/";
		#else
		PATH = Application.dataPath + "/SaveFile/";
		#endif 

		b_createdHeader = false;
		save_path = "";
		gameName = "";
		length = 0;

	}
	
	void Start () {  
		
		if (instance == null) 
		{
			//if(!Directory.Exists(PATH)) 
			//	Directory.CreateDirectory(PATH);
			instance = this;
			b_threadAlive = true;
			t = new Thread (TrackingHandling);
			t.Start ();
			t.IsBackground = true; 
			DontDestroyOnLoad (this.gameObject);
		} 
		else if (this != instance)
			Destroy (this.gameObject);
	} 
	
	public static void EditSaveFile (string path )
	{ 
		System.IO.FileStream tmp1;
		if (File.Exists (path)) 
		{ 
			tmp1 = System.IO.File.Open (path,FileMode.Append);  
		} else 
		{
			tmp1 = System.IO.File.Create (path);  
			List<byte> blankHeader = new List<byte>();
			for (int i = 0; i<32;i++)
				blankHeader.Add ((byte)0);
			tmp1.Write(blankHeader.ToArray() , (int)0 , blankHeader.Count);
		}
		tmp1.Write(eventBuffer.ToArray() , (int)0 , eventBuffer.Count);
		length = tmp1.Length;
		tmp1.Close();
		eventBuffer.Clear ();  
	}
	
	// this save file to binary format
	// use Application.persistentDataPath for ios ( not sure about android )
	// use Application.datapath or Application.streamingData for other!
	public static bool ThreadSaveFile (string path , string tmpTimeStamp , string tmpData , int numOfEvent)
	{    
		
		//byte[] testByteArray;  
		try 
		{ 
			System.IO.FileStream tmp1;
			if (File.Exists(path))
				tmp1 = System.IO.File.OpenWrite(path); 
			else
				tmp1 = System.IO.File.Create(path); 
			// maybe need to do a constant writing instead write all at the same time!
			//tmpTimeStamp = tmpTimeStamp.Insert( 28 , ConvertToHexInt( numOfEvent , 4 ) );
			CloseHeader(numOfEvent);
			tmp1.Write(headerBuffer.ToArray() , (int)0 , headerBuffer.Count);
			tmp1.Close();
			tmp1 = System.IO.File.Open (path,FileMode.Append);  
			tmp1.Write(eventBuffer.ToArray() , 0 , eventBuffer.Count);
			tmp1.Close();

			//to begin upload of files
			upload = true;
			uploaded = false;

			//System.IO.File.WriteAllBytes(path, headerBuffer.ToArray()); // Requires System.Linq
			/*
			testByteArray = System.Text.Encoding.UTF8.GetBytes( tmpTimeStamp + tmpData );
			System.IO.FileStream tmp = System.IO.File.Create(path); 
			tmp.Write(testByteArray , 0 , testByteArray.Length);
			tmp.Close();
			*/
			return true;
		}
		catch( System.Exception ) 
		{

			upload = true;
			uploaded = false;

			return false;
		}
	}
	public static bool SaveFile (string path , string tmpTimeStamp , string tmpData , int numOfEvent)
	{    
		save_path = path;
		save_data = tmpData;
		save_numOfEvent = numOfEvent;
		return false;
	}
	
	//single value event 
	public static void AddEventToBuffer ( TrackEvent tmpList )
	{
        AddToBufferString(ref eventBuffer, tmpList.userID);
		// add the millisecond hex to the byte
		AddToBufferInt(ref eventBuffer , int.Parse(ConvertToHexInt( tmpList.c_time , 8 ),NumberStyles.HexNumber));
		AddToBufferShort(ref eventBuffer , short.Parse(ConvertToHexInt( (uint)tmpList.c_event, 4 ),NumberStyles.HexNumber));
		// add the value hex to the byte
		AddToBufferInt(ref eventBuffer , int.Parse(ConvertToHexInt( tmpList.value_1 , 8 ),NumberStyles.HexNumber));
		// add the game code string to the byte 
		for (int i = 0;i<20;i++) 
			AddToBufferByte(ref eventBuffer , (byte)short.Parse(ConvertToHexInt( 0 , 2 ),NumberStyles.HexNumber));
		
	}
	
	//multi value event 
	public static void AddMultiValueEventToBuffer ( TrackEvent tmpList )
	{
        AddToBufferString(ref eventBuffer, tmpList.userID);
		// add the millisecond hex to the byte
		AddToBufferInt(ref eventBuffer , int.Parse(ConvertToHexInt( tmpList.c_time , 8 ),NumberStyles.HexNumber));
		AddToBufferShort(ref eventBuffer , short.Parse(ConvertToHexInt( (uint)tmpList.c_event, 4 ),NumberStyles.HexNumber));
		// add the game code string to the byte  
		for (int i = 0;i<24;i++) 
		{
			if (i<tmpList.multiValue.Length)
				AddToBufferByte(ref eventBuffer , (byte)short.Parse(ConvertToHexInt( tmpList.multiValue[i] , 2 ),NumberStyles.HexNumber));
			else
				AddToBufferByte(ref eventBuffer , (byte)short.Parse(ConvertToHexInt( 0 , 2 ),NumberStyles.HexNumber));
		}
		
	}
	public static string TimelineToString ( List<TrackEvent> tmpList )
	{  
		string finalOutput = "";
		/*
		for( int i = 0 ; i < tmpList.Count; i++ )
		{  
			// add the millisecond hex to the byte
			//AddToBufferInt(ref eventBuffer , int.Parse(ConvertToHexInt( tmpList[i].c_time , 8 ),NumberStyles.HexNumber));
			// add the game code string to the byte
			//AddToBufferString(ref eventBuffer , tmpList[i].gameType.ToString());
			//AddToBufferShort(ref eventBuffer , short.Parse(ConvertToHexInt( (uint)tmpList[i].c_event, 4 ),NumberStyles.HexNumber));
			// add the value hex to the byte
			//AddToBufferInt(ref eventBuffer , int.Parse(ConvertToHexInt( tmpList[i].value_1 , 8 ),NumberStyles.HexNumber));
			// add the game code string to the byte
			//AddToBufferString(ref eventBuffer , "00");
			

			finalOutput += ConvertToHexInt( tmpList[i].c_time , 8 ) + tmpList[i].gameType.ToString() 
				+ ConvertToHexInt( (uint)tmpList[i].c_event , 4 );
			finalOutput += ConvertToHexInt( tmpList[i].value_1 , 8 ); 

		}
		*/
		return finalOutput;
	} 
	
	string SortTimeline ( List<TrackEvent[]> tmpList )
	{
		List<TrackEvent> sortList = new List<TrackEvent>();
		for( int i = 0 ; i < tmpList.Count ; i++ )
		{
			for( int z = 0 ; z < tmpList[i].Length ; z++ )
			{ 
				// add to the correct position
				bool b_add = false;
				for( int y = 0 ; y < sortList.Count ; y++ )
				{
					if( sortList[y].c_time >= tmpList[i][z].c_time )
					{
						b_add = true;
						sortList.Insert( y , tmpList[i][z] );
						y = sortList.Count;
					}
				}
				// mean the list is empty
				if( !b_add ) sortList.Add(tmpList[i][z]); 
			}
		}
		
		string finalOutput = "";
		for( int i = 0 ; i < sortList.Count ; i++ )
		{ 
            
			// add the millisecond hex to the byte
			AddToBufferInt(ref eventBuffer , int.Parse(ConvertToHexInt( sortList[i].c_time , 8 ),NumberStyles.HexNumber));
            // add the user id string to the byte
            AddToBufferString(ref eventBuffer, sortList[i].userID);
            
            AddToBufferShort(ref eventBuffer , short.Parse(ConvertToHexInt( (uint)sortList[i].c_event, 4 ),NumberStyles.HexNumber));
			// add the value hex to the byte
			AddToBufferInt(ref eventBuffer , int.Parse(ConvertToHexInt( sortList[i].value_1 , 8 ),NumberStyles.HexNumber));
			// add the game code string to the byte
			AddToBufferString(ref eventBuffer , "00");
			
			//string tmp = ConvertToHexInt( sortList[i].c_time , 8 );
			//finalOutput += tmp + sortList[i].gameType.ToString();
			//tmp =  ConvertToHexInt( (uint)sortList[i].c_event, 4 );
			//finalOutput += tmp;
			//tmp = ConvertToHexInt( sortList[i].value_1 , 8 ); 
		}
		
		/*
		System.IO.FileStream tmp1 = System.IO.File.Create(PATH  + "2.nrox" ); 
		tmp1.Write(eventBuffer.ToArray() , 0 , eventBuffer.Count);
		tmp1.Close();
		*/
		return finalOutput;
	}
	
	public static void ClearBuffer ()
	{
		eventBuffer.Clear();
		headerBuffer.Clear(); 
	}
	
	public static void CreateHeader (int gameID, int level)
	{
		// to create a file name for the file~
		gameName = GetFileName (gameID, level);
		b_createdHeader = true;
		string startTimeStamp = System.DateTime.Now.ToString();
		startTimeStamp = startTimeStamp.Replace( "/" , "-" );
		startTimeStamp = startTimeStamp.Replace( " " , "-" );
		startTimeStamp = startTimeStamp.Replace( ":" , "-" );
		string[] tmpSplit = startTimeStamp.Split('-');
		string finalString = "";
		for(int i = 0 ; i < tmpSplit.Length ; i++ )
		{
			switch( i )
			{
			case 0: // day
				if( tmpSplit[i].Length == 1 ) finalString = "0" + tmpSplit[i];
				else finalString = tmpSplit[i];
				break;
			case 1: // month
			case 3: // hours
			case 4: // minutes
			case 5: // second
				if( tmpSplit[i].Length == 1 ) finalString += "0" + tmpSplit[i];
				else finalString += tmpSplit[i];
				break;
			case 2: // year
				finalString += tmpSplit[i];
				break;
			}
		} 
		long tmp = System.Convert.ToInt64( finalString );  
		finalString = ConvertToHexInt( tmp , 16 );  
		AddToBufferString(ref headerBuffer, "NROX01" ); 
		AddToBufferLong(ref headerBuffer , long.Parse(finalString,NumberStyles.HexNumber) );
		AddToBufferInt(ref headerBuffer , int.Parse(32161632+"",NumberStyles.HexNumber) ); 
	}
	
	private static void CloseHeader ( int numOfEvent )
	{
		AddToBufferShort(ref headerBuffer, short.Parse(ConvertToHexInt( numOfEvent , 4 ),NumberStyles.HexNumber));
		AddToBufferString(ref headerBuffer,"            " );
		/*
		// for testing purpose only!
		System.IO.FileStream tmp1 = System.IO.File.Create(PATH  + "1.nrox" ); 
		tmp1.Write(headerBuffer.ToArray() , 0 , headerBuffer.Count);
		tmp1.Close();*/
	}
	
	public static bool AddToBufferString(ref List<byte> buffer , string tmpBuffer )
	{ 
		byte[] intBytes =  System.Text.Encoding.UTF8.GetBytes( tmpBuffer);
		//System.Array.Reverse(intBytes);
		List<byte> result = intBytes.ToList(); 
		buffer = buffer.Concat(result).ToList();
		return true;
	}
	
	public static bool AddToBufferLong(ref List<byte> buffer , long tmpBuffer )
	{ 
		byte[] intBytes = System.BitConverter.GetBytes((long)tmpBuffer);
		System.Array.Reverse(intBytes);
		List<byte> result = intBytes.ToList(); 
		buffer = buffer.Concat(result).ToList();
		return true;
	}
	
	public static bool AddToBufferInt(ref List<byte> buffer , int tmpBuffer )
	{ 
		byte[] intBytes = System.BitConverter.GetBytes(tmpBuffer);
		System.Array.Reverse(intBytes);
		List<byte> result = intBytes.ToList(); 
		buffer = buffer.Concat(result).ToList();
		return true;
	}
	
	public static bool AddToBufferByte(ref List<byte> buffer ,byte tmpBuffer)
	{ 
		byte[] intBytes = new byte[1];
		intBytes[0] = tmpBuffer;
		System.Array.Reverse(intBytes);
		List<byte> result = intBytes.ToList(); 
		buffer = buffer.Concat(result).ToList();
		return true;
	}
	
	public static bool AddToBufferShort(ref List<byte> buffer , short tmpBuffer )
	{ 
		byte[] intBytes = System.BitConverter.GetBytes(tmpBuffer);
		System.Array.Reverse(intBytes);
		List<byte> result = intBytes.ToList(); 
		buffer = buffer.Concat(result).ToList();
		return true;
	}
	
	public static TrackEvent AddEvent ( string tmpUserID, uint tmpTime, TrackEventType tmpEvent , uint tmpValue = 0 )
	{  
		TrackEvent tmpEventObj = new TrackEvent();
		//if (!b_createdHeader) return tmpEventObj;
        tmpEventObj.userID = tmpUserID;
		tmpEventObj.c_time = tmpTime;
		tmpEventObj.c_event = tmpEvent;
		tmpEventObj.value_1 = tmpValue;
		tmpEventObj.b_singleInput = true;
		addEvent.Add (tmpEventObj); 
		//AddEventToBuffer( tmpEventObj );
		return tmpEventObj;
	}
	
	public static TrackEvent AddEvent ( string tmpUserID, uint tmpTime , TrackEventType tmpEvent , short[] tmpValue )
	{  
		TrackEvent tmpEventObj = new TrackEvent();
		// (!b_createdHeader) return tmpEventObj;
        tmpEventObj.userID = tmpUserID;
		tmpEventObj.c_time = tmpTime;
		tmpEventObj.c_event = tmpEvent;
		tmpEventObj.multiValue = tmpValue;
		tmpEventObj.b_singleInput = false; 
		addEvent.Add (tmpEventObj); 
		//AddMultiValueEventToBuffer( tmpEventObj );
		return tmpEventObj;
	}
	
	public static string ConvertToHexInt( long tmpValue , int tmpBytes )
	{
		// Convert to hex 
		string tmpString =  tmpValue.ToString ("X"+tmpBytes); 
		return tmpString;
	}
	
	public static string GetFileName (int gameID, int level)
	{
		string tmpFileName = System.DateTime.Now.ToString("yyyyMMdd-HHmmss-01-" + gameID.ToString("00#") +"-"+ level.ToString("0#")); 
		tmpFileName = tmpFileName.Replace( "/" , "-" );
		tmpFileName = tmpFileName.Replace( " " , "-" );
		tmpFileName = tmpFileName.Replace( ":" , "-" );
		return tmpFileName;
	}
	
	// functions for testing purposes only!
	/*
	IEnumerator Test ()
	{
		List<TrackEvent[]> tmpList = new List<TrackEvent[]>();
		tmpList.Add(eventList_1);
		tmpList.Add(eventList_2);
		tmpList.Add(eventList_3);
		tmpList.Add(eventList_4);
		string testStr = "";
		CreateHeader();
		//testStr += SortTimeline(tmpList);
		string fileName = "Neeuro-Data-"+System.DateTime.Now.ToString();
		fileName = fileName.Replace( "/" , "-" );
		fileName = fileName.Replace( " " , "-" );
		fileName = fileName.Replace( ":" , "-" );
		SaveFile(PATH + fileName + ".nrox" , testStr , SortTimeline(tmpList) , tmpList.Count ); 
		/*
		byte[] testByteArray = System.IO.File.ReadAllBytes( PATH + fileName + ".nrox");  
		string tmpp =  System.Text.Encoding.UTF8.GetString(testByteArray); 
		// the timestamp of the file
		print ( tmpp.Substring( 0 , 35 ) );
		tmpp = tmpp.Substring( 35 ); 
		//ExtractData(tmpp);
		*/
	/*
		yield return null;
	}
	*/
} 
