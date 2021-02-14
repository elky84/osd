using FlatBuffers.Protocol.Response;
using NetworkShared;
using UnityEngine;

public partial class GameController : MonoBehaviour
{
    [FlatBufferEvent]
    public bool OnState(State response)
    {
        var character = GetCharacter(response.Sequence);
        if (character == null)
            return false;

        character.CurrentPosition = new Position((float)response.Position.Value.X, (float)response.Position.Value.Y);
        character.Velocity = new UnityEngine.Vector2((float)response.Velocity.Value.X, (float)response.Velocity.Value.Y);

        if (response.Jump)
            character.Jump();

        if (response.Move)
            character.MoveDirection((Direction)response.Direction, false);
        else
            character.StopMove(false);

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

        CreateCharacter(response.Character.Value.Name, response.Character.Value.Sequence, ObjectType.Character, response.Character.Value.Position.Value);

        Debug.Log($"My sequence : {response.Character.Value.Sequence}");
        SetMyCharacter(response.Character.Value.Sequence);

        Debug.Log($"After position : {response.Position.Value.X}, {response.Position.Value.Y}");
        Debug.Log($"After map name : {response.Map?.Name}");

        return true;
    }

    [FlatBufferEvent]
    public bool OnShow(Show response)
    {
        for (int i = 0; i < response.ObjectsLength; i++)
        {
            var obj = response.Objects(i).Value;
            Debug.Log($"Object {i} : {obj.Name}({obj.Sequence}) => {(ObjectType)obj.Type}, {obj.Position.Value.X} {obj.Position.Value.Y}");

            var created = CreateCharacter(obj.Name, obj.Sequence, (ObjectType)obj.Type, obj.Position.Value);
            if(obj.Moving)
                created.MoveDirection((Direction)obj.Direction, false);
            else
                created.StopMove(false);
        }

        for (int i = 0; i < response.CharactersLength; i++)
        {
            var character = response.Characters(i).Value;
            Debug.Log($"Object {i} : {character.Name}({character.Sequence}) => {ObjectType.Character}, {character.Position.Value.X} {character.Position.Value.Y}");

            var created = CreateCharacter(character.Name, character.Sequence, ObjectType.Character, character.Position.Value);
            if (created.Moving)
                created.MoveDirection((Direction)character.Direction, false);
            else
                created.StopMove(false);
        }

        return true;
    }

    [FlatBufferEvent]
    public bool OnLeave(Leave response)
    {
        for (int i = 0; i < response.SequenceLength; i++)
        {
            Debug.Log($"{response.Sequence(i)} is leave from current map.");
            RemoveCharacter(response.Sequence(i));
        }
        return true;
    }
}
