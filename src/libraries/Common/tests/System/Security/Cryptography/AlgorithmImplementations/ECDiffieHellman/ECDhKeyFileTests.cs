// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Security.Cryptography.Tests;
using Xunit;

namespace System.Security.Cryptography.EcDiffieHellman.Tests
{
    [SkipOnPlatform(TestPlatforms.Browser, "Not supported on Browser")]
    public class ECDhKeyFileTests : ECKeyFileTests<ECDiffieHellman>
    {
        protected override ECDiffieHellman CreateKey() => ECDiffieHellmanFactory.Create();
        protected override void Exercise(ECDiffieHellman key) => key.Exercise();

        protected override Func<ECDiffieHellman, byte[]> PublicKeyWriteArrayFunc { get; } =
            key => key.PublicKey.ExportSubjectPublicKeyInfo();

        protected override WriteKeyToSpanFunc PublicKeyWriteSpanFunc { get; } =
            (ECDiffieHellman key, Span<byte> destination, out int bytesWritten) =>
                key.PublicKey.TryExportSubjectPublicKeyInfo(destination, out bytesWritten);
    }
}
