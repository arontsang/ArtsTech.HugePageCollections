using System.Collections.Generic;
using ArtsTech.HugePageCollections.Collections;

namespace ArtsTech.HugePageCollections.UnitTests.Collections;

[TestFixture]
public class HugePagesSetISetTests
{
    [Test]
    public void AddRemoveAndContainsFollowISetContract()
    {
        using var hugePagesSet = new HugePagesSet<int>();
        ISet<int> set = hugePagesSet;

        Assert.That(((ICollection<int>)set).IsReadOnly, Is.False);
        Assert.That(set.Add(10), Is.True);
        Assert.That(set.Add(10), Is.False);
        Assert.That(set.Contains(10), Is.True);
        Assert.That(set.Contains(20), Is.False);
        Assert.That(set.Count, Is.EqualTo(1));

        Assert.That(set.Remove(10), Is.True);
        Assert.That(set.Remove(10), Is.False);
        Assert.That(set, Is.Empty);

        ((ICollection<int>)set).Add(20);
        Assert.That(set, Is.EquivalentTo(new[] { 20 }));
    }

    [Test]
    public void UnionWithAddsUniqueElements()
    {
        using var hugePagesSet = new HugePagesSet<int>(new[] { 1, 2 });
        ISet<int> set = hugePagesSet;

        set.UnionWith(new[] { 2, 3, 3, 4 });

        Assert.That(set, Is.EquivalentTo(new[] { 1, 2, 3, 4 }));
        Assert.That(set.Count, Is.EqualTo(4));
    }

    [Test]
    public void IntersectWithKeepsOnlyElementsInOther()
    {
        using var hugePagesSet = new HugePagesSet<int>(new[] { 1, 2, 3, 4 });
        ISet<int> set = hugePagesSet;

        set.IntersectWith(new[] { 2, 2, 4, 6 });

        Assert.That(set, Is.EquivalentTo(new[] { 2, 4 }));
    }

    [Test]
    public void ExceptWithRemovesElementsInOther()
    {
        using var hugePagesSet = new HugePagesSet<int>(new[] { 1, 2, 3, 4 });
        ISet<int> set = hugePagesSet;

        set.ExceptWith(new[] { 2, 2, 4, 6 });

        Assert.That(set, Is.EquivalentTo(new[] { 1, 3 }));
    }

    [Test]
    public void SymmetricExceptWithKeepsElementsPresentInOnlyOneSet()
    {
        using var hugePagesSet = new HugePagesSet<int>(new[] { 1, 2, 3 });
        ISet<int> set = hugePagesSet;

        set.SymmetricExceptWith(new[] { 2, 2, 4, 4, 5 });

        Assert.That(set, Is.EquivalentTo(new[] { 1, 3, 4, 5 }));
    }

    [Test]
    public void SetRelationMethodsAccountForDuplicatesInOther()
    {
        using var hugePagesSet = new HugePagesSet<int>(new[] { 1, 2 });
        ISet<int> set = hugePagesSet;

        Assert.That(set.IsSubsetOf(new[] { 1, 2, 2, 3 }), Is.True);
        Assert.That(set.IsProperSubsetOf(new[] { 1, 2, 2, 3 }), Is.True);
        Assert.That(set.IsSupersetOf(new[] { 1, 1, 2 }), Is.True);
        Assert.That(set.IsProperSupersetOf(new[] { 1, 1 }), Is.True);
        Assert.That(set.SetEquals(new[] { 2, 1, 1 }), Is.True);
        Assert.That(set.SetEquals(new[] { 1, 3 }), Is.False);
        Assert.That(set.Overlaps(new[] { 3, 2 }), Is.True);
        Assert.That(set.Overlaps(new[] { 3, 4 }), Is.False);
    }

    [Test]
    public void SetOperationsHandleSelfAndEmptySets()
    {
        using var hugePagesSet = new HugePagesSet<int>(new[] { 1, 2, 3 });
        ISet<int> set = hugePagesSet;

        set.UnionWith(set);
        Assert.That(set, Is.EquivalentTo(new[] { 1, 2, 3 }));

        set.IntersectWith(set);
        Assert.That(set, Is.EquivalentTo(new[] { 1, 2, 3 }));

        set.SymmetricExceptWith(set);
        Assert.That(set, Is.Empty);
        Assert.That(set.IsSubsetOf(Array.Empty<int>()), Is.True);
        Assert.That(set.IsSupersetOf(Array.Empty<int>()), Is.True);
        Assert.That(set.Overlaps(new[] { 1 }), Is.False);

        set.UnionWith(new[] { 4, 5 });
        set.ExceptWith(set);
        Assert.That(set, Is.Empty);
    }

    [Test]
    public void ComparerIsAppliedAcrossISetOperations()
    {
        using var hugePagesSet = new HugePagesSet<string>(StringComparer.OrdinalIgnoreCase);
        ISet<string> set = hugePagesSet;

        Assert.That(set.Add("alpha"), Is.True);
        Assert.That(set.Add("ALPHA"), Is.False);
        set.UnionWith(new[] { "Beta", "BETA" });

        Assert.That(set.Contains("beta"), Is.True);
        Assert.That(set.SetEquals(new[] { "ALPHA", "beta", "BETA" }), Is.True);
        set.ExceptWith(new[] { "aLpHa" });
        Assert.That(set, Is.EquivalentTo(new[] { "Beta" }));
    }

    [Test]
    public void AddRemoveAndSetOperationsRemainCorrectAcrossGrowth()
    {
        using var hugePagesSet = new HugePagesSet<int>();
        ISet<int> set = hugePagesSet;

        for (int value = 0; value < 1_000; value++)
        {
            Assert.That(set.Add(value), Is.True);
        }

        for (int value = 0; value < 1_000; value += 2)
        {
            Assert.That(set.Remove(value), Is.True);
        }

        set.UnionWith(Enumerable.Range(1_000, 250));

        Assert.That(set.Count, Is.EqualTo(750));
        Assert.That(set.Contains(999), Is.True);
        Assert.That(set.Contains(1_249), Is.True);
        Assert.That(set.Contains(998), Is.False);
        Assert.That(set.IsSupersetOf(new[] { 1, 3 }), Is.True);
        Assert.That(set.IsSupersetOf(new[] { 1, 2 }), Is.False);
    }

    [Test]
    public void ClearEmptiesTheSetAndItCanBeReused()
    {
        using var hugePagesSet = new HugePagesSet<int>(new[] { 1, 2, 3 });
        ISet<int> set = hugePagesSet;

        set.Clear();
        Assert.That(set, Is.Empty);

        Assert.That(set.Add(4), Is.True);
        Assert.That(set.SetEquals(new[] { 4 }), Is.True);
    }
}
