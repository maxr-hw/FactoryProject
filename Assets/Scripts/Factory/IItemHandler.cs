using Factory.Core;

namespace Factory.Factory
{
    public interface IItemHandler
    {
        bool CanAcceptItem(ItemDefinition type);
        void ReceiveItem(ConveyorItem item);
    }
}
