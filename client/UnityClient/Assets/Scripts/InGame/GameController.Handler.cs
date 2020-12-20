﻿using FlatBuffers.Protocol;
using NetworkShared;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public partial class GameController : MonoBehaviour
{
    [FlatBufferEvent]
    public bool OnMove(Move x)
    {
        Debug.Log($"OnMove() {x.Position.Value.X} {x.Position.Value.Y} {x.Direction} {x.Now}");
        return true;
    }

    [FlatBufferEvent]
    public bool OnStop(Stop x)
    {
        Debug.Log($"OnStop() {x.Position.Value.X} {x.Position.Value.Y} {x.Now}");
        return true;
    }


    [FlatBufferEvent]
    public bool OnMoveStatus(MoveStatus x)
    {
        Debug.Log($"OnMoveStatus() {x.Sequence} {(Direction)x.Direction} {x.Position.Value.X} {x.Position.Value.Y}");
        var character = GetCharacter(x.Sequence);
        if (character != null)
        {
            if (x.Moving)
            {
                character.MoveDirection((Direction)x.Direction, false);
            }
            else
            {
                character.StopMove(false);
            }
        }
        return true;
    }

    [FlatBufferEvent]
    public bool OnDialog(ShowDialog response)
    {
        Debug.Log($"Enabled next button : {response.Next}");
        Debug.Log($"Enabled quit button : {response.Quit}");
        Debug.Log($"Message : {response.Message}");

        return true;
    }

    [FlatBufferEvent]
    public bool OnSelectListDialog(ShowListDialog response)
    {
        Debug.Log($"Message : {response.Message}");
        for (int i = 0; i < response.ListLength; i++)
        {
            var item = response.List(i);
            Debug.Log($"item {i} : {item}");
        }
        return true;
    }

    [FlatBufferEvent]
    public bool OnEnter(Enter response)
    {
        for (int i = 0; i < response.PortalsLength; i++)
        {
            var portal = response.Portals(i).Value;
            Debug.Log($"Portal to {portal.Map} : {portal.Position?.X}, {portal.Position?.Y}");
        }

        for (int i = 0; i < response.ObjectsLength; i++)
        {
            var obj = response.Objects(i).Value;
            Debug.Log($"Object {i} : {obj.Name}({obj.Sequence}) => {(ObjectType)obj.Type}");

            CreateCharacter(obj.Name, obj.Sequence, (ObjectType)obj.Type, obj.Position.Value);
        }

        Debug.Log($"My sequence : {response.Sequence}");
        SetMyCharacter(response.Sequence);

        Debug.Log($"After position : {response.Position.Value.X}, {response.Position.Value.Y}");
        Debug.Log($"After map name : {response.Map?.Name}");

        LoadMap(response.Map.Value.Name);

        return true;
    }

    [FlatBufferEvent]
    public bool OnShow(Show response)
    {
        Debug.Log($"{response.Name}({response.Sequence}) is entered in current map.");
        CreateCharacter(response.Name, response.Sequence, ObjectType.Character, response.Position.Value);
        return true;
    }


    [FlatBufferEvent]
    public bool OnShowCharacter(ShowCharacter response)
    {
        Debug.Log($"{response.Name}({response.Sequence}) is entered in current map.");
        CreateCharacter(response.Name, response.Sequence, ObjectType.Character, response.Position.Value);
        return true;
    }

    [FlatBufferEvent]
    public bool OnLeave(Leave response)
    {
        Debug.Log($"{response.Sequence} is leave from current map.");
        RemoveCharacter(response.Sequence);
        return true;
    }
}