﻿/*
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
    // These currently do not work with my API keys..
    [Ignore]
    [TestClass]
    public class PickupTest
    {
        private EasyPostClient _client;
        private Pickup _testPickup;
        private Address _address, _toAddress, _fromAddress;
        private Shipment _shipment;

        [TestInitialize]
        public async Task Initialize()
        {
            _client = new EasyPostClient(Environment.GetEnvironmentVariable("EASYPOST_TEST_API_KEY"));
            _address = new Address {
                Company = "Simpler Postage Inc",
                Street1 = "164 Townsend Street",
                Street2 = "Unit 1",
                City = "San Francisco",
                State = "CA",
                Country = "US",
                Zip = "94107",
                Phone = "1234567890"
            };
            _toAddress = new Address {
                Company = "Simpler Postage Inc",
                Street1 = "164 Townsend Street",
                Street2 = "Unit 1",
                City = "San Francisco",
                State = "CA",
                Country = "US",
                Zip = "94107",
            };
            _fromAddress = new Address {
                Name = "Andrew Tribone",
                Street1 = "480 Fell St",
                Street2 = "#3",
                City = "San Francisco",
                State = "CA",
                Country = "US",
                Zip = "94102",
            };
            _shipment = await _client.CreateShipment(new Shipment {
                Parcel = new Parcel {
                    Length = 8,
                    Width = 6,
                    Height = 5,
                    Weight = 10,
                },
                ToAddress = _toAddress,
                FromAddress = _fromAddress,
                Reference = "ShipmentRef",
            });
            _client.BuyShipment(_shipment.Id, _shipment.LowestRate().Id).Wait();
            _testPickup = new Pickup {
                IsAccountAddress = false,
                Instructions = "In mailbox.",
                Address = _address,
                Shipment = _shipment,
                MinDatetime = DateTime.Now.AddDays(1),
                MaxDatetime = DateTime.Now.AddDays(1),
            };
        }

        [TestMethod]
        public async Task TestCreateAndRetrieve()
        {
            var pickup = await _client.CreatePickup(_testPickup);

            Assert.IsNotNull(pickup.Id);
            Assert.AreEqual(pickup.Address.Street1, "164 Townsend Street");

            var retrieved = await _client.GetPickup(pickup.Id);
            Assert.AreEqual(pickup.Id, retrieved.Id);
        }

        [TestMethod]
        public async Task TestBuyAndCancel()
        {
            var pickup = await _client.CreatePickup(_testPickup);

            pickup = await _client.BuyPickup(pickup.Id, pickup.PickupRates[0].Carrier, pickup.PickupRates[0].Service);
            Assert.IsNotNull(pickup.Confirmation);

            pickup = await _client.CancelPickp(pickup.Id);
            Assert.AreEqual(pickup.Status, "canceled");
        }
    }
}