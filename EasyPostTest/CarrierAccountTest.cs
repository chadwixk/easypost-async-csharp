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
    public class CarrierAccountTest
    {
        private EasyPostClient _client;

        [TestInitialize]
        public void Initialize()
        {
            _client = new EasyPostClient(Environment.GetEnvironmentVariable("EASYPOST_PRODUCTION_API_KEY"));
        }

        [TestMethod]
        public async Task TestRetrieve()
        {
            var account = await _client.GetCarrierAccount("ca_2f709c59fccb4488a10641920a12cc7d");
            Assert.AreEqual("ca_2f709c59fccb4488a10641920a12cc7d", account.Id);
        }

        [TestMethod]
        public async Task TestCrud()
        {
            var account = await _client.CreateCarrierAccount(new CarrierAccount {
                Type = "DhlExpressAccount",
                Description = "test account description",
            });

            Assert.IsNotNull(account.Id);
            Assert.AreEqual(account.Type, "DhlExpressAccount");

            account.Reference = "new-reference";
            account = await _client.UpdateCarrierAccount(account);
            Assert.AreEqual("new-reference", account.Reference);

            _client.DestroyCarrierAccount(account.Id).Wait();

            account = await _client.GetCarrierAccount(account.Id);
            Assert.IsNotNull(account.RequestError);
            Assert.AreEqual(account.RequestError.Code, "NOT_FOUND");
        }

        [TestMethod]
        public async Task TestList()
        {
            var accounts = await _client.ListCarrierAccounts();
            Assert.AreEqual(accounts[0].Id, "ca_2f709c59fccb4488a10641920a12cc7d");
        }
    }
}