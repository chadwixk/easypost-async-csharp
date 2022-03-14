/*
 * Licensed under The MIT License (MIT)
 *
 * Copyright (c) 2014 EasyPost
 * Copyright (C) 2017 AMain.com, Inc.
 * All Rights Reserved
 */

using System;
using System.Threading.Tasks;
using EasyPost;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyPostTest
{
    [TestClass]
    public class ScanFormTest
    {
        private EasyPostClient _client;

        [TestInitialize]
        public void Initialize()
        {
            _client = new EasyPostClient(Environment.GetEnvironmentVariable("EASYPOST_TEST_API_KEY"));
        }

        [TestMethod]
        public async Task TestScanFormList()
        {
            var scanFormList = await _client.ListScanForms(new ScanFormListOptions {
                PageSize = 1,
            });
            Assert.AreNotEqual(null, scanFormList.ScanForms[0].BatchId);
            Assert.AreNotEqual(0, scanFormList.ScanForms.Count);
            var nextScanFormList = await scanFormList.Next(_client);
            Assert.AreNotEqual(scanFormList.ScanForms[0].Id, nextScanFormList.ScanForms[0].Id);
        }

        [TestMethod]
        public async Task TestGetScanForm()
        {
            var scanForm = await _client.GetScanForm("sf_327dee75b87841ff85a260ae77fc223e");
            Assert.AreEqual(scanForm.Id, "sf_327dee75b87841ff85a260ae77fc223e");
        }
    }
}