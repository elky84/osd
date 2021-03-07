using Assets.Scripts.InGame.OOP;
using FlatBuffers.Protocol.Response;
using MasterData;
using NetworkShared;
using UnityEngine;

public partial class GameController : MonoBehaviour
{
    public void RegisterEvent()
    {
        EventAggregator.Instance.Subscribe<GameEvent.DeadEnd>(OnDeadEnd);
    }

    public void UnregisterEvent()
    {
        EventAggregator.Instance.Unsubscribe<GameEvent.DeadEnd>(OnDeadEnd);
    }

    public void OnDeadEnd(GameEvent.DeadEnd deadEnd)
    {
        RemoveObject(deadEnd.Sequence);
    }
}
