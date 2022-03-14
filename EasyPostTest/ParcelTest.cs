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
    public class ParcelTest
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
            var parcel = await _client.CreateParcel(new Parcel {
                Length = 10,
                Width = 20,
                Height = 5,
                Weight = 1.8,
            });
            var retrieved = await _client.GetParcel(parcel.Id);
            Assert.AreEqual(parcel.Id, retrieved.Id);
        }

        [TestMethod]
        public async Task TestPredefinedPackage()
        {
            var parcel = new Parcel { Weight = 1.8, PredefinedPackage = "SMALLFLATRATEBOX" };
            var shipment = new Shipment { Parcel = parcel };
            shipment = await _client.CreateShipment(shipment);

            Assert.AreEqual(null, shipment.Parcel.Height);
            Assert.AreEqual("SMALLFLATRATEBOX", shipment.Parcel.PredefinedPackage);
        }
    }
}