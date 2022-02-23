using NUnit.Framework;
using Fab.Geo.UI;
using UnityEngine.UIElements;

public class VisualElementHierachyBuilderTests
{
    private VisualElement CreateElement(string name)
    {
        return new VisualElement();
    }

    [Test]
    [TestCase("a.b.name", ExpectedResult = "name")]
    [TestCase("name", ExpectedResult = "name")]
    [TestCase(".name", ExpectedResult = "name")]
    [TestCase("a.b.", ExpectedResult = "")]
    [TestCase(null, ExpectedResult = null)]
    public string Get_name_returns_string_after_last_seperator(string path)
    {
        var sut = new VisualElementHierachyBuilder(null, null);

        return sut.GetName(path);
    }

    [Test]
    public void Add_to_hierachy_creates_hierachy()
    {
        VisualElement root = new VisualElement();
        VisualElement element = new VisualElement();
        var sut = new VisualElementHierachyBuilder(root, (name) => new VisualElement());

        sut.AddToHierachy(element, "a.b.name");

        VisualElement groupA = root[0];
        VisualElement groupB = groupA[0];
        Assert.AreEqual(groupB, element.parent);
    }

    [Test]
    public void Add_to_hierachy_adds_to_exisiting_hierachy()
    {
        VisualElement root = new VisualElement();
        VisualElement groupA = new VisualElement();
        groupA.name = "a" + VisualElementHierachyBuilder.GroupNameSuffix;
        VisualElement groupB = new VisualElement();
        groupB.name = "b" + VisualElementHierachyBuilder.GroupNameSuffix;
        groupA.Add(groupB);
        root.Add(groupA);
        VisualElement element = new VisualElement();
        var sut = new VisualElementHierachyBuilder(root, (name) => new VisualElement());

        sut.AddToHierachy(element, "a.b.name");

        Assert.AreEqual(groupB, element.parent);
    }

    [Test]
    public void Add_to_hierachy_sets_elements_name()
    {
        VisualElement root = new VisualElement();
        VisualElement element = new VisualElement();
        var sut = new VisualElementHierachyBuilder(root, (name) => new VisualElement());

        sut.AddToHierachy(element, "name");

        Assert.AreEqual("name", element.name);
    }

    [Test]
    public void Get_element_at_path_returns_element ()
    {
        VisualElement root = new VisualElement();
        VisualElement element = new VisualElement();
        var sut = new VisualElementHierachyBuilder(root, (name) => new VisualElement());
        string path = "a.b.name";
        sut.AddToHierachy(element, path);

        VisualElement found = sut.GetElementAtPath(path);

        Assert.AreEqual(element, found);
    }

    [Test]
    public void Get_nonexisting_element_at_path_returns_null()
    {
        VisualElement root = new VisualElement();
        var sut = new VisualElementHierachyBuilder(root, (name) => new VisualElement());
        string path = "a.b.name";

        VisualElement found = sut.GetElementAtPath(path);

        Assert.Null(found);
    }

    [Test]
    public void Remove_nonexisting_element_returns_false()
    {
        VisualElement root = new VisualElement();
        var sut = new VisualElementHierachyBuilder(root, (name) => new VisualElement());
        string path = "a.b.name";

        bool result = sut.RemoveElement(path);

        Assert.False(result);
    }

    [Test]
    public void Remove_element_at_path_removes_empty_groups()
    {
        VisualElement root = new VisualElement();
        VisualElement element = new VisualElement();
        var sut = new VisualElementHierachyBuilder(root, (name) => new VisualElement());
        string path = "a.b.name";
        sut.AddToHierachy(element, path);

        sut.RemoveElement(path);

        Assert.AreEqual(0, root.childCount);
    }

    [Test]
    public void Remove_all_removes_all_children_of_root()
    {
        VisualElement root = new VisualElement();
        VisualElement element = new VisualElement();
        var sut = new VisualElementHierachyBuilder(root, (name) => new VisualElement());
        string path = "a.b.name";
        sut.AddToHierachy(element, path);

        sut.RemoveAll();

        Assert.AreEqual(0, root.childCount);
    }
}
