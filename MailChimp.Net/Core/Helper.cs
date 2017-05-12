// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Helper.cs" company="Brandon Seydel">
//   N/A
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace MailChimp.Net.Core
{
    /// <summary>
    /// The helper.
    /// </summary>
    public static class Helper
    {
        private class MailChimpError
        {
			public string Detail { get; set; }
			public string Instance { get; set; }
			public int Status { get; set; }
			public string Title { get; set; }
			public string Type { get; set; }
            public List<MailChimpException.Error> Errors { get; set; }

		}

        /// <summary>
        /// The ensure success mail chimp async.
        /// </summary>
        /// <param name="response">
        /// The response.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        /// <exception cref="MailChimpException">
        /// Custom Mail Chimp Exception
        /// </exception>
        public static async Task EnsureSuccessMailChimpAsync(this HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new MailChimpNotFoundException($"Unable to find the resource at {response.RequestMessage.RequestUri} ");
                }
                var err = (await response.Content.ReadAsStreamAsync()).Deserialize<MailChimpError>();
				var errorText =
	$"Title: {err.Title + Environment.NewLine} Type: {err.Type + Environment.NewLine} Status: {err.Status + Environment.NewLine} + Detail: {err.Detail + Environment.NewLine}";
				errorText += "Errors: " + string.Join(" : ", err.Errors.Select(x => x.Field + " " + x.Message));

                throw new MailChimpException(errorText)
                {
                    Detail = err.Detail,
                    Errors = err.Errors,
                    Instance = err.Instance,
                    Status = err.Status,
                    Title = err.Title,
                    Type = err.Type,
                };
            }
        }

        /// <summary>
        /// The get md 5 hash.
        /// </summary>
        /// <param name="md5Hash">
        /// The md 5 hash.
        /// </param>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        /// The object has already been disposed.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref>
        ///         <name>s</name>
        ///     </paramref>
        ///     is null. 
        /// </exception>
        /// <exception cref="EncoderFallbackException">
        /// A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback"/> is set to <see cref="T:System.Text.EncoderExceptionFallback"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Enlarging the value of this instance would exceed <see cref="P:System.Text.StringBuilder.MaxCapacity"/>. 
        /// </exception>
        /// <exception cref="FormatException">
        /// <paramref>
        ///         <name>format</name>
        ///     </paramref>
        /// includes an unsupported specifier. Supported format specifiers are listed in the Remarks section.
        /// </exception>
        public static string GetHash(this HashAlgorithm md5Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            var builder = new StringBuilder();
            foreach (var t in data)
            {
                builder.Append(t.ToString("x2"));
            }

            return builder.ToString();
        }

        /// <summary>
        /// The deserialize.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <typeparam name="T">
        /// Object to Deserialize
        /// </typeparam>
        /// <returns>
        /// The <see cref="T"/>.
        /// </returns>
        private static T Deserialize<T>(this Stream stream)
        {
            using (var reader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(reader))
            {
                var jsonSerializer = new JsonSerializer();
                return jsonSerializer.Deserialize<T>(jsonReader);
            }
        }
    }
}