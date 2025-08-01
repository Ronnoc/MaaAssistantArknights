// <copyright file="IHttpService.cs" company="MaaAssistantArknights">
// Part of the MaaWpfGui project, maintained by the MaaAssistantArknights team (Maa Team)
// Copyright (C) 2021-2025 MaaAssistantArknights Contributors
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License v3.0 only as published by
// the Free Software Foundation, either version 3 of the License, or
// any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY
// </copyright>

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace MaaWpfGui.Services.Web
{
    public interface IHttpService
    {
        /// <summary>
        /// Test url available and return legacy
        /// </summary>
        /// <param name="uri">Target Uri</param>
        /// <param name="extraHeader">Extra HTTP Request Headers</param>
        /// <param name="uriPartial">Which parts of uri to log</param>
        /// <returns>Legacy in ms, -1 when response code not equal 200</returns>
        Task<double> HeadAsync(Uri uri, Dictionary<string, string>? extraHeader = null, UriPartial uriPartial = UriPartial.Query);

        /// <summary>
        /// Send HTTP GET request and get a string response
        /// </summary>
        /// <param name="uri">Target Uri</param>
        /// <param name="extraHeader">Extra HTTP Request Headers</param>
        /// <param name="httpCompletionOption">The HTTP completion option</param>
        /// <returns>Response string, null when failed</returns>
        Task<string?> GetStringAsync(Uri uri, Dictionary<string, string>? extraHeader = null, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead);

        /// <summary>
        /// Send HTTP GET request and get a body stream response
        /// </summary>
        /// <param name="uri">Target Uri</param>
        /// <param name="extraHeader">Extra HTTP Request Headers</param>
        /// <param name="httpCompletionOption">The HTTP completion option</param>
        /// <returns>Response stream, null when failed</returns>
        [UsedImplicitly]
        Task<Stream?> GetStreamAsync(Uri uri, Dictionary<string, string>? extraHeader = null, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead);

        /// <summary>
        /// Send HTTP GET request and get the original <see cref="HttpRequestMessage"/>
        /// </summary>
        /// <param name="uri">Target Uri</param>
        /// <param name="extraHeader">Extra HTTP Request Headers</param>
        /// <param name="httpCompletionOption">The HTTP completion option</param>
        /// <param name="uriPartial">Which parts of uri to log</param>
        /// <returns><see cref="HttpRequestMessage"/> object</returns>
        Task<HttpResponseMessage> GetAsync(Uri uri, Dictionary<string, string>? extraHeader = null, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseHeadersRead, UriPartial uriPartial = UriPartial.Query);

        /// <summary>
        /// Send HTTP POST request and a string response
        /// </summary>
        /// <param name="uri">Target Uri</param>
        /// <param name="content">The POST body content, will be serialized by <see cref="System.Text.Json.JsonSerializer"/></param>
        /// <param name="extraHeader">Extra HTTP Request Headers</param>
        /// <typeparam name="T">The type of the POST body content</typeparam>
        /// <returns>Response string, null when failed</returns>
        Task<string?> PostAsJsonAsync<T>(Uri uri, T content, Dictionary<string, string>? extraHeader = null);

        /// <summary>
        /// Send HTTP POST request and a string response
        /// </summary>
        /// <param name="uri">Target Uri</param>
        /// <param name="content">The POST body content</param>
        /// <param name="extraHeader">Extra HTTP Request Headers</param>
        /// <returns>Response string, null when failed</returns>
        Task<string?> PostAsFormUrlEncodedAsync(Uri uri, Dictionary<string, string?> content, Dictionary<string, string>? extraHeader = null);

        /// <summary>
        /// Send HTTP POST request with raw HttpContent and get the response
        /// </summary>
        /// <param name="uri">Target Uri</param>
        /// <param name="content">The POST body content</param>
        /// <param name="extraHeader">Extra HTTP Request Headers</param>
        /// <param name="uriPartial">Which parts of uri to log</param>
        /// <returns>HttpResponseMessage, null when failed</returns>
        Task<HttpResponseMessage> PostAsync(Uri uri, HttpContent content, Dictionary<string, string>? extraHeader = null, UriPartial uriPartial = UriPartial.Query);

        /// <summary>
        /// Download a file from the Web
        /// </summary>
        /// <param name="uri">The URI of the file</param>
        /// <param name="fileName">On disk filename</param>
        /// <param name="contentType">File content type</param>
        /// <returns>True if success, False if failed</returns>
        Task<bool> DownloadFileAsync(Uri uri, string fileName, string? contentType = null);
    }
}
