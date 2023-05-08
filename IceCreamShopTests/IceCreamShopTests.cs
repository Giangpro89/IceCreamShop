namespace IceCreamShopTests;

using FluentAssertions;
using IceCreamShop;
using Moq;
using Xunit;

public class IceCreamShopTests
{
    private static readonly IBillingSystem _billingSystemMock = new Mock<IBillingSystem>().Object;
    private static readonly IIceCreamStock _iceCreamStock;

    static IceCreamShopTests()
    {
        _iceCreamStock = GetIceCreamMock().Object;
    }

    [Fact]
    public void Non_vegan_order_is_fulfilled()
    {
        var order = new IceCreamOrderBuilder()
            .WithCone(ConeType.Cup, PortionSize.Large)
            .AddFlavor(Flavor.Strawberry)
            .AddFlavor(Flavor.Vanilla)
            .AddFlavor(Flavor.Chocolate)
            .CreateOrder();
        var dish = GetIceCreamShop().Submit(order);
        dish.Cone.Flavors.Should().HaveCount(3);
    }

    [Fact]
    public void Vegan_order_is_fullfilled()
    {
        var order = new IceCreamOrderBuilder()
            .VeganOnly()
            .WithCone(ConeType.Cup, PortionSize.Large)
            .AddFlavor(Flavor.Strawberry)
            .AddFlavor(Flavor.Vanilla)
            .CreateOrder();
        var dish = GetIceCreamShop().Submit(order);
        dish.Cone.Flavors.Should().HaveCount(2);
    }

    [Fact]
    public void Vegan_order_is_rejected_when_dairy_flavor_is_included()
    {
        var order = new IceCreamOrderBuilder()
            .VeganOnly()
            .WithCone(ConeType.Cup, PortionSize.Large)
            .AddFlavor(Flavor.Strawberry)
            .AddFlavor(Flavor.Vanilla)
            .AddFlavor(Flavor.Chocolate)
            .CreateOrder();
        var placeOrder = () => GetIceCreamShop().Submit(order);
        placeOrder.Should().Throw<VeganMismatchException>();
    }


    [Fact]
    public void Large_order_on_small_cup_spills()
    {
        var order = new IceCreamOrderBuilder()
            .WithCone(ConeType.Cup, PortionSize.Medium)
            .AddFlavor(Flavor.Vanilla)
            .AddFlavor(Flavor.Strawberry)
            .AddFlavor(Flavor.Chocolate)
            .CreateOrder();
        var placeOrder = () => GetIceCreamShop().Submit(order);
        placeOrder.Should().Throw<PortionSizeException>();
    }

    [Fact]
    public void Small_order_fulfilled_when_stock_is_limited()
    {
        var order = new IceCreamOrderBuilder()
            .WithCone(ConeType.Cup, PortionSize.Large)
            .AddFlavor(Flavor.Vanilla)
            .AddFlavor(Flavor.Strawberry)
            .CreateOrder();
        var dish = GetIceCreamShopWithLimitedFlavorsStock(maxFlavors: 2).Submit(order);
        dish.Cone.Flavors.Should().HaveCount(2);
    }

    [Fact]
    public void Order_is_rejected_when_out_of_stock()
    {
        var order = new IceCreamOrderBuilder()
            .WithCone(ConeType.Cup, PortionSize.Large)
            .AddFlavor(Flavor.Vanilla)
            .AddFlavor(Flavor.Strawberry)
            .AddFlavor(Flavor.Chocolate)
            .CreateOrder();
        var placeOrder = () => GetIceCreamShopWithLimitedFlavorsStock(maxFlavors: 2).Submit(order);
        placeOrder.Should().Throw<OutOfStockException>();
    }


    [Fact]
    public void Flavor_cannot_be_added_after_topping()
    {
        var order = new IceCreamOrderBuilder()
            .WithCone(ConeType.Biscuit, PortionSize.Medium)
            .AddFlavor(Flavor.Vanilla)
            .AddTopping(Topping.GummyBears)
            .CreateOrder();
        var dish = GetIceCreamShop().Submit(order);
        var putMore = () => dish.AddFlavor(Flavor.Chocolate);
        putMore.Should().Throw<FlavorAfterToppingException>();
    }


    private IceCreamShop GetIceCreamShop(IIceCreamStock? iceCreamStock = null)
    {
        return new IceCreamShop(iceCreamStock ?? _iceCreamStock, _billingSystemMock);
    }

    private IceCreamShop GetIceCreamShopWithLimitedFlavorsStock(int maxFlavors)
    {
        var stockMock = GetIceCreamMock();
        int currentStock = maxFlavors;
        stockMock.Setup(s => s.HasFlavorAvailable(It.IsAny<Flavor>()))
            .Returns<Flavor>(
                flavor =>
                {
                    currentStock--;
                    return currentStock >= 0;
                });

        return GetIceCreamShop(stockMock.Object);
    }

    private static Mock<IIceCreamStock> GetIceCreamMock()
    {
        var iceCreamStockMock = new Mock<IIceCreamStock>();
        iceCreamStockMock.Setup(s => s.HasFlavorAvailable(It.IsAny<Flavor>())).Returns(true);
        iceCreamStockMock.Setup(s => s.GrabCone(It.IsAny<ConeType>(), It.IsAny<PortionSize>())).Returns((ConeType coneType, PortionSize portionSize) => new Cone(coneType, portionSize));
        return iceCreamStockMock;
    }
}

public interface IIceCreamBasicOrderBuilder
{
    IIceCreamBasicOrderBuilder VeganOnly();

    IIceCreamOrderWithConeBuilder WithCone(ConeType coneType, PortionSize size);
}

public interface IIceCreamOrderWithConeBuilder
{
    IIceCreamOrderWithConeBuilder AddFlavor(Flavor flavor);

    IIceCreamOrderWithConeBuilder AddTopping(Topping topping);

    Order CreateOrder();
}

public class IceCreamOrderBuilder : IIceCreamBasicOrderBuilder
{
    private bool _isVegan;

    public IIceCreamBasicOrderBuilder VeganOnly()
    {
        _isVegan = true;
        return this;
    }

    public IIceCreamOrderWithConeBuilder WithCone(ConeType coneType, PortionSize size)
    {
        return new IceCreamOrderWithConeBuilder(coneType, size, _isVegan);
    }

    private class IceCreamOrderWithConeBuilder : IceCreamOrderBuilder, IIceCreamOrderWithConeBuilder
    {
        private readonly ConeType _coneType;
        private readonly PortionSize _size;
        private readonly List<Flavor> _flavors = new();
        private readonly List<Topping> _toppings = new();

        public IceCreamOrderWithConeBuilder(ConeType coneType, PortionSize size, bool isVegan)
        {
            _coneType = coneType;
            _size = size;
            _isVegan = isVegan;
        }

        public IIceCreamOrderWithConeBuilder AddFlavor(Flavor flavor)
        {
            _flavors.Add(flavor);
            return this;
        }

        public IIceCreamOrderWithConeBuilder AddTopping(Topping topping)
        {
            _toppings.Add(topping);
            return this;
        }

        public Order CreateOrder()
        {
            return new Order(_isVegan, _coneType, _size, _flavors.ToArray(), _toppings.ToArray());
        }
    }
}
