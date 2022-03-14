/*
 * Licensed under The MIT License (MIT)
 *
 * Copyright (c) 2014 EasyPost
 * Copyright (C) 2017 AMain.com, Inc.
 * All Rights Reserved
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EasyPost;

namespace EasyPostTest
{
    [TestClass]
    public class OrderTest
    {
        private EasyPostClient _client;
        private Order _testOrder;
        private List<Shipment> _testShipments;
        private Address _fromAddress;
        private Address _toAddress;

        [TestInitialize]
        public void Initialize()
        {
            _client = new EasyPostClient(Environment.GetEnvironmentVariable("EASYPOST_TEST_API_KEY"));

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
            _testShipments = new List<Shipment> {
                new Shipment {
                    Parcel = new Parcel {
                        Length = 8,
                        Width = 6,
                        Height = 5,
                        Weight = 18,
                    },
                },
                new Shipment {
                    Parcel = new Parcel {
                        Length = 9,
                        Width = 5,
                        Height = 4,
                        Weight = 18,
                    },
                },
            };
            _testOrder = new Order {
                ToAddress = _toAddress,
                FromAddress = _fromAddress,
                Reference = "OrderRef",
                Shipments = _testShipments,
            };
        }

        [TestMethod]
        public async Task TestCreateAndRetrieveOrder()
        {
            var order = await _client.CreateOrder(_testOrder);

            Assert.IsNotNull(order.Id);
            Assert.AreEqual(order.Reference, "OrderRef");

            var retrieved = await _client.GetOrder(order.Id);
            Assert.AreEqual(order.Id, retrieved.Id);
        }

        [TestMethod]
        public async Task TestBuyOrder()
        {
            var order = await _client.CreateOrder(_testOrder);
            order = await _client.BuyOrder(order.Id, "USPS", "Priority");
            Assert.IsNotNull(order.Shipments[0].PostageLabel);
        }

        [TestMethod]
        public async Task TestOrderCarrierAccounts()
        {
            _testOrder.CarrierAccounts = new List<CarrierAccount> {
                new CarrierAccount {
                    // USPS carrier
                    Id = "ca_69353cc3d46b42b5ae06f6949eb0ce0b",
                }
            };
            var order = await _client.CreateOrder(_testOrder);

            Assert.IsNotNull(order.Id);
            Assert.AreEqual(order.Reference, "OrderRef");
            var hashSet = new HashSet<string>(order.Shipments.SelectMany(s => s.Rates).Select(r => r.CarrierAccountId));
            CollectionAssert.AreEqual(hashSet.ToList(), new List<string> { "ca_69353cc3d46b42b5ae06f6949eb0ce0b" });
            Assert.AreEqual(3, order.Rates.Count);

            _testOrder.CarrierAccounts = null;
            var largeOrder = await _client.CreateOrder(_testOrder);
            Assert.IsTrue(order.Rates.Count < largeOrder.Rates.Count);
        }
    }
}