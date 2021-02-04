﻿using FlatBuffers.Protocol.Response;
using NetworkShared;
using UnityEngine;

public partial class GameController : MonoBehaviour
{
    [FlatBufferEvent]
    public bool OnState(State response)
    {
        var character = GetCharacter(response.Sequence);
        if (character != null)
        {
            character.CurrentPosition = new Position((float)response.Position.Value.X, (float)response.Position.Value.Y);
            character.Velocity = new UnityEngine.Vector2((float)response.Velocity.Value.X, (float)response.Velocity.Value.Y);
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
    public bool OnShowListDialog(ShowListDialog response)
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
            Debug.Log($"Object {i} : {obj.Name}({obj.Sequence}) => {(ObjectType)obj.Type}, {obj.Position.Value.X} {obj.Position.Value.Y}");

            CreateCharacter(obj.Name, obj.Sequence, (ObjectType)obj.Type, obj.Position.Value);
        }

        Debug.Log($"My sequence : {response.Sequence}");
        SetMyCharacter(response.Sequence);

        Debug.Log($"After position : {response.Position.Value.X}, {response.Position.Value.Y}");
        Debug.Log($"After map name : {response.Map?.Name}");

        return true;
    }

    [FlatBufferEvent]
    public bool OnShow(Show response)
    {
        var character = GetCharacter(response.Sequence);
        if (character == null)
        {
            Debug.Log($"{response.Name}({response.Sequence}) is entered in current map.");
            character = CreateCharacter(response.Name, response.Sequence, ObjectType.Character, response.Position.Value);
        }

        if (response.Moving)
            character.MoveDirection((Direction)response.Direction, false);
        else
            character.StopMove(false);

        return true;
    }


    [FlatBufferEvent]
    public bool OnShowCharacter(ShowCharacter response)
    {
        var character = GetCharacter(response.Sequence);
        if (character == null)
        {
            Debug.Log($"{response.Name}({response.Sequence}) is entered in current map.");
            character = CreateCharacter(response.Name, response.Sequence, ObjectType.Character, response.Position.Value);
        }

        if (response.Moving)
            character.MoveDirection((Direction)response.Direction, false);
        else
            character.StopMove(false);

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
