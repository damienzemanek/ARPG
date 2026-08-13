using System;

namespace EMILtools.Extensions
{
    public static class ArrayEX
    {
        //pass in an empty lambda with the const call like
        // Populate( () => new Class() )
        public static T[] Populate<T>(this T[] arr, Func<T> delegateFactory)
        {
            for (int i = 0; i < arr.Length; i++)
                arr[i] = delegateFactory.Invoke();

            return arr;
        }

        public static T[] Populate<T>(this T[] arr) where T : class, new()
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = new();
            }

            return arr;
        }

        //i dont have the struct one


    }
}