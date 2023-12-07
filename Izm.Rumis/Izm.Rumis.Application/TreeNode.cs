using System.Collections.Generic;

namespace Izm.Rumis.Application
{
    public class TreeNode<T>
    {
        public T Data { get; set; }
        public TreeNode<T> Parent { get; set; }
        public IEnumerable<TreeNode<T>> Children { get; set; }
    }
}
