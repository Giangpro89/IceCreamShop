namespace IceCreamShopTests;

using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using IceCreamShop;

public static class AssertionExtensions
{
    public static IceCreamDishAssertions Should(this IceCreamDish? iceCreamDish)
    {
        return new IceCreamDishAssertions(iceCreamDish);
    }
}

public class IceCreamDishAssertions : ReferenceTypeAssertions<IceCreamDish?, IceCreamDishAssertions>
{
    public IceCreamDishAssertions(IceCreamDish? iceCreamDish)
        : base(iceCreamDish)
    {
    }

    protected override string Identifier => nameof(IceCreamDish);

    public AndConstraint<IceCreamDishAssertions> HaveConeType(ConeType expectedConeType, string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject != null)
            .FailWith("You can't assert a null IceCreamDish")
            .Then
            .Given(() => Subject)
            .ForCondition(dish => dish!.Cone.ConeType == expectedConeType)
            .FailWith("IceCreamDish.Cone.ConeType is expected to be '{0}', but found '{1}'", _ => expectedConeType,  _ => _!.Cone.ConeType);

        return new AndConstraint<IceCreamDishAssertions>(this);
    }

    public AndConstraint<IceCreamDishAssertions> HaveFlavorsCount(int expectedFlavorsCount, string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject != null)
            .FailWith("You can't assert a null IceCreamDish")
            .Then
            .Given(() => Subject)
            .ForCondition(dish => dish!.Cone.Flavors.Count == expectedFlavorsCount)
            .FailWith("IceCreamDish.Cone.Flavors.Count is expected to be '{0}', but found '{1}'", _ => expectedFlavorsCount, _ => _!.Cone.Flavors.Count);

        return new AndConstraint<IceCreamDishAssertions>(this);
    }

    public AndConstraint<IceCreamDishAssertions> BeVegan(string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject != null)
            .FailWith("You can't assert a null IceCreamDish")
            .Then
            .Given(() => Subject)
            .ForCondition(dish => dish!.IsVegan)
            .FailWith("IceCreamDish.IsVegan is expected to be true, but found {0}", _ => _!.IsVegan);

        return new AndConstraint<IceCreamDishAssertions>(this);
    }

    public AndConstraint<IceCreamDishAssertions> NotBeVegan(string because = "", params object[] becauseArgs)
    {
        Execute.Assertion
            .BecauseOf(because, becauseArgs)
            .ForCondition(Subject != null)
            .FailWith("You can't assert a null IceCreamDish")
            .Then
            .Given(() => Subject)
            .ForCondition(dish => !dish!.IsVegan)
            .FailWith("IceCreamDish.IsVegan is expected to be false, but found {0}", _ => _!.IsVegan);

        return new AndConstraint<IceCreamDishAssertions>(this);
    }
}