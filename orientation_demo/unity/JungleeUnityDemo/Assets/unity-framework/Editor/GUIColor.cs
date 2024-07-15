using System.Collections.Generic;
using UnityEngine;

namespace XcelerateGames.Editor
{
    public static class GUIColor
    {
        private static Stack<Color> mStack = null;

        static GUIColor()
        {
            mStack = new Stack<Color>();
            mStack.Push(GUI.color);
        }

        public static void Push(Color color)
        {
            mStack.Push(GUI.color);
            GUI.color = color;
        }

        public static void Pop()
        {
            if(mStack.Count > 0)
            {
                mStack.Pop();
                GUI.color = mStack.Peek();
            }
        }
    }
}