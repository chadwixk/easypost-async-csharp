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
            var address = new Address {
                Company = "Simpler Postage Inc",
                Street1 = "164 Townsend Street",
                Street2 = "Unit 1",
                City = "San Francisco",
                State = "CA",
                Country = "US",
                Zip = "94107",
                Phone = "1234567890"
            };
            var toAddress = new Address {
                Company = "Simpler Postage Inc",
                Street1 = "164 Townsend Street",
                Street2 = "Unit 1",
                City = "San Francisco",
                State = "CA",
                Country = "US",
                Zip = "94107",
            };
            var fromAddress = new Address {
                Name = "Andrew Tribone",
                Street1 = "480 Fell St",
                Street2 = "#3",
                City = "San Francisco",
                State = "CA",
                Country = "US",
                Zip = "94102",
            };
            var shipment = new Shipment {
                Parcel = parcel,
                ToAddress = toAddress,
                FromAddress = fromAddress,
                Reference = "ShipmentRef",
            };
            shipment = await _client.CreateShipment(shipment);

            Assert.AreEqual(null, shipment.Parcel.Height);
            Assert.AreEqual("SMALLFLATRATEBOX", shipment.Parcel.PredefinedPackage);
        }
    }
}