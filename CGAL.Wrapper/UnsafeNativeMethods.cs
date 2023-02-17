using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CGAL.Wrapper
{
    internal class UnsafeNativeMethods
    {
        private const string DLL_NAME = "CGAL.Native.dll";
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void CreateInteriorStraightSkeleton(ref IntPtr testPtr);/* output - obb as points */

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void ReleaseDoubleArray(IntPtr arr);
    }
}
