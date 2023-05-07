namespace IceCreamShop;

using System;

public record Order(bool IsVegan, ConeType ConeType, PortionSize PortionSize, Flavor[] Flavors, Topping[]? Toppings = default)
{
}

public class IceCreamShop : IIceCreamShop
{
    private readonly IIceCreamStock _iceCreamStock;
    private readonly IBillingSystem _billingSystem;

    public IceCreamShop(IIceCreamStock stock, IBillingSystem billingSystem)
    {
        _iceCreamStock = stock;
        _billingSystem = billingSystem;
    }

    public IceCreamDish Submit(Order order)
    {
        var iceCreamDish = PrepareIceCreamDish(order);
        _billingSystem.Charge(order);
        return iceCreamDish;
    }

    private IceCreamDish PrepareIceCreamDish(Order order)
    {
        var cone = _iceCreamStock.GrabCone(order.ConeType, order.PortionSize);
        
        var iceCreamDish = new IceCreamDish(cone, order.IsVegan);
        foreach (var flavor in order.Flavors)
        {
            if (!_iceCreamStock.HasFlavorAvailable(flavor))
            {
                throw new OutOfStockException();
            }

            iceCreamDish.AddFlavor(flavor);
        }

        foreach (var topping in order.Toppings ?? Enumerable.Empty<Topping>())
        {
            iceCreamDish.AddTopping(topping);
        }

        return iceCreamDish;
    }
}

public interface IIceCreamShop
{
    IceCreamDish Submit(Order order);
}

public interface IIceCreamStock
{
    Cone GrabCone(ConeType coneType, PortionSize portionSize);

    bool HasFlavorAvailable(Flavor flavor);
}

public interface IBillingSystem
{
    void Charge(Order order);
}

public record IceCreamDish(Cone Cone, bool IsVegan)
{
    internal void AddFlavor(Flavor flavor)
    {
        if (IsVegan && !flavor.IsVeganFlavor())
        {
            throw new VeganMismatchException();
        }

        Cone.AddFlavor(flavor);
    }

    internal void AddTopping(Topping topping)
    {
        Cone.AddTopping(topping);
    }
}

public record Cone(ConeType ConeType, PortionSize PortionSize)
{
    private readonly int _maxFlavorsToAdd = (int)PortionSize;
    public readonly List<Topping> Toppings = new();
    public readonly List<Flavor> Flavors = new();

    public void AddFlavor(Flavor flavor)
    {
        if (Toppings.Count > 0)
        {
            throw new FlavorAfterToppingException();
        }

        Flavors.Add(flavor);

        if (Flavors.Count > _maxFlavorsToAdd)
        {
            throw new PortionSizeException();
        }
    }

    public void AddTopping(Topping topping)
    {
        Toppings.Add(topping);
    }
}

public class VeganMismatchException : Exception
{
    public VeganMismatchException() : base("Cannot add non-vegan type to vegan ice cream.")
    {
    }
}

public class FlavorAfterToppingException : Exception
{
    public FlavorAfterToppingException() : base("Cannot add flavor after topping was added.")
    {
    }
}

public class PortionSizeException : Exception
{
    public PortionSizeException() : base("No more space for another ice cream ball.")
    {
    }
}

public class OutOfStockException : Exception
{
    public OutOfStockException() : base("Out of stock.")
    {
    }
}

public static class Utils
{
    public static bool IsVeganFlavor(this Flavor flavor) => flavor != Flavor.Chocolate;
}

public enum ConeType
{
    Cup,
    Biscuit
}

public enum PortionSize
{
    Small = 1,
    Medium = 2,
    Large = 3,
}

public enum Flavor
{
    Chocolate,
    Vanilla,
    Strawberry,
}

public enum Topping
{
    Sprinkles,
    Candies,
    HotFudge,
    GummyBears,
}
