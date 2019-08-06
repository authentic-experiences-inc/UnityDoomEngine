﻿using System.Collections.Generic;
using UnityEngine;

public class Thing
{
    public float posX;
    public float posY;
    public short _posX;
    public short _posY;
    public float facing;
    public int _facing;
    public int thingType;
    public int flags;

    public Thing(short PosX, short PosY, int Facing, int ThingType, int Flags)
    {
        posX = (float)PosX / MapLoader.sizeDividor;
        posY = (float)PosY / MapLoader.sizeDividor;
        _posX = PosX;
        _posY = PosY;
        facing = ((Facing >= 180 || Facing <= 0) ? -(Facing - 180) : Facing) - 90;
        _facing = Facing;
        thingType = ThingType;
        flags = Flags;
    }
}

public class Lump
{
    public int offset;
    public int length;
    public string lumpName;

    public byte[] data;

    public Lump(int Offset, int Length, string Name)
    {
        offset = Offset;
        length = Length;
        lumpName = Name;
    }
}

public class Vertex
{
    public short _x;
    public short _y;

    public Vector2 Position;
    public List<Linedef> Linedefs = new List<Linedef>();

    public Vertex(short X, short Y)
    {
        _x = X;
        _y = Y;

        Position = new Vector2((float)X / MapLoader.sizeDividor, (float)Y / MapLoader.sizeDividor);
    }
}

public class Sector
{
    public static Dictionary<int, List<Sector>> TaggedSectors = new Dictionary<int, List<Sector>>();

    public List<Triangle> triangles = new List<Triangle>();
    
    public GameObject ceilingObject;
    public SectorController floorObject;

    public float maximumCeilingHeight;
    public float minimumCeilingHeight;
    public float maximumFloorHeight;
    public float minimumFloorHeight;

    public int _floorHeight;
    public float floorHeight;

    public int _ceilingHeight;
    public float ceilingHeight;

    public string floorTexture;
    public string ceilingTexture;

    public int _brightness;
    public float brightness;

    public int specialType;
    public int tag;

    public bool Dynamic;

    public List<Sidedef> Sidedefs = new List<Sidedef>();

    public Sector(short hfloor, short hceil, string tfloor, string tceil, int special, int Tag, int bright)
    {
        _floorHeight = hfloor;
        _ceilingHeight = hceil;
        floorHeight = (float)hfloor / MapLoader.sizeDividor;
        ceilingHeight = (float)hceil / MapLoader.sizeDividor;

        maximumCeilingHeight = ceilingHeight;
        minimumCeilingHeight = ceilingHeight;
        minimumFloorHeight = floorHeight;
        maximumFloorHeight = floorHeight;

        floorTexture = tfloor;
        ceilingTexture = tceil;

        _brightness = bright;
        brightness = (float)_brightness / 255;

        specialType = special;
        tag = Tag;

        if (tag > 0)
        {
            if (!TaggedSectors.ContainsKey(tag))
                TaggedSectors.Add(tag, new List<Sector>());

            TaggedSectors[tag].Add(this);
        }
    }

    public Vector2 RandomPoint
    {
        get
        {
            if (triangles.Count == 0)
            {
                Debug.LogError("Sector.RandomPoint: no triangles in sector!");
                return Vector2.zero;
            }

            Triangle t = triangles[Random.Range(0, triangles.Count)];

            return t.RandomPoint;
        }
    }
}

public class Linedef
{
    public const float HalfPI = Mathf.PI * .5f;
    public const float TwoPI = Mathf.PI * 2f;

    public Vertex start;
    public Vertex end;

    public Vertex Start { get { return start; } }
    public Vertex End { get { return end; } }

    public Sidedef Front;
    public Sidedef Back;

    public float Angle;
    public int flags;

    public int lineType;
    public int lineTag;

    public GameObject TopFrontObject;
    public GameObject MidFrontObject;
    public GameObject BotFrontObject;
    public GameObject TopBackObject;
    public GameObject MidBackObject;
    public GameObject BotBackObject;

    public GameObject[] GameObjects { get {
            return new GameObject[6] {
              TopFrontObject,
              MidFrontObject,
              BotFrontObject,
              TopBackObject,
              MidBackObject,
              BotBackObject }; } }

    public GameObject[] FrontObjects
    {
        get
        {
            return new GameObject[3] {
              TopFrontObject,
              MidFrontObject,
              BotFrontObject };
        }
    }

    public GameObject[] BackObjects
    {
        get
        {
            return new GameObject[3] {
              TopBackObject,
              MidBackObject,
              BotBackObject };
        }
    }

    public Linedef(Vertex Start, Vertex End, int Flags, int LineType, int Tag)
    {
        start = Start;
        end = End;

        Vector2 delta = end.Position - start.Position;
        Angle = -(float)Mathf.Atan2(-delta.y, delta.x) + HalfPI;
        if (Angle < 0f) Angle += TwoPI;

        Start.Linedefs.Add(this);
        End.Linedefs.Add(this);

        flags = Flags;
        lineType = LineType;
        lineTag = Tag;
    }
}

public class Sidedef
{
    public bool IsFront { get { return (Line != null) && (this == Line.Front); } }

    public Linedef Line;
    public Sector Sector;

    public Sidedef Other { get { return (this == Line.Front ? Line.Back : Line.Front); } }

    public short offsetX;
    public short offsetY;
    public string tHigh;
    public string tLow;
    public string tMid;

    public int index;

    public Sidedef(Sector sector, short OffsetX, short OffsetY, string THigh, string TLow, string TMid, int Index)
    {
        Sector = sector;
        offsetX = OffsetX;
        offsetY = OffsetY;
        tHigh = THigh;
        tLow = TLow;
        tMid = TMid;
        index = Index;

        Sector.Sidedefs.Add(this);
    }

    public void SetLine(Linedef line, bool front)
    {
        Line = line;

        if (front)
            Line.Front = this;
        else
            Line.Back = this;
    }
}
