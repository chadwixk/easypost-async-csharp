/*
 * Licensed under The MIT License (MIT)
 *
 * Copyright (c) 2014 EasyPost
 * Copyright (C) 2017 AMain.com, Inc.
 * All Rights Reserved
 */

using System;
using EasyPost;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyPostTest
{
    [TestClass]
    public class BatchTest
    {
        private EasyPostClient _client;
        private Shipment _testShipment;
        private Shipment _testBatchShipment;
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
            _testShipment = new Shipment {
                ToAddress = _toAddress,
                FromAddress = _fromAddress,
                Parcel = new Parcel {
                    Length = 8,
                    Width = 6,
                    Height = 5,
                    Weight = 10,
                },
            };
            _testBatchShipment = new Shipment {
                ToAddress = _toAddress,
                FromAddress = _fromAddress,
                Parcel = new Parcel {
                    Length = 8,
                    Width = 6,
                    Height = 5,
                    Weight = 10,
                },
                Carrier = "USPS",
                Service = "Priority",
            };
        }

        [TestMethod]
        public async Task TestRetrieve()
        {
            var batch = await _client.CreateBatch();
            var retrieved = await _client.GetBatch(batch.Id);
            Assert.AreEqual(batch.Id, retrieved.Id);
        }

        [TestMethod]
        public async Task TestAddRemoveShipments()
        {
            var batch = await _client.CreateBatch();
            var shipment = await _client.CreateShipment(_testShipment);
            var otherShipment = await _client.CreateShipment(_testShipment);

            while (batch.State != "created") {
                batch = await _client.GetBatch(batch.Id);
            }

            batch = await _client.AddShipmentsToBatch(batch.Id, new[] { shipment, otherShipment });

            while (batch.Shipments == null) {
                batch = await _client.GetBatch(batch.Id);
            }
            var shipmentIds = batch.Shipments.Select(ship => ship.Id).ToList();
            Assert.AreEqual(batch.NumShipments, 2);
            CollectionAssert.Contains(shipmentIds, shipment.Id);
            CollectionAssert.Contains(shipmentIds, otherShipment.Id);

            batch = await _client.RemoveShipmentsFromBatch(batch.Id, new[] { shipment, otherShipment });
            Assert.AreEqual(batch.NumShipments, 0);
        }

        public async Task<Batch> CreateBatch()
        {
            return await _client.CreateBatch(new[] { _testBatchShipment }, "EasyPostCSharpTest");
        }

        [TestMethod]
        public async Task TestCreateThenBuyThenGenerateLabelAndScanForm()
        {
            var batch = await CreateBatch();

            Assert.IsNotNull(batch.Id);
            Assert.AreEqual(batch.Reference, "EasyPostCSharpTest");
            Assert.AreEqual(batch.State, "creating");

            while (batch.State == "creating") {
                batch = await _client.GetBatch(batch.Id);
            }
            batch = await _client.BuyLabelsForBatch(batch.Id);

            while (batch.State == "created") {
                batch = await _client.GetBatch(batch.Id);
            }
            Assert.AreEqual(batch.State, "purchased");

            batch = await _client.GenerateLabelForBatch(batch.Id, "pdf");
            Assert.AreEqual(batch.State, "label_generating");

            batch = await _client.GenerateScanFormForBatch(batch.Id);
        }

        [TestMethod]
        public async Task TestGenerateLabelWithOrderBy()
        {
            var batch = await CreateBatch();

            while (batch.State == "creating") {
                batch = await _client.GetBatch(batch.Id);
            }
            batch = await _client.BuyLabelsForBatch(batch.Id);

            while (batch.State == "created") {
                batch = await _client.GetBatch(batch.Id);
            }
            batch = await _client.GenerateLabelForBatch(batch.Id, "pdf", "reference DESC");
            Assert.AreEqual(batch.State, "label_generating");
        }
    }
}