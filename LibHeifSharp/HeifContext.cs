/*
 * .NET bindings for libheif.
 * Copyright (c) 2020 Nicholas Hayes
 *
 * Portions Copyright (c) 2017 struktur AG, Dirk Farin <farin@struktur.de>
 *
 * This file is part of libheif-sharp.
 *
 * libheif-sharp is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as
 * published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version.
 *
 * libheif-sharp is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with libheif-sharp.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using LibHeifSharp.Interop;
using LibHeifSharp.ResourceManagement;

namespace LibHeifSharp
{
    /// <summary>
    /// The LibHeif context.
    /// </summary>
    /// <seealso cref="Disposable" />
    /// <threadsafety static="true" instance="false"/>
    public sealed class HeifContext : Disposable
    {
        private SafeHeifContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeifContext"/> class.
        /// </summary>
        /// <exception cref="HeifException">
        /// Unable to create the native HeifContext.
        ///
        /// -or-
        ///
        /// The LibHeif version is not supported.
        /// </exception>
        public HeifContext()
        {
            this.context = CreateNativeContext();
        }

        /// <summary>
        /// Writes this instance to the specified file.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="path"/> is empty, contains only whitespace or contains invalid characters.
        /// </exception>
        /// <exception cref="HeifException">A LibHeif error occurred.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="UnauthorizedAccessException">
        /// The access requested is not permitted by the operating system for the specified path.
        /// </exception>
        public void WriteToFile(string path)
        {
            Validate.IsNotNull(path, nameof(path));
            VerifyNotDisposed();

            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writerStreamIO = new HeifStreamWriter(stream, ownsStream: false))
            {
                var error = LibHeifNative.heif_context_write(this.context,
                                                             writerStreamIO.WriterHandle,
                                                             IntPtr.Zero);
                HandleFileIOError(writerStreamIO, error);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposableUtil.Free(ref this.context);
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Creates the native LibHeif context.
        /// </summary>
        /// <returns>The native LibHeif context.</returns>
        /// <exception cref="HeifException">Unable to create the native HeifContext.</exception>
        private static SafeHeifContext CreateNativeContext()
        {
            var context = LibHeifNative.heif_context_alloc();

            if (context.IsInvalid)
            {
               throw new HeifException("Unable to create the native HeifContext.");
            }

            return context;
        }

        /// <summary>
        /// Handles the file IO error.
        /// </summary>
        /// <param name="heifIOError">The IO error.</param>
        /// <param name="error">The error.</param>
        /// <exception cref="HeifException">The exception that is thrown with the error details.</exception>
        private static void HandleFileIOError(IHeifIOError heifIOError, in heif_error error)
        {
            if (error.IsError)
            {
                if (heifIOError != null && heifIOError.CallbackExceptionInfo != null)
                {
                    var inner = heifIOError.CallbackExceptionInfo.SourceException;

                    throw new HeifException(inner.Message, inner);
                }
                else
                {
                    error.ThrowIfError();
                }
            }
        }
    }
}
