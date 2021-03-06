// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Microsoft.Win32.SafeHandles;

#pragma warning disable SA1121 // we don't want to simplify built-ins here as we're using aliasing
using CFAbsoluteTime = System.Double;

internal static partial class Interop
{
    internal static partial class CoreFoundation
    {
        // https://developer.apple.com/reference/corefoundation/cfabsolutetime
        private static readonly DateTime s_cfDateEpoch = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [GeneratedDllImport(Libraries.CoreFoundationLibrary)]
        private static partial SafeCFDateHandle CFDateCreate(IntPtr zero, CFAbsoluteTime at);

        internal static SafeCFDateHandle CFDateCreate(DateTime date)
        {
            Debug.Assert(
                date.Kind != DateTimeKind.Unspecified,
                "DateTimeKind.Unspecified should be specified to Local or UTC by the caller");

            // UTC stays unchanged, Local is changed.
            // Unspecified gets treated as Local (which may or may not be desired).
            DateTime utcDate = date.ToUniversalTime();

            double epochDeltaSeconds = (utcDate - s_cfDateEpoch).TotalSeconds;

            SafeCFDateHandle cfDate = CFDateCreate(IntPtr.Zero, epochDeltaSeconds);

            if (cfDate.IsInvalid)
            {
                cfDate.Dispose();
                throw new OutOfMemoryException();
            }

            return cfDate;
        }
    }
}

namespace Microsoft.Win32.SafeHandles
{
    internal sealed class SafeCFDateHandle : SafeHandle
    {
        public SafeCFDateHandle()
            : base(IntPtr.Zero, ownsHandle: true)
        {
        }

        protected override bool ReleaseHandle()
        {
            Interop.CoreFoundation.CFRelease(handle);
            SetHandle(IntPtr.Zero);
            return true;
        }

        public override bool IsInvalid => handle == IntPtr.Zero;
    }
}
