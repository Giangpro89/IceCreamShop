namespace IceCreamShopTests;

using FluentAssertions;
using IceCreamShop;
using Moq;
using Xunit;

public class IceCreamShopTests
{
    [Fact]
    public void Non_vegan_order_is_fulfilled()
    {
        // arrange
        var iceCreamStockMock = new Mock<IIceCreamStock>();
        iceCreamStockMock.Setup(s => s.HasFlavorAvailable(It.IsAny<Flavor>())).Returns(true);
        iceCreamStockMock.Setup(s => s.GrabCone(It.IsAny<ConeType>(), It.IsAny<PortionSize>())).Returns((ConeType coneType, PortionSize portionSize) => new Cone(coneType, portionSize));

        var billingSystemMock = new Mock<IBillingSystem>();
        var shop = new IceCreamShop(iceCreamStockMock.Object, billingSystemMock.Object);

        // act
        var order = new Order(false, ConeType.Cup, PortionSize.Large, new[] { Flavor.Strawberry, Flavor.Vanilla, Flavor.Chocolate });
        var dish = shop.Submit(order);

        // assert
        dish.IsVegan.Should().BeFalse();
        dish.Cone.Flavors.Should().HaveCount(3);
    }

    [Fact]
    public void Vegan_order_is_fullfilled()
    {
        // arrange
        var iceCreamStockMock = new Mock<IIceCreamStock>();
        iceCreamStockMock.Setup(s => s.HasFlavorAvailable(It.IsAny<Flavor>())).Returns(true);
        iceCreamStockMock.Setup(s => s.GrabCone(It.IsAny<ConeType>(), It.IsAny<PortionSize>())).Returns((ConeType coneType, PortionSize portionSize) => new Cone(coneType, portionSize));

        var billingSystemMock = new Mock<IBillingSystem>();
        var shop = new IceCreamShop(iceCreamStockMock.Object, billingSystemMock.Object);

        // act
        var order = new Order(true, ConeType.Cup, PortionSize.Large, new[] { Flavor.Strawberry, Flavor.Vanilla });
        var dish = shop.Submit(order);

        // assert
        dish.IsVegan.Should().BeTrue();
        dish.Cone.ConeType.Should().Be(ConeType.Cup);
        dish.Cone.Flavors.Should().HaveCount(2);
    }

    [Fact]
    public void Vegan_order_is_rejected_when_dairy_flavor_is_included()
    {
        // arrange
        var iceCreamStockMock = new Mock<IIceCreamStock>();
        iceCreamStockMock.Setup(s => s.HasFlavorAvailable(It.IsAny<Flavor>())).Returns(true);
        iceCreamStockMock.Setup(s => s.GrabCone(It.IsAny<ConeType>(), It.IsAny<PortionSize>())).Returns((ConeType coneType, PortionSize portionSize) => new Cone(coneType, portionSize));

        var billingSystemMock = new Mock<IBillingSystem>();
        var shop = new IceCreamShop(iceCreamStockMock.Object, billingSystemMock.Object);

        // act
        var order = new Order(true, ConeType.Cup, PortionSize.Large, new[] { Flavor.Strawberry, Flavor.Vanilla, Flavor.Chocolate });
        
        // assert
        var placeOrder = () => shop.Submit(order);
        placeOrder.Should().Throw<VeganMismatchException>();
    }


    [Fact]
    public void Large_order_on_small_cup_spills()
    {
        // arrange
        var iceCreamStockMock = new Mock<IIceCreamStock>();
        iceCreamStockMock.Setup(s => s.HasFlavorAvailable(It.IsAny<Flavor>())).Returns(true);
        iceCreamStockMock.Setup(s => s.GrabCone(It.IsAny<ConeType>(), It.IsAny<PortionSize>())).Returns((ConeType coneType, PortionSize portionSize) => new Cone(coneType, portionSize));

        var billingSystemMock = new Mock<IBillingSystem>();
        var shop = new IceCreamShop(iceCreamStockMock.Object, billingSystemMock.Object);

        // act
        var order = new Order(false, ConeType.Cup, PortionSize.Medium, new[] { Flavor.Vanilla, Flavor.Strawberry, Flavor.Chocolate });

        // assert
        var placeOrder = () => shop.Submit(order);
        placeOrder.Should().Throw<PortionSizeException>();
    }

    [Fact]
    public void Small_order_fulfilled_when_stock_is_limited()
    {
        // arrange
        var iceCreamStockMock = new Mock<IIceCreamStock>();
        int currentStock = 2;
        iceCreamStockMock.Setup(s => s.HasFlavorAvailable(It.IsAny<Flavor>()))
            .Returns<Flavor>(
                flavor =>
                {
                    currentStock--;
                    return currentStock >= 0;
                });
        iceCreamStockMock.Setup(s => s.GrabCone(It.IsAny<ConeType>(), It.IsAny<PortionSize>())).Returns((ConeType coneType, PortionSize portionSize) => new Cone(coneType, portionSize));

        var billingSystemMock = new Mock<IBillingSystem>();
        var shop = new IceCreamShop(iceCreamStockMock.Object, billingSystemMock.Object);

        // act
        var order = new Order(false, ConeType.Biscuit, PortionSize.Large, new[] { Flavor.Vanilla, Flavor.Strawberry });
        var dish = shop.Submit(order);

        // assert
        dish.Cone.ConeType.Should().Be(ConeType.Biscuit);
        dish.Cone.Flavors.Should().HaveCount(2);
    }

    [Fact]
    public void Order_is_rejected_when_out_of_stock()
    {
        // arrange
        var iceCreamStockMock = new Mock<IIceCreamStock>();
        int currentStock = 2;
        iceCreamStockMock.Setup(s => s.HasFlavorAvailable(It.IsAny<Flavor>()))
            .Returns<Flavor>(
                flavor =>
                {
                    currentStock--;
                    return currentStock >= 0;
                });
        iceCreamStockMock.Setup(s => s.GrabCone(It.IsAny<ConeType>(), It.IsAny<PortionSize>())).Returns((ConeType coneType, PortionSize portionSize) => new Cone(coneType, portionSize));

        var billingSystemMock = new Mock<IBillingSystem>();
        var shop = new IceCreamShop(iceCreamStockMock.Object, billingSystemMock.Object);

        // act
        var order = new Order(false, ConeType.Cup, PortionSize.Large, new[] { Flavor.Vanilla, Flavor.Strawberry, Flavor.Chocolate });
        var placeOrder = () => shop.Submit(order);

        // assert
        placeOrder.Should().Throw<OutOfStockException>();
    }

    [Fact]
    public void Flavor_cannot_be_added_after_topping()
    {
        // arrange
        var iceCreamStockMock = new Mock<IIceCreamStock>();
        iceCreamStockMock.Setup(s => s.HasFlavorAvailable(It.IsAny<Flavor>())).Returns(true);
        iceCreamStockMock.Setup(s => s.GrabCone(It.IsAny<ConeType>(), It.IsAny<PortionSize>())).Returns((ConeType coneType, PortionSize portionSize) => new Cone(coneType, portionSize));

        var billingSystemMock = new Mock<IBillingSystem>();
        var shop = new IceCreamShop(iceCreamStockMock.Object, billingSystemMock.Object);

        // act
        var order = new Order(false, ConeType.Biscuit, PortionSize.Medium, new[] { Flavor.Vanilla }, new Topping[] { Topping.GummyBears });
        var dish = shop.Submit(order);
        var putMore = () => dish.AddFlavor(Flavor.Chocolate);

        // assert
        putMore.Should().Throw<FlavorAfterToppingException>();
    }
}