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
    public class ReportTest
    {
        private EasyPostClient _client;

        [TestInitialize]
        public void Initialize()
        {
            _client = new EasyPostClient(Environment.GetEnvironmentVariable("EASYPOST_TEST_API_KEY"));
        }

        [TestMethod]
        public async Task TestCreateAndRetrieve()
        {
            var report = await _client.CreateReport("shipment", new Report {
                // Unfortunately, this can only be run once a day. If you need to test more than that change the date here.
                //EndDate = DateTime.Parse("2016-06-01"),
            });
            Assert.IsNotNull(report.Id);

            var retrieved = await _client.GetReport("shipment", report.Id);
            Assert.AreEqual(report.Id, retrieved.Id);

            retrieved = await _client.GetReport(report.Id);
            Assert.AreEqual(report.Id, retrieved.Id);
        }

        [TestMethod]
        public async Task TestList()
        {
            var reportList = await _client.ListReports("shipment", new ReportListOptions {
                PageSize = 1,
            });
            Assert.AreNotEqual(0, reportList.Reports.Count);

            var nextReportList = await reportList.Next(_client);
            Assert.AreNotEqual(reportList.Reports[0].Id, nextReportList.Reports[0].Id);
        }
    }
}