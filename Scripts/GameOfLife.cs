using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;


public class Coords {
	public byte X {get;set;}
	public byte Y {get;set;}
	public byte Z {get;set;}
                
	public Coords(byte x, byte y, byte z) {
		X=x;
		Y=y;
		Z=z;
	}
	public void SetValues(byte x,byte y,byte z) {
		X=x;
		Y=y;
		Z=z;
	}
}


public class Cell  {
	public GameObject Cube;
	public static GameObject ObjectToInstatiate;
	public static float scalefactor;
	public static GameObject parent;
	public Cell(byte x, byte y, byte z,byte isOn) {
		Vector3 position = new Vector3(x,y,z);
		position = position * scalefactor;
		Cube = GameObject.Instantiate(ObjectToInstatiate, position, Quaternion.identity);
		Cube.transform.parent = parent.transform;
		Show(isOn);
	}
	public void Show(byte state) {
		
		Cube.SetActive(state == 1);
	}
}



public class GameOfLife : MonoBehaviour
{
	[Header("Presentation")]
	public GameObject cell;
	public Vector3 XYZDimensions;
	public float cellSpacing = 0.1f;
	public bool hideMouse = false;
	[Space]
	[Header("Rules")]
	public AnimationCurve limitCurve;
	public int blockCountMultiplier = 1000;
	public int neigbourLimitMultiplier = 10;


	[Space]
	[Header("Random Spawn")]
	public bool randomSpawn = false;
	public int SpawnChance = 2;
	public Vector3 SpawnDimensions;
	[Space]
	[Header("Performance")]
	public float waitInterval = 10f;
	private Coords XYZLocation;
	private Coords XYZInt;
	private Cell[,,] cellArray;
	private byte[] baseMap;
	private byte[] nextMap;
	[Space]
	private bool state = false;
	private bool updating = false;

	[Space]
	[Header("Diagnostics")]
	public bool running = false;
	public float fps = 0f;
	public int blockCount = 0;
	public int neighbourLimit;
	
	private int npoint = 0;
                
	void updateArrays() {
		//for (int i = 0; i <= 1000; i++) {
		//	Debug.LogWarning(i.ToString()+"<>"+nextMap[i].ToString());
		//}
		byte oldvis = 0;
		byte newvis = 0;
		int apoint = 0;
		blockCount = 0;
		for (byte x = 0; x < XYZInt.X; x++) {
			for (byte y = 0; y < XYZInt.Y; y++) {
				for (byte z = 0; z < XYZInt.Z; z++) {
					apoint = (x << 16) + (y << 8) + z;					
					oldvis = (byte)(baseMap[apoint] & 0x01);
					newvis = (byte)(nextMap[apoint] & 0x01);
					if (oldvis != newvis) {
						if (cellArray[x,y,z] == null) {
							cellArray[x,y,z] = new Cell(x,y,z,newvis);
						} else {
							cellArray[x,y,z].Show(newvis);
						}
						if (newvis ==1) blockCount++;
					}
					baseMap[apoint] = nextMap[apoint];					
				}
			}
		}
		
		// This seems to produce different results ?????  Really doesn't work properly
		//Array.Copy(nextMap,baseMap,XYZInt.X*XYZInt.Y*XYZInt.Z);
	}

	void Setup()
	{
		if (!Application.isEditor || hideMouse)
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
		}
	
		Cell.ObjectToInstatiate = cell;
		Cell.scalefactor = cellSpacing;
		Cell.parent = gameObject;
		
		XYZInt = new Coords((byte)XYZDimensions.x,(byte)XYZDimensions.y,(byte)XYZDimensions.z); 
		
		cellArray = new Cell[XYZInt.X+1,XYZInt.Y+1,XYZInt.Z+1];
		int ArraySize = (256 * 256 * 256);
		
