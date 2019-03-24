using System.Diagnostics;
using System.IO;
using System.Linq;
using FreeCellAI;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
  [TestClass]
  public class ImmutableStackTests
  {
    [TestMethod]
    public void SimpleTest() {
      var stack = ImmutableStack<int>.New(new[] { 1, 2, 3 });
      Assert.AreEqual(stack.Head, 3);
      stack = stack.Pop();
      stack = stack.Pop();
      Assert.AreEqual(stack.Head, 1);
      stack = stack.Pop();
      Assert.IsTrue(stack.IsEmpty);
    }

    [TestMethod]
    public void CompareTest() {
      var empty = ImmutableStack<int>.Empty;
      var one = empty.Push(1);

      Assert.IsFalse(empty.Equals(one));
      Assert.IsFalse(one.Equals(empty));
      Assert.IsFalse(one.Equals(empty));
      Assert.IsTrue(empty.Equals(empty));
      Assert.IsTrue(one.Equals(empty.Push(1)));
      Assert.AreEqual(one.GetHashCode(), empty.Push(1).GetHashCode());
    }

    [TestMethod]
    public void EnumerableTests() {
      var nums = new[] { 1, 2, 3 };
      var stack = ImmutableStack<int>.New(nums);
      CollectionAssert.AreEqual(stack.ToList(), nums.Reverse().ToList());
    }
  }
}
