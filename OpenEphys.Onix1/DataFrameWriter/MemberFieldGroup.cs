using Apache.Arrow;
using System.Collections.Generic;

namespace OpenEphys.Onix1.DataFrameWriter
{
    record MemberFieldGroup(MemberNode Member, IReadOnlyList<Field> Fields);
}
