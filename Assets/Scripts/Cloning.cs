using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using UnityEngine;

static public class Cloning
{
    static public Type[] KnownTypes = new Type[] { typeof(Player), typeof(City), typeof(Unit), typeof(Map), typeof(PlayerSettings), typeof(Vector2), typeof(Vector3), typeof(Vector4), typeof(Vector2Int), typeof(List<Player>), typeof(List<Unit>), typeof(List<City>), typeof(Color), typeof(Army), typeof(Fighter), typeof(Transport), typeof(Destroyer), typeof(Submarine), typeof(Cruiser), typeof(Battleship), typeof(Carrier), typeof(TileType), typeof(UnitType) };

    static public T Clone<T>(this T obj)
    {
        DataContractSerializer dcs = new DataContractSerializer(typeof(T),KnownTypes);
        MemoryStream stream = new MemoryStream();
        dcs.WriteObject(stream, obj);
        stream.Flush();
        stream.Position = 0;
        return (T)dcs.ReadObject(stream);
    }
}