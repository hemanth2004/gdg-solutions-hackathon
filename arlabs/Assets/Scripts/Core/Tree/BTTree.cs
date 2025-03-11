using UnityEngine;

public class BTTree<T>
{
    public BTNode<T> Root { get; private set; }

    public BTTree(T rootData)
    {
        Root = new BTNode<T>(rootData);
    }

    public void Clear()
    {
        Root = null;
    }

    public bool IsEmpty()
    {
        return Root == null;
    }
}
