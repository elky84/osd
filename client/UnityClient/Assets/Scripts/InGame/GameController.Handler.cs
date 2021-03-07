using Assets.Scripts.InGame.OOP;
using FlatBuffers.Protocol.Response;
using MasterData;
using NetworkShared;
using UnityEngine;

public partial class GameController : MonoBehaviour
{
    [FlatBufferEvent]
    public bool OnState(State response)
    {
        var obj = GetObject(response.Sequence);
        if (obj == null)
            return false;

        obj.transform.localPosition = new Position((float)response.Position.Value.X, (float)response.Position.Value.Y).ToVector3();
        if (obj is Life)
        {
            var life = obj as Life;
            if (response.Jump)
                life.Jump();

            if (response.Move)
                life.Move((Direction)response.Direction);
            else
                life.Stop();
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
        var portals = PortalGroup.GetComponentsInChildren<Assets.Scripts.InGame.Portal>();
        foreach (var portal in portals)
            Destroy(portal.gameObject);

        for (int i = 0; i < response.PortalsLength; i++)
        {
            var portal = response.Portals(i).Value;
            Assets.Scripts.InGame.Portal.Create(portal.Position.Value.ToVector2(), PortalGroup);
        }

        CreateObject(response.Character.Value.Name, response.Character.Value.Sequence, ObjectType.Character, response.Character.Value.Position.Value);

        Debug.Log($"My sequence : {response.Character.Value.Sequence}");
        SetMyCharacter(response.Character.Value.Sequence);

        Debug.Log($"After position : {response.Position.Value.X}, {response.Position.Value.Y}");
        Debug.Log($"After map name : {response.Map?.Name}");

        //var window = UIPool.Show<UIWindow>();
        //window.Contents = "안녕하세요??";

        return true;
    }

    [FlatBufferEvent]
    public bool OnShow(Show response)
    {
        for (int i = 0; i < response.ObjectsLength; i++)
        {
            var obj = response.Objects(i).Value;
            Debug.Log($"show : {obj.Sequence}({obj.Name})");

            var created = CreateObject(obj.Name, obj.Sequence, (ObjectType)obj.Type, obj.Position.Value);
            if (created is Life)
            {
                var createdLife = created as Life;
                if (obj.Moving)
                    createdLife.Move((Direction)obj.Direction);
                else
                    createdLife.Stop();
            }
        }

        for (int i = 0; i < response.CharactersLength; i++)
        {
            var character = response.Characters(i).Value;
            Debug.Log($"show : {character.Sequence}({character.Name})");

            var created = CreateObject(character.Name, character.Sequence, ObjectType.Character, character.Position.Value) as Assets.Scripts.InGame.OOP.Character;
            if (created.Moving)
                created.Move((Direction)character.Direction);
            else
                created.Stop();
        }

        return true;
    }

    [FlatBufferEvent]
    public bool OnLeave(Leave response)
    {
        for (int i = 0; i < response.SequenceLength; i++)
        {
            Debug.Log($"hide : {response.Sequence(i)}");
            RemoveObject(response.Sequence(i));
        }
        return true;
    }

    [FlatBufferEvent]
    public bool OnSetOwner(SetOwner response)
    {
        for (int i = 0; i < response.SequencesLength; i++)
        {
            var sequence = response.Sequences(i);
            if (Objects.TryGetValue(sequence, out var obj) == false)
                continue;

            UnityEngine.Debug.Log($"set owner : {sequence}", obj.gameObject);
            SetControllable(obj);
        }

        return true;
    }

    [FlatBufferEvent]
    public bool OnUnsetOwner(UnsetOwner response)
    {
        for (int i = 0; i < response.SequencesLength; i++)
        {
            var sequence = response.Sequences(i);
            if (Objects.TryGetValue(sequence, out var obj) == false)
                continue;

            UnityEngine.Debug.Log($"unset owner : {sequence}", obj.gameObject);
            UnsetControllable(obj);
        }

        return true;
    }

    [FlatBufferEvent]
    public bool OnDamaged(Damaged response)
    {
        UnityEngine.Debug.Log($"damaged ({response.Sequence} > {response.Damage})");
        return true;
    }

    [FlatBufferEvent]
    public bool OnHealed(Healed response)
    {
        UnityEngine.Debug.Log($"healed ({response.Sequence} > {response.Heal})");
        return true;
    }

    [FlatBufferEvent]
    public bool OnDie(Die response)
    {
        if (Objects.TryGetValue(response.Sequence, out var obj) == false)
            return true;

        if (obj.Type == ObjectType.Mob)
        {
            RemoveObject(obj.Sequence);
            UnityEngine.Debug.Log($"{obj.Sequence} is dead");
        }

        return true;
    }

    [FlatBufferEvent]
    public bool OnAttack(Attack response)
    {
        if (Objects.TryGetValue(response.Sequence, out var obj) == false)
            return false;

        UnityEngine.Debug.Log($"attack action : {obj.Sequence}");
        return true;
    }

    [FlatBufferEvent]
    public bool OnItems(Items response)
    {
        for (int i = 0; i < response.InventoryLength; i++)
        {
            var item = response.Inventory(i);
            UnityEngine.Debug.Log($"item {i + 1} : {item.Value.Name}");
        }

        for (int i = 0; i < response.EquipmentLength; i++)
        {
            var equipment = response.Equipment(i);
            UnityEngine.Debug.Log($"equipment {(EquipmentType)equipment.Value.Type} : {equipment.Value.Name}");
        }

        UnityEngine.Debug.Log($"gold : {response.Gold}");

        return true;
    }

    [FlatBufferEvent]
    public bool OnVisualUpdate(FlatBuffers.Protocol.Response.Character response)
    {
        var obj = GetObject(response.Sequence);
        if (obj == null)
            return true;

        var character = obj as Assets.Scripts.InGame.OOP.Character;
        if (character == null)
            return true;

        for (int i = 0; i < response.EquipmentLength; i++)
        {
            var equipment = response.Equipment(i);
            var equipmentType = (EquipmentType)equipment.Value.Type;
            var equipmentName = equipment.Value.Name;

            UnityEngine.Debug.Log($"character({character.Sequence}) visual updated");
        }

        return true;
    }

    [FlatBufferEvent]
    public bool OnLevelUp(LevelUp response)
    {
        var obj = GetObject(response.Sequence);
        if (obj == null)
            return true;

        var character = obj as Assets.Scripts.InGame.OOP.Character;
        if (character == null)
            return true;

        UnityEngine.Debug.Log($"level up : {character.Sequence}({response.Level})");

        if (MyCharacter == character)
            MyCharacter.Level = response.Level;

        return true;
    }

    [FlatBufferEvent]
    public bool OnExperienceChanged(ExpChange response)
    {
        if (MyCharacter != null)
            MyCharacter.Exp = response.Exp;

        UnityEngine.Debug.Log($"experience : {response.Exp}");
        return true;
    }

    [FlatBufferEvent]
    public bool OnAcquireSkill(AcquireSkill response)
    {
        var id = response.Id;
        return true;
    }

    [FlatBufferEvent]
    public bool OnSkillLevelUp(SkillLevelUp response)
    {
        return true;
    }
}
