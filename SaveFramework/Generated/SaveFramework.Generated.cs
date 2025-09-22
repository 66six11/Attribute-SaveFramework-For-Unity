// 此文件由保存框架代码生成器自动生成
// 请勿手动修改此文件，因为更改将被覆盖
// 生成时间： 2025-09-22 00:01:42

using System;
using System.Collections.Generic;
using SaveFramework.Runtime.Core;
using UnityEngine;

namespace SaveFramework.Generated
{
    public class SaveFrameworkRegistry : GeneratedSaveRegistry
    {
        private static readonly Type[] HandledTypes = new Type[]
        {
            typeof(MoreTest),
            typeof(SaveFramework.Sample.PlayerStats),
        };

        public override bool HandlesType(Type componentType)
        {
            foreach (var handledType in HandledTypes)
            {
                if (handledType == componentType)
                    return true;
            }
            return false;
        }

        public override Type[] GetHandledTypes()
        {
            return HandledTypes;
        }

        public override void RegisterSaveEntries(Type componentType, Dictionary<string, SaveEntry> entries)
        {
            if (componentType == typeof(MoreTest))
            {
                // 字段: myInt
                var aliases_myInt_0 = new string[0];
                entries[" My Int "] = new SaveEntry(
                    " My Int ",
                    aliases_myInt_0,
                    typeof(MoreTest),
                    "myInt",
                    typeof(int),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((MoreTest)obj).myInt,
                    (obj, val) => ((MoreTest)obj).myInt = (int)val
                );

                // 字段: myString
                var aliases_myString_1 = new string[0];
                entries[" My String "] = new SaveEntry(
                    " My String ",
                    aliases_myString_1,
                    typeof(MoreTest),
                    "myString",
                    typeof(string),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((MoreTest)obj).myString,
                    (obj, val) => ((MoreTest)obj).myString = (string)val
                );

                // 字段: myFloat
                var aliases_myFloat_2 = new string[0];
                entries[" My Float "] = new SaveEntry(
                    " My Float ",
                    aliases_myFloat_2,
                    typeof(MoreTest),
                    "myFloat",
                    typeof(float),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((MoreTest)obj).myFloat,
                    (obj, val) => ((MoreTest)obj).myFloat = (float)val
                );

                // 字段: myBool
                var aliases_myBool_3 = new string[0];
                entries[" My Bool "] = new SaveEntry(
                    " My Bool ",
                    aliases_myBool_3,
                    typeof(MoreTest),
                    "myBool",
                    typeof(bool),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((MoreTest)obj).myBool,
                    (obj, val) => ((MoreTest)obj).myBool = (bool)val
                );

                // 字段: myVector3
                var aliases_myVector3_4 = new string[0];
                entries[" My Vector3 "] = new SaveEntry(
                    " My Vector3 ",
                    aliases_myVector3_4,
                    typeof(MoreTest),
                    "myVector3",
                    typeof(UnityEngine.Vector3),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((MoreTest)obj).myVector3,
                    (obj, val) => ((MoreTest)obj).myVector3 = (UnityEngine.Vector3)val
                );

                // 字段: myQuaternion
                var aliases_myQuaternion_5 = new string[0];
                entries[" My Quaternion "] = new SaveEntry(
                    " My Quaternion ",
                    aliases_myQuaternion_5,
                    typeof(MoreTest),
                    "myQuaternion",
                    typeof(UnityEngine.Quaternion),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((MoreTest)obj).myQuaternion,
                    (obj, val) => ((MoreTest)obj).myQuaternion = (UnityEngine.Quaternion)val
                );

                // 字段: myColor
                var aliases_myColor_6 = new string[0];
                entries[" My Color "] = new SaveEntry(
                    " My Color ",
                    aliases_myColor_6,
                    typeof(MoreTest),
                    "myColor",
                    typeof(UnityEngine.Color),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((MoreTest)obj).myColor,
                    (obj, val) => ((MoreTest)obj).myColor = (UnityEngine.Color)val
                );

                // 字段: myIntArray
                var aliases_myIntArray_7 = new string[0];
                entries[" My Int Array "] = new SaveEntry(
                    " My Int Array ",
                    aliases_myIntArray_7,
                    typeof(MoreTest),
                    "myIntArray",
                    typeof(int[]),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((MoreTest)obj).myIntArray,
                    (obj, val) => ((MoreTest)obj).myIntArray = (int[])val
                );

                // 字段: myStringArray
                var aliases_myStringArray_8 = new string[0];
                entries[" My String Array "] = new SaveEntry(
                    " My String Array ",
                    aliases_myStringArray_8,
                    typeof(MoreTest),
                    "myStringArray",
                    typeof(string[]),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((MoreTest)obj).myStringArray,
                    (obj, val) => ((MoreTest)obj).myStringArray = (string[])val
                );

                // 字段: myFloatArray
                var aliases_myFloatArray_9 = new string[0];
                entries[" My Float Array "] = new SaveEntry(
                    " My Float Array ",
                    aliases_myFloatArray_9,
                    typeof(MoreTest),
                    "myFloatArray",
                    typeof(float[]),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((MoreTest)obj).myFloatArray,
                    (obj, val) => ((MoreTest)obj).myFloatArray = (float[])val
                );

                // 字段: myBoolArray
                var aliases_myBoolArray_10 = new string[0];
                entries[" My Bool Array "] = new SaveEntry(
                    " My Bool Array ",
                    aliases_myBoolArray_10,
                    typeof(MoreTest),
                    "myBoolArray",
                    typeof(bool[]),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((MoreTest)obj).myBoolArray,
                    (obj, val) => ((MoreTest)obj).myBoolArray = (bool[])val
                );

                // 字段: myVector3Array
                var aliases_myVector3Array_11 = new string[0];
                entries[" My Vector3 Array "] = new SaveEntry(
                    " My Vector3 Array ",
                    aliases_myVector3Array_11,
                    typeof(MoreTest),
                    "myVector3Array",
                    typeof(UnityEngine.Vector3[]),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((MoreTest)obj).myVector3Array,
                    (obj, val) => ((MoreTest)obj).myVector3Array = (UnityEngine.Vector3[])val
                );

                // 字段: myQuaternionArray
                var aliases_myQuaternionArray_12 = new string[0];
                entries[" My Quaternion Array "] = new SaveEntry(
                    " My Quaternion Array ",
                    aliases_myQuaternionArray_12,
                    typeof(MoreTest),
                    "myQuaternionArray",
                    typeof(UnityEngine.Quaternion[]),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((MoreTest)obj).myQuaternionArray,
                    (obj, val) => ((MoreTest)obj).myQuaternionArray = (UnityEngine.Quaternion[])val
                );

                // 字段: myColorArray
                var aliases_myColorArray_13 = new string[0];
                entries[" My Color Array "] = new SaveEntry(
                    " My Color Array ",
                    aliases_myColorArray_13,
                    typeof(MoreTest),
                    "myColorArray",
                    typeof(UnityEngine.Color[]),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((MoreTest)obj).myColorArray,
                    (obj, val) => ((MoreTest)obj).myColorArray = (UnityEngine.Color[])val
                );

                // 字段: myUInt
                var aliases_myUInt_14 = new string[0];
                entries[" My UInt "] = new SaveEntry(
                    " My UInt ",
                    aliases_myUInt_14,
                    typeof(MoreTest),
                    "myUInt",
                    typeof(uint),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((MoreTest)obj).myUInt,
                    (obj, val) => ((MoreTest)obj).myUInt = (uint)val
                );

                // 字段: myLong
                var aliases_myLong_15 = new string[0];
                entries[" My Long "] = new SaveEntry(
                    " My Long ",
                    aliases_myLong_15,
                    typeof(MoreTest),
                    "myLong",
                    typeof(long),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((MoreTest)obj).myLong,
                    (obj, val) => ((MoreTest)obj).myLong = (long)val
                );

                // 字段: myULong
                var aliases_myULong_16 = new string[0];
                entries[" My ULong "] = new SaveEntry(
                    " My ULong ",
                    aliases_myULong_16,
                    typeof(MoreTest),
                    "myULong",
                    typeof(ulong),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((MoreTest)obj).myULong,
                    (obj, val) => ((MoreTest)obj).myULong = (ulong)val
                );

                // 字段: myChar
                var aliases_myChar_17 = new string[0];
                entries[" My Char "] = new SaveEntry(
                    " My Char ",
                    aliases_myChar_17,
                    typeof(MoreTest),
                    "myChar",
                    typeof(char),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((MoreTest)obj).myChar,
                    (obj, val) => ((MoreTest)obj).myChar = (char)val
                );

                // 字段: myByte
                var aliases_myByte_18 = new string[0];
                entries[" My Byte "] = new SaveEntry(
                    " My Byte ",
                    aliases_myByte_18,
                    typeof(MoreTest),
                    "myByte",
                    typeof(byte),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((MoreTest)obj).myByte,
                    (obj, val) => ((MoreTest)obj).myByte = (byte)val
                );

                // 字段: mySByte
                var aliases_mySByte_19 = new string[0];
                entries[" My SByte "] = new SaveEntry(
                    " My SByte ",
                    aliases_mySByte_19,
                    typeof(MoreTest),
                    "mySByte",
                    typeof(sbyte),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((MoreTest)obj).mySByte,
                    (obj, val) => ((MoreTest)obj).mySByte = (sbyte)val
                );

                // 字段: myBounds
                var aliases_myBounds_20 = new string[0];
                entries[" My Bounds "] = new SaveEntry(
                    " My Bounds ",
                    aliases_myBounds_20,
                    typeof(MoreTest),
                    "myBounds",
                    typeof(UnityEngine.Bounds),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((MoreTest)obj).myBounds,
                    (obj, val) => ((MoreTest)obj).myBounds = (UnityEngine.Bounds)val
                );

                return;
            }

            if (componentType == typeof(SaveFramework.Sample.PlayerStats))
            {
                // 字段: Health
                var aliases_Health_0 = new string[0];
                entries["health"] = new SaveEntry(
                    "health",
                    aliases_Health_0,
                    typeof(SaveFramework.Sample.PlayerStats),
                    "Health",
                    typeof(int),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((SaveFramework.Sample.PlayerStats)obj).Health,
                    (obj, val) => ((SaveFramework.Sample.PlayerStats)obj).Health = (int)val
                );

                // 字段: Mana
                var aliases_Mana_1 = new string[0];
                entries["mana"] = new SaveEntry(
                    "mana",
                    aliases_Mana_1,
                    typeof(SaveFramework.Sample.PlayerStats),
                    "Mana",
                    typeof(int),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((SaveFramework.Sample.PlayerStats)obj).Mana,
                    (obj, val) => ((SaveFramework.Sample.PlayerStats)obj).Mana = (int)val
                );

                // 字段: Level
                var aliases_Level_2 = new string[0];
                entries["level"] = new SaveEntry(
                    "level",
                    aliases_Level_2,
                    typeof(SaveFramework.Sample.PlayerStats),
                    "Level",
                    typeof(int),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((SaveFramework.Sample.PlayerStats)obj).Level,
                    (obj, val) => ((SaveFramework.Sample.PlayerStats)obj).Level = (int)val
                );

                // 字段: Experience
                var aliases_Experience_3 = new string[0];
                entries["exp"] = new SaveEntry(
                    "exp",
                    aliases_Experience_3,
                    typeof(SaveFramework.Sample.PlayerStats),
                    "Experience",
                    typeof(float),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((SaveFramework.Sample.PlayerStats)obj).Experience,
                    (obj, val) => ((SaveFramework.Sample.PlayerStats)obj).Experience = (float)val
                );

                // 字段: Position
                var aliases_Position_4 = new string[0];
                entries["pos"] = new SaveEntry(
                    "pos",
                    aliases_Position_4,
                    typeof(SaveFramework.Sample.PlayerStats),
                    "Position",
                    typeof(UnityEngine.Vector3),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((SaveFramework.Sample.PlayerStats)obj).Position,
                    (obj, val) => ((SaveFramework.Sample.PlayerStats)obj).Position = (UnityEngine.Vector3)val
                );

                // 字段: Rotation
                var aliases_Rotation_5 = new string[0];
                entries["rot"] = new SaveEntry(
                    "rot",
                    aliases_Rotation_5,
                    typeof(SaveFramework.Sample.PlayerStats),
                    "Rotation",
                    typeof(UnityEngine.Quaternion),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((SaveFramework.Sample.PlayerStats)obj).Rotation,
                    (obj, val) => ((SaveFramework.Sample.PlayerStats)obj).Rotation = (UnityEngine.Quaternion)val
                );

                // 字段: Inventory
                var aliases_Inventory_6 = new string[0];
                entries["Inventory"] = new SaveEntry(
                    "Inventory",
                    aliases_Inventory_6,
                    typeof(SaveFramework.Sample.PlayerStats),
                    "Inventory",
                    typeof(System.Collections.Generic.List<int>),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((SaveFramework.Sample.PlayerStats)obj).Inventory,
                    (obj, val) => ((SaveFramework.Sample.PlayerStats)obj).Inventory = (System.Collections.Generic.List<int>)val
                );

                // 字段: Currency
                var aliases_Currency_7 = new string[0];
                entries["coins"] = new SaveEntry(
                    "coins",
                    aliases_Currency_7,
                    typeof(SaveFramework.Sample.PlayerStats),
                    "Currency",
                    typeof(int),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((SaveFramework.Sample.PlayerStats)obj).Currency,
                    (obj, val) => ((SaveFramework.Sample.PlayerStats)obj).Currency = (int)val
                );

                // 字段: MasterVolume
                var aliases_MasterVolume_8 = new string[0];
                entries["volume"] = new SaveEntry(
                    "volume",
                    aliases_MasterVolume_8,
                    typeof(SaveFramework.Sample.PlayerStats),
                    "MasterVolume",
                    typeof(float),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((SaveFramework.Sample.PlayerStats)obj).MasterVolume,
                    (obj, val) => ((SaveFramework.Sample.PlayerStats)obj).MasterVolume = (float)val
                );

                // 字段: Difficulty
                var aliases_Difficulty_9 = new string[0];
                entries["difficulty"] = new SaveEntry(
                    "difficulty",
                    aliases_Difficulty_9,
                    typeof(SaveFramework.Sample.PlayerStats),
                    "Difficulty",
                    typeof(SaveFramework.Sample.GameDifficulty),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((SaveFramework.Sample.PlayerStats)obj).Difficulty,
                    (obj, val) => ((SaveFramework.Sample.PlayerStats)obj).Difficulty = (SaveFramework.Sample.GameDifficulty)val
                );

                // 字段: FavoriteColors
                var aliases_FavoriteColors_10 = new string[0];
                entries["colors"] = new SaveEntry(
                    "colors",
                    aliases_FavoriteColors_10,
                    typeof(SaveFramework.Sample.PlayerStats),
                    "FavoriteColors",
                    typeof(UnityEngine.Color[]),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((SaveFramework.Sample.PlayerStats)obj).FavoriteColors,
                    (obj, val) => ((SaveFramework.Sample.PlayerStats)obj).FavoriteColors = (UnityEngine.Color[])val
                );

                // 字段: Waypoints
                var aliases_Waypoints_11 = new string[0];
                entries["waypoints"] = new SaveEntry(
                    "waypoints",
                    aliases_Waypoints_11,
                    typeof(SaveFramework.Sample.PlayerStats),
                    "Waypoints",
                    typeof(UnityEngine.Vector3[]),
                    null, // 直接访问不需要 FieldInfo
                    obj => ((SaveFramework.Sample.PlayerStats)obj).Waypoints,
                    (obj, val) => ((SaveFramework.Sample.PlayerStats)obj).Waypoints = (UnityEngine.Vector3[])val
                );

                return;
            }

        }
    }

    public static class SaveFrameworkAutoInit
    {
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            SaveRegistryManager.RegisterRegistry(new SaveFrameworkRegistry());
        }
    }
}
