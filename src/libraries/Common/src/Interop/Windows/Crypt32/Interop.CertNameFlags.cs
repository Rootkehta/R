// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

internal static partial class Interop
{
    internal static partial class Crypt32
    {
        [Flags]
        internal enum CertNameFlags : int
        {
            None = 0x00000000,
            CERT_NAME_ISSUER_FLAG = 0x00000001,
        }
    }
}
