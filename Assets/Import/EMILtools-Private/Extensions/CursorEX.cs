using UnityEngine;

namespace EMILtools.Extensions
{
    public static class CursorEX
    {
        public static void Set(bool visable)
        {
            Cursor.visible = visable;
        }

        public static void Set(CursorLockMode mode)
        {
            Cursor.lockState = mode;
        }

        public static void Set(bool visable, CursorLockMode mode)
        {
            Cursor.visible = visable;
            Cursor.lockState = mode;
        }


    }
}
