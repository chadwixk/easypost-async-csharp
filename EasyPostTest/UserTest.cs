/*
 * Licensed under The MIT License (MIT)
 *
 * Copyright (c) 2014 EasyPost
 * Copyright (C) 2017 AMain.com, Inc.
 * All Rights Reserved
 */

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EasyPost;

namespace EasyPostTest
{
    [TestClass]
    public class UserTest
    {
        private EasyPostClient _client;

        [TestInitialize]
        public void Initialize()
        {
            _client = new EasyPostClient(Environment.GetEnvironmentVariable("EASYPOST_PRODUCTION_API_KEY"));
        }

        [TestMethod]
        public async Task TestRetrieveSelf()
        {
            var user = await _client.GetUser();
            Assert.IsNotNull(user.Id);

            var user2 = await _client.GetUser(user.Id);
            Assert.AreEqual(user.Id, user2.Id);
        }

        [TestMethod]
        public async Task TestCrud()
        {
            var user = await _client.CreateUser("Unit Test User Name");
            Assert.AreEqual(user.ApiKeys.Count, 2);
            Assert.IsNotNull(user.Id);

            var other = await _client.GetUser(user.Id);
            Assert.AreEqual(user.Id, other.Id);

            user.Name = "New Unit Test User Name";
            user = await _client.UpdateUser(user);
            Assert.AreEqual("New Unit Test User Name", user.Name);

            _client.DestroyUser(user.Id).Wait();
            user = await _client.GetUser(user.Id);
            Assert.IsNotNull(user.RequestError);
            Assert.AreEqual(user.RequestError.Code, "NOT_FOUND");
        }
    }
}