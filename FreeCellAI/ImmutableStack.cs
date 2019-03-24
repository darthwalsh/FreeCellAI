using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FreeCellAI
{
  public class ImmutableStack<T> : IEnumerable<T>, IEquatable<ImmutableStack<T>>
  {
    ImmutableStack<T> tail;
    T head;
    int hashCode;

    ImmutableStack(ImmutableStack<T> tail, T head) {
      this.tail = tail;
      this.head = head;
      hashCode = HashCode.Combine(tail, head);
    }

    public virtual bool IsEmpty => false;
    public virtual T Head => head;
    public ImmutableStack<T> Push(T t) => new ImmutableStack<T>(this, t);
    public virtual ImmutableStack<T> Pop() => tail;

    public static ImmutableStack<T> New(IEnumerable<T> elems) {
      var stack = Empty;
      foreach (var e in elems) {
        stack = stack.Push(e);
      }
      return stack;
    }

    public override bool Equals(object obj) => Equals(obj as ImmutableStack<T>);
    public bool Equals(ImmutableStack<T> other) => other != null && 
      ((IsEmpty || other.IsEmpty) ? 
        (IsEmpty == other.IsEmpty) :
        EqualityComparer<T>.Default.Equals(Head, other.Head)) &&
      EqualityComparer<ImmutableStack<T>>.Default.Equals(tail, other.tail);
    public override int GetHashCode() => hashCode;
    public IEnumerator<T> GetEnumerator() {
      for(var t = this; !t.IsEmpty; t = t.Pop()) {
        yield return t.Head;
      }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static readonly ImmutableStack<T> Empty = new EmptyStack();

    class EmptyStack : ImmutableStack<T>
    {
      public EmptyStack() : base(null, default(T)) { }
      public override bool IsEmpty => true;
      public override T Head => throw new InvalidOperationException("Empty stack");
      public override ImmutableStack<T> Pop() => throw new InvalidOperationException("Empty stack");
    }
  }
}
