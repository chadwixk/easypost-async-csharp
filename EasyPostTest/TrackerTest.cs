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
    public class TrackerTest
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
            const string carrier = "USPS";
            const string trackingCode = "EZ1000000001";

            var tracker = await _client.CreateTracker(carrier, trackingCode);
            Assert.AreEqual(tracker.TrackingCode, trackingCode);
            Assert.IsNotNull(tracker.EstDeliveryDate);
            Assert.IsNotNull(tracker.Carrier);
            Assert.IsNotNull(tracker.PublicUrl);

            var t = await _client.GetTracker(tracker.Id);
            Assert.AreEqual(t.Id, tracker.Id);
        }

        [TestMethod]
        public async Task TestList()
        {
            var trackerList = await _client.ListTrackers();
            Assert.AreNotEqual(0, trackerList.Trackers.Count);

            var nextTrackerList = await trackerList.Next(_client);
            Assert.AreNotEqual(trackerList.Trackers[0].Id, nextTrackerList.Trackers[0].Id);
        }
    }
}