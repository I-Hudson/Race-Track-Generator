using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Stack class. Store items in an array structure with the functionality
 * of a stack. E.G.Pop, push, top
 */
public class Stack<T>
{
    const int defaultSize = 32;
    int count = 0;
    T[] items = null;

    /// <summary>
    /// Constructor
    /// </summary>
    public Stack()
    {
        items = new T[defaultSize];
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="a_size"></param>
    public Stack(int a_size)
    {
        items = new T[a_size];
    }

    /// <summary>
    /// Double the size of the stack if the max number of item is 
    /// reached
    /// </summary>
    void Grow()
    {
        // double the size of items
        int size = 2 * items.Length;
        // Set oldItems to be items
        T[] oldItems = items;
        // Resize the array items to it's new size
        items = new T[size];
        // Place everthing from oldItems into the resized array items
        for (int i = 0; i < count; i++)
        {
            items[i] = oldItems[i];
        }
    }

    /// <summary>
    /// Check if the stack is empty. Return true if it is empty
    /// </summary>
    /// <returns></returns>
    public bool IsEmpty()
    {
        return (count == 0);
    }

    /// <summary>
    /// Return the number of items in the Stack
    /// </summary>
    /// <returns></returns>
    public int Count()
    {
        return count;
    }

    /// <summary>
    /// Clear the Stack
    /// </summary>
    public void Clear()
    {
        count = 0;
    }

    /// <summary>
    /// Add (Push) a new item onto the Stack
    /// </summary>
    /// <param name="a_item"></param>
    public void Push(T a_item)
    {
        // if count is the same as items.length then increase the size of items
        if(count == items.Length)
        {
            Grow();
        }
        // Add the new item
        items[count++] = a_item;
    }

    /// <summary>
    ///  Return the top item without removing it 
    /// </summary>
    /// <returns></returns>
    public T Pop()
    {
        return items[--count];
    }
}
