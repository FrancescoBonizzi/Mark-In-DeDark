using MarkInDeDark.Shadows.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace MarkInDeDark.Shadows
{
    /// <summary>
    /// Provides extension methods for various types.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Adds the elements of the specified collection to the end of the <see cref="IList{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of collection elements.</typeparam>
        /// <param name="listInterface">The <see cref="IList{T}"/> to add elements to.</param>
        /// <param name="collection">
        /// The collection whose elements should be added to the end of the <see cref="IList{T}"/>.
        /// The collection itself cannot be <c>null</c>, but it can contain elements that are
        /// <c>null</c>, if type <typeparamref name="T"/> is a reference type.
        /// </param>
        public static void AddRange<T>(this IList<T> listInterface, IEnumerable<T> collection)
        {
            // Use fast path in case of extended observable collection.
            if (listInterface is ExtendedObservableCollection<T> extendedObservableCollection)
            {
                extendedObservableCollection.AddRange(collection);
                return;
            }

            // Use fast path in case of list.
            if (listInterface is List<T> list)
            {
                list.AddRange(collection);
                return;
            }

            // Fallback to iterative add.
            foreach (T item in collection)
                listInterface.Add(item);
        }

        public static T[] GetInternalArray<T>(this List<T> list)
        {
            return ArrayAccessor<T>.Getter(list);
        }
    }

    public static class ArrayAccessor<T>
    {
        public static Func<List<T>, T[]> Getter;

        static ArrayAccessor()
        {
            var dm = new DynamicMethod("get", MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(T[]), new Type[] { typeof(List<T>) }, typeof(ArrayAccessor<T>), true);
            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0); // Load List<T> argument
            il.Emit(OpCodes.Ldfld, typeof(List<T>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance)); // Replace argument by field
            il.Emit(OpCodes.Ret); // Return field
            Getter = (Func<List<T>, T[]>)dm.CreateDelegate(typeof(Func<List<T>, T[]>));
        }
    }
}
