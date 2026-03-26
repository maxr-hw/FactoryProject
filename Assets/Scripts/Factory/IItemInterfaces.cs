using UnityEngine;
using Factory.Core;

namespace Factory.Factory
{
    public interface IItemReceiver
    {
        bool CanReceive(ItemDefinition item);
        void ReceiveItem(ConveyorItem item);
    }

    public interface IItemProvider
    {
        ItemDefinition PeekItem();
        ItemDefinition ExtractItem();
        bool HasItem();
    }

    public enum Direction
    {
        North,
        East,
        South,
        West
    }

    public interface IFactoryConnection
    {
        Vector2Int GridPosition { get; }
        Direction OutputDirection { get; }
        Direction InputDirection { get; }
    }
}
