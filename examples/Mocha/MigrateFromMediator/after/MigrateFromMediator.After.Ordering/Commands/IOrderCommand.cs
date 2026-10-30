namespace Shop.Ordering;

public interface IOrderCommand
{
    string? Validate();
}
