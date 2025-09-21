using SaveFramework;
using UnityEngine;

public class MoreTest : MonoBehaviour
{
    [Save(" My Int ")] public int myInt = 10;
    [Save(" My String ")] public string myString= "My String";
    [Save(" My Float ")] public float myFloat= 3.14f;
    [Save(" My Bool ")] public bool myBool= true;
    [Save(" My Vector3 ")] public Vector3 myVector3= new Vector3(1,2,3);
    [Save(" My Quaternion ")] public Quaternion myQuaternion= new Quaternion(1,2,3,4);
    [Save(" My Color ")] public Color myColor= Color.red;
    //其他基本类型
    [Save(" My Int Array ")] public int[] myIntArray= new int[]{1,2,3};
    [Save(" My String Array ")] public string[] myStringArray= new string[]{"Hello","World"};
    [Save(" My Float Array ")] public float[] myFloatArray= new float[]{1.2f,3.4f,5.6f};
    [Save(" My Bool Array ")] public bool[] myBoolArray= new bool[]{true,false,true};    
    [Save(" My Vector3 Array ")] public Vector3[] myVector3Array= new Vector3[]{new Vector3(1,2,3),new Vector3(4,5,6)};
    [Save(" My Quaternion Array ")] public Quaternion[] myQuaternionArray= new Quaternion[]{new Quaternion(1,2,3,4),new Quaternion(5,6,7,8)};
    [Save(" My Color Array ")] public Color[] myColorArray= new Color[]{Color.red,Color.green,Color.blue};  
    //其他类型
    [Save(" My UInt ") ]public uint myUInt= 10; 
    [Save(" My Long ")] public long myLong= 1000000000000000000;
    [Save(" My ULong ")] public ulong myULong= 1000000000000000000;
    [Save(" My Char ")] public char myChar= 'A';
    [Save(" My Byte ")] public byte myByte= 255;
   
  
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}