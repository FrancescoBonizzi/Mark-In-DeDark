using System.Collections.Generic;

namespace Penumbra.Utilities
{
    // Differs from List{T} by allowing direct access to the underlying array.
    // Ref: https://github.com/SiliconStudio/paradox/blob/master/sources/common/core/SiliconStudio.Core/Collections/FastList.cs
    internal class FastList<T> : List<T>
    {
        public T[] Items => this.GetInternalArray();
    }
}
