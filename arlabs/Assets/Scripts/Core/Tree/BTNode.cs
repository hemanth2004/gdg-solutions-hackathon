using UnityEngine;
using System.Collections.Generic;

public class BTNode<T>
{
    public T Data { get; set; }
    public BTNode<T> Parent { get; private set; }
    public List<BTNode<T>> Children { get; private set; }

    public BTNode(T data)
    {
        Data = data;
        Children = new List<BTNode<T>>();
    }

    public void AddChild(BTNode<T> child)
    {
        child.Parent = this;
        Children.Add(child);
    }

    public void RemoveChild(BTNode<T> child)
    {
        if (Children.Contains(child))
        {
            child.Parent = null;
            Children.Remove(child);
        }
    }

    public bool IsLeaf()
    {
        return Children.Count == 0;
    }

    public bool IsRoot()
    {
        return Parent == null;
    }
}
