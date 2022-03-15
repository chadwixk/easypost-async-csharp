/*
 * Licensed under The MIT License (MIT)
 *
 * Copyright (c) 2014 EasyPost
 * Copyright (C) 2017 AMain.com, Inc.
 * All Rights Reserved
 */

using System;

namespace EasyPost
{
    /// <summary>
    /// Provides configuration options for the EasyPost client
    /// </summary>
    public class ClientConfiguration
    {
        internal const string DefaultBaseUrl = "https://api.easypost.com/v2";

        /// <summary>
        /// The API key to use on per-request basis
        /// </summary>
        public string ApiKey { get; }

        /// <summary>
        /// The API base URI to use on a per-request basis
        /// </summary>
        public string ApiBase { get; }

        /// <summary>
        /// Timeout in milliseconds to use for requests made by the client
        /// </summary>
        public int Timeout { get; }

        /// <summary>
        /// Create a ClientConfiguration instance
        /// </summary>
        /// <param name="apiKey">The API key to use for the client connection</param>
        public ClientConfiguration(
            string apiKey)
            : this(apiKey, DefaultBaseUrl, 0)
        {
        }

        /// <summary>
        /// Create a ClientConfiguration instance
        /// </summary>
        /// <param name="apiKey">The API key to use for the client connection</param>
        /// <param name="timeout">The timeout to use for client operations. 0 for the default.</param>
        public ClientConfiguration(
            string apiKey,
            int timeout)
            : this(apiKey, DefaultBaseUrl, timeout)
        {
        }

        /// <summary>
        /// Create an ClientConfiguration instance
        /// </summary>
        /// <param name="apiKey">The API key to use for the client connection</param>
        /// <param name="apiBase">The base API url to use for the client connection</param>
        public ClientConfiguration(
            string apiKey,
            string apiBase)
            : this(apiKey, apiBase, 0)
        {
        }

        /// <summary>
        /// Create an ClientConfiguration instance
        /// </summary>
        /// <param name="apiKey">The API key to use for the client connection</param>
        /// <param name="apiBase">The base API url to use for the client connection</param>
        /// <param name="timeout">The timeout to use for client operations. 0 for the default.</param>
        public ClientConfiguration(
            string apiKey,
            string apiBase,
            int timeout)
        {
            if (string.IsNullOrEmpty(apiKey)) {
                throw new ArgumentException("The API Key must be provided!", nameof(apiKey));
            }
            ApiKey = apiKey;
            ApiBase = apiBase;
            Timeout = timeout;
        }
    }
}