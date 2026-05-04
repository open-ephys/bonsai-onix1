using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenEphys.Onix1.DataFrameWriter
{
    class MemberNode
    {
        public MemberInfo Member { get; set; }
        public MemberNode Parent { get; set; }

        public IEnumerable<MemberInfo> GetPath()
        {
            var current = this;
            while (current != null)
            {
                yield return current.Member;
                current = current.Parent;
            }
        }

        public string GetFullName()
        {
            return string.Join("_", GetPath().Select(m => m.Name).Reverse());
        }
    }
}