		baseMap = new byte[ArraySize]; 
		nextMap = new byte[ArraySize]; 
		Reset();	
	}
	void Reset()
	{
		neighbourLimit = neigbourLimitMultiplier;
		int ArrayPointer = 0;
                    
		for (byte x = 0; x < XYZInt.X; x++) {
			for (byte y = 0; y < XYZInt.Y; y++) {
				for (byte z = 0; z < XYZInt.Z; z++) {
					ArrayPointer = (x << 16) + (y << 8) + z;
					baseMap[ArrayPointer] = 0;
					nextMap[ArrayPointer] = 0;
					if (cellArray[x,y,z] != null) {cellArray[x,y,z].Show(0);}
					//cellArray[x,y,z] = new Cell(x,y,z,0);  
				}                                              
			}
		}
		if (randomSpawn) {
			GenerateRandomMap();
		} else {
			GenerateStartingMap();
		}
		updateArrays();
		
	}
	void Set(int whichOne) {
		nextMap[whichOne] = 1;
	}
	void Set(byte xpos,byte ypos, byte zpos) {
		npoint = (xpos << 16) + (ypos << 8) + zpos;
		nextMap[npoint] |= 1;
		for (int x = xpos-1; x <= xpos+1; x++) {
			for (int y = ypos-1; y <= ypos+1; y++) {
				for (int z = zpos-1; z <= zpos+1; z++) {
					if (z >= 0 && z < 0xFF && y >=0 && y <  0xFF && x >= 0 && x <  0xFF) {
						if(x != xpos || y != ypos || z != zpos) {
							npoint = (x << 16) + (y << 8) + z;
							if (nextMap[npoint] < 52) nextMap[npoint] += 2;
						}
					}
				}
	                                                                                
			}
		}
	}
	void Clear(int whichOne) {
		nextMap[whichOne] = 0;
	}
	void Clear(byte xpos, byte ypos, byte zpos) {
		npoint = (xpos << 16) + (ypos << 8) + zpos;
		nextMap[npoint] &= 0xFE;
		for (int x = xpos-1; x <= xpos+1; x++) {
			for (int y = ypos-1; y <= ypos+1; y++) {
				for (int z = zpos-1; z <= zpos+1; z++) {
					if (z >= 0 && z < 0xFF && y >=0 && y <  0xFF && x >= 0 && x <  0xFF) {
						if(x != xpos || y != ypos || z != zpos) {
							npoint = (x << 16) + (y << 8) + z;
							if (nextMap[npoint] > 1) {
								nextMap[npoint] -= 2;
							}
						}
					}
				}
	                                                                                
			}
		}
	}
	void DebugDisplay(byte x, byte y, byte z) {
		npoint = (x << 16) + (y << 8) + z;
		Debug.Log(x.ToString()+":"+y.ToString()+":"+z.ToString()+" "+baseMap[npoint].ToString() + "<>"+nextMap[npoint].ToString());
	}
	void GenerateStartingMap() {
		byte x = (byte)(XYZInt.X/2);
		byte y = (byte)(XYZInt.Y/2);
		byte z = (byte)(XYZInt.Z/2);
		Set(x,y,z);
		Set(x,(byte)(y+1),z);
		Set(x,y,(byte)(z+1));
		Set(x,(byte)(y+1),(byte)(z+1));
		//Set((byte)(x+1),(byte)(y+2),(byte)(z));
		//Set((byte)(x+1),(byte)(y+2),(byte)(z+1));
		//Set((byte)(x+1),(byte)(y-1),(byte)(z));
		//Set((byte)(x+1),(byte)(y-1),(byte)(z+1));


	}
	
	void GenerateRandomMap() {
		Coords XYZMinSpawn = new Coords((byte)(XYZDimensions.x/2 - SpawnDimensions.x/2),(byte)(XYZDimensions.y/2-SpawnDimensions.y/2),(byte)(XYZDimensions.z/2-SpawnDimensions.z/2));
		Coords XYZMaxSpawn = new Coords((byte)(XYZDimensions.x/2 + SpawnDimensions.x/2),(byte)(XYZDimensions.y/2+SpawnDimensions.y/2),(byte)(XYZDimensions.z/2+SpawnDimensions.z/2));
		
		for (byte x = 0; x < XYZInt.X; x++) {
			for (byte y = 0; y < XYZInt.Y; y++) {
				for (byte z = 0; z < XYZInt.Z; z++) {
					XYZLocation = new Coords(x,y,z);
					if (x > XYZMinSpawn.X && x < XYZMaxSpawn.X && y > XYZMinSpawn.Y && y < XYZMaxSpawn.Y && z > XYZMinSpawn.Z && z < XYZMaxSpawn.Z) 
					{                                                            						
						int rand = UnityEngine.Random.Range(0,SpawnChance); 
						if (rand == 0) {state = true;} else {state = false;}
						if (state) {
							Set(x,y,z);
						}
					}
				}
			}
		}
	}
	IEnumerator Next() {
		byte state = 0;
		byte neighbours = 0;
		int pointer = 0;
		Coords whichOne = new Coords(0,0,0);
		updating = true; 
		for (byte x = 0; x < XYZInt.X; x++) {
			for (byte y = 0; y < XYZInt.Y; y++) {
				for (byte z = 0; z < XYZInt.Z; z++) {
					pointer = (x << 16) + (y << 8) + z;
					state = (byte)(baseMap[pointer] & 0x01);
					neighbours = (byte)(baseMap[pointer] >> 1);
	
					//-------------------------Here be the rules
					if (neighbours == 4 && state == 0) {
						
						Set(x,y,z);
						
					} else if (state == 1 || neighbours > neighbourLimit) {  // currently on
                                                                                                
						Clear(x,y,z);
						
					}
					//------------------------End of the rules
				}
			}
		}
		updateArrays();
		float nl = limitCurve.Evaluate((float)blockCount/(float)blockCountMultiplier);
		
		neighbourLimit = (int)(nl* neigbourLimitMultiplier);
		if (neighbourLimit < 1) {neighbourLimit = 1;}
		if (waitInterval > 1) {
			yield return new WaitForSeconds(waitInterval/100);
		}
		updating = false;
	}
    
	void Start(){
		Setup();                          
	}
	void Update() {
		if (Input.GetKeyUp(KeyCode.Space)) {
			running = !running;
		} else if (Input.GetKeyUp(KeyCode.KeypadPlus)) {
			if (waitInterval > 1) {
				waitInterval /= 2;
			}
		} else if (Input.GetKeyUp(KeyCode.KeypadMinus)){
			if (waitInterval < 30) {
				waitInterval *= 2;
			}
		} else if (Input.GetKeyUp(KeyCode.Escape)) {
			Application.Quit();
		}
		if (Input.GetKeyUp(KeyCode.R) || blockCount == 0) {
			Reset();
		}
		if (running) {
			if (!updating) {
				StartCoroutine(Next());
				fps = 1f/Time.deltaTime;
			}
		} else {
			if (Input.GetMouseButtonUp(0)) {
				StartCoroutine(Next());
			}
		}
	}

}
