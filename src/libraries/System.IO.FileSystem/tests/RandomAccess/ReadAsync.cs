// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using Xunit;

namespace System.IO.Tests
{
    [ActiveIssue("https://github.com/dotnet/runtime/issues/34582", TestPlatforms.Windows, TargetFrameworkMonikers.Netcoreapp, TestRuntimes.Mono)]
    [SkipOnPlatform(TestPlatforms.Browser, "async file IO is not supported on browser")]
    public class RandomAccess_ReadAsync : RandomAccess_Base<ValueTask<int>>
    {
        protected override ValueTask<int> MethodUnderTest(SafeFileHandle handle, byte[] bytes, long fileOffset)
            => RandomAccess.ReadAsync(handle, bytes, fileOffset);

        [Theory]
        [MemberData(nameof(GetSyncAsyncOptions))]
        public async Task TaskAlreadyCanceledAsync(FileOptions options)
        {
            using (SafeFileHandle handle = File.OpenHandle(GetTestFilePath(), FileMode.CreateNew, FileAccess.ReadWrite, options: options))
            {
                CancellationTokenSource cts = GetCancelledTokenSource();
                CancellationToken token = cts.Token;

                Assert.True(RandomAccess.ReadAsync(handle, new byte[1], 0, token).IsCanceled);

                TaskCanceledException ex = await Assert.ThrowsAsync<TaskCanceledException>(() => RandomAccess.ReadAsync(handle, new byte[1], 0, token).AsTask());
                Assert.Equal(token, ex.CancellationToken);
            }
        }

        [Theory]
        [MemberData(nameof(GetSyncAsyncOptions))]
        public async Task ThrowsOnWriteAccess(FileOptions options)
        {
            using (SafeFileHandle handle = GetHandleToExistingFile(FileAccess.Write, options))
            {
                await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await RandomAccess.ReadAsync(handle, new byte[1], 0));
            }
        }

        [Theory]
        [MemberData(nameof(GetSyncAsyncOptions))]
        public async Task ReadToAnEmptyBufferReturnsZeroAsync(FileOptions options)
        {
            string filePath = GetTestFilePath();
            File.WriteAllBytes(filePath, new byte[1]);

            using (SafeFileHandle handle = File.OpenHandle(filePath, FileMode.Open, options: options))
            {
                Assert.Equal(0, await RandomAccess.ReadAsync(handle, Array.Empty<byte>(), fileOffset: 0));
            }
        }

        [Theory]
        [MemberData(nameof(GetSyncAsyncOptions))]
        public async Task ReadFromBeyondEndOfFileReturnsZeroAsync(FileOptions options)
        {
            string filePath = GetTestFilePath();
            File.WriteAllBytes(filePath, new byte[100]);

            using (SafeFileHandle handle = File.OpenHandle(filePath, FileMode.Open, options: options))
            {
                long eof = RandomAccess.GetLength(handle);
                Assert.Equal(0, await RandomAccess.ReadAsync(handle, new byte[1], fileOffset: eof + 1));
            }
        }

        [Theory]
        [MemberData(nameof(GetSyncAsyncOptions))]
        public async Task HappyPath(FileOptions options)
        {
            const int fileSize = 4_001;
            string filePath = GetTestFilePath();
            byte[] expected = RandomNumberGenerator.GetBytes(fileSize);
            File.WriteAllBytes(filePath, expected);

            using (SafeFileHandle handle = File.OpenHandle(filePath, FileMode.Open, options: options))
            {
                byte[] actual = new byte[fileSize + 1];
                int current = 0;
                int total = 0;

                do
                {
                    Memory<byte> buffer = actual.AsMemory(total, Math.Min(actual.Length - total, fileSize / 4));

                    current = await RandomAccess.ReadAsync(handle, buffer, fileOffset: total);

                    Assert.InRange(current, 0, buffer.Length);

                    total += current;
                } while (current != 0);

                Assert.Equal(fileSize, total);
                Assert.Equal(expected, actual.Take(total).ToArray());
            }
        }
    }
}
