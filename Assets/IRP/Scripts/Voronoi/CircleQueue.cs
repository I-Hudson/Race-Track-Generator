using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleQueue
{ 
    /// <summary>
    /// Event struct 
    /// </summary>
    public struct Event
    {
        public double yLock;
        public DVector2 vertex;
        public BeachLine.Arc arc;
    }

    /// <summary>
    /// Node struct
    /// </summary>
    public class Node
    {
        public Node parent;
        public Node left;
        public Node right;
        public Event cEvent;
    }

    Node root;
    Node top;
    Stack<Node> recycledNodes;

    /// <summary>
    /// Constructor
    /// </summary>
    public CircleQueue()
    {
        recycledNodes = new Stack<Node>();
    }

    /// <summary>
    /// Check if empty. If empty return true
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty()
    {
        return root == null;
    }

    /// <summary>
    /// Push new event to circle queue. Return node 
    /// </summary>
    /// <param name="a_cEvent"></param>
    /// <returns></returns>
    public Node Push(Event a_cEvent)
    {
        //Check if we can use a previous node
        Node n;
        if(!recycledNodes.IsEmpty())
        {
            n = recycledNodes.Pop();
        }
        else
        {
            n = new Node();
        }

        //Set n variables
        n.parent = null;
        n.left = null;
        n.right = null;
        n.cEvent = a_cEvent;

        //check for root
        if(root == null)
        {
            root = n;
            top = n;
            return n;
        }

        bool leftInsert = true;
        Node p = null;
        Node i = root;

        //Determin if we should insert new event to the left or right
        while(i != null)
        {
            //set p to i (p == root)
            p = i;
            //if a_cEvent.yLock is less than i(root).yLock then 
            //i is equal to i.left
            if (a_cEvent.yLock < i.cEvent.yLock)
            {
                leftInsert = true;
                i = i.left;
            }
            else
            {
                leftInsert = false;
                i = i.right;
            }
        }

        n.parent = p;

        //set p left/right
        if(leftInsert)
        {
            p.left = n;
        }
        else
        {
            p.right = n;
        }

        // check if n.yLock is higher than the node which is at the top of the 
        //stack. If so then set top to n
        if(n.cEvent.yLock > top.cEvent.yLock)
        {
            top = n;
        }

        return n;
    }

    /// <summary>
    /// Return top event
    /// </summary>
    /// <returns></returns>
    public Event Top()
    {
        return top.cEvent;
    }

    /// <summary>
    /// Return the top event and remove it
    /// </summary>
    /// <returns></returns>
    public Event Pop()
    {
        Event cEvent = top.cEvent;
        Delete(top);
        return cEvent;
    }

    /// <summary>
    /// Delete Node
    /// </summary>
    /// <param name="a_n"></param>
    public void Delete(Node a_n)
    {
        //Get all the need values from a_n
        Node parent = a_n.parent;
        Node left = a_n.left;
        Node right = a_n.right;

        // deleted node has 0 or 1 children
        if (left == null || right == null)
        {
            //      P           - Parent
            //      a_n         - Node to be deleted
            //  left    right   - Left or right nodes of a_n

            //if only one child remove a_n and re-parent left or right
            //      P           - Parent
            //  left    right   - Left and right nodes of a_n     

            //if left is not null then set child to left. Else set child to right
            Node child;
            if (left != null)
            {
                child = left;
            }
            else
            {
                child = right;
            }

            //Get child parent
            if (child != null)
            {
                child.parent = parent;
            }

            ///
            // Reparent child
            ///

            // if root is equal to a_n. Set root to be child
            if (root == a_n)
            {
                root = child;
            }
            else
            {
                // if a_n is equal to parent.left.
                //Set parent.left to child(a_n.left)
                if (a_n == parent.left)
                {
                    parent.left = child;
                }
                else
                {
                    // if a_n is equal to parent.right.
                    //Set parent.right to child(a_n.right)
                    parent.right = child;
                }
            }
        }
        //both left and right are not null
        else
        {
            //Get the node to replace a_n
            Node splice = Next(a_n);
            //Get the parent node of splice
            Node spliceParent = splice.parent;
            //Get the right child of splice
            Node child = splice.right;

            //Check if child is not null
            if (child != null)
            {
                //set child's parent to spliceParent.
                //This reparents it so it is conencted
                child.parent = spliceParent;
            }

            //check if splice is equal to spliceParent.left
            //then set spliceParent.left to child. Else set spliceParent.right to child
            //This frees up splice so it is no longer connected to anything
            if (splice == spliceParent.left)
            {
                spliceParent.left = child;
            }
            else
            {
                spliceParent.right = child;
            }

            //reparent splice
            splice.parent = a_n.parent;
            //set splice.left and splice.right
            splice.left = a_n.left;
            splice.right = a_n.right;

            //check if root is equal to a_n
            //if so then set root to splice
            if (root == a_n)
            {
                root = splice;
            }
            else
            {
                //check if a_n is equal to parent.left
                //if so then set parent.left to splice
                //else set parent.right to splice
                if (a_n == parent.left)
                {
                    parent.left = splice;
                }
                else
                {
                    parent.right = splice;
                }
            }

            //Check if either left or right is not null
            //if it is not null then set it's left or right .parent to 
            //splice. This sets the children of splice parent to splice
            if (splice.left != null)
            {
                splice.left.parent = splice;
            }
            if (splice.right != null)
            {
                splice.right.parent = splice;
            }
        }

        //check if a_n is equal to top
        if (a_n == top)
        {
            //if root is null
            if (root == null)
            {
                //set top to null
                top = null;
            }
            else
            {
                top = Maximum(root);
            }
        }
        //add a_n to recyclednodes
        recycledNodes.Push(a_n);
    }

    /// <summary>
    /// Get the lowest node
    /// </summary>
    /// <returns></returns>
    Node Minimum()
    {
        return Minimum(root);
    }

    /// <summary>
    /// Get the lowest node 
    /// </summary>
    /// <param name="a_n"></param>
    /// <returns></returns>
    Node Minimum(Node a_n)
    {
        while(a_n.left != null)
        {
            a_n = a_n.left;
        }
        return a_n;
    }

    /// <summary>
    /// Get the highest node
    /// </summary>
    /// <returns></returns>
    Node Maximum()
    {
        return Maximum(root);
    }

    /// <summary>
    /// Get the highest node
    /// </summary>
    /// <param name="a_n"></param>
    /// <returns></returns>
    Node Maximum(Node a_n)
    {
        while (a_n.right != null)
        {
            a_n = a_n.right;
        }
        return a_n;
    }

    /// <summary>
    /// Get the next node
    /// </summary>
    /// <param name="a_n"></param>
    /// <returns></returns>
    Node Next(Node a_n)
    {
        if(a_n.right != null)
        {
            return Minimum(a_n.right);
        }
        Node p = a_n.parent;
        while (p != null && p.right == a_n)
        {
            a_n = p;
            p = a_n.parent;
        }
        return p;
    }

    /// <summary>
    /// Get the previous node 
    /// </summary>
    /// <param name="a_n"></param>
    /// <returns></returns>
    Node Previous(Node a_n)
    {
        if (a_n.left != null)
        {
            return Maximum(a_n.left);
        }
        Node p = a_n.parent;
        while (p != null && p.left == a_n)
        {
            a_n = p;
            p = a_n.parent;
        }
        return p;
    }
}
