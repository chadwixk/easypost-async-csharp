/*
 * Licensed under The MIT License (MIT)
 *
 * Copyright (c) 2014 EasyPost
 * Copyright (C) 2017 AMain.com, Inc.
 * All Rights Reserved
 */

using System.Collections.Generic;
using System.Text.Json;
using RestSharp;
using RestSharp.Serializers.Json;

namespace EasyPost
{
    public static class ClientFactory
    {
        /// <summary>
        /// Dictionary of all static client instances based on the client API base URL
        /// </summary>
        private static readonly Dictionary<string, RestClient> _clients = new();

        public static RestClient GetClient(
            string apiBase)
        {
            // If we have an existing instance of this client, return it outside of the lock
            if (_clients.TryGetValue(apiBase, out var client)) {
                return client;
            }

            // Create a new instance, but make sure only one thread can create it
            lock (_clients) {
                // Try again to get it, as it might have just been created by another thread
                if (_clients.TryGetValue(apiBase, out client)) {
                    return client;
                }

                // Create the client and set it up to use snake_case naming
                client = new RestClient(apiBase);
                client.UseSystemTextJson(new JsonSerializerOptions(JsonSerializerDefaults.Web) {
                    PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
                    Converters = {
                        new NullToDefaultConverterFactory()
                    }
                });
                _clients.Add(apiBase, client);
                return client;
            }
        }
    }
}