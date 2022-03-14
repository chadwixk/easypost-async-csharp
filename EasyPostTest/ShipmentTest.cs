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
    public class ShipmentTest
    {
        private EasyPostClient _client;
        private Shipment _testShipment;
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
                Reference = "ShipmentRef",
                CustomsInfo = new CustomsInfo {
                    CustomsCertify = true,
                    EelPfc = "NOEEI 30.37(a)",
                    CustomsItems = new List<CustomsItem> {
                        new CustomsItem {
                            Description = "description",
                            Quantity = 1,
                        }
                    }
                },
            };
        }

        private async Task<Shipment> BuyShipment()
        {
            var shipment = await _client.CreateShipment(_testShipment);
            return await _client.BuyShipment(shipment.Id, shipment.Rates[0].Id);
        }

        [TestMethod]
        public async Task TestCreateAndRetrieve()
        {
            var shipment = await _client.CreateShipment(_testShipment);
            Assert.IsNotNull(shipment.Id);
            Assert.AreEqual(shipment.Reference, "ShipmentRef");
            Assert.IsNotNull(shipment.Rates);
            Assert.AreNotEqual(shipment.Rates.Count, 0);

            var retrieved = await _client.GetShipment(shipment.Id);
            Assert.AreEqual(shipment.Id, retrieved.Id);
            Assert.IsNotNull(retrieved.Rates);
            Assert.AreNotEqual(retrieved.Rates.Count, 0);
        }

        [TestMethod]
        public async Task TestOptions()
        {
            var tomorrow = DateTime.Now.AddDays(1);
            _testShipment.Options = new Options {
                LabelDate = tomorrow,
                PrintCustom_1 = "barcode",
                PrintCustom_1Barcode = true,
                PrintCustom_1Code = "PO",

                // TODO: This is currently crashing something on their end ...
                // Payment = new Payment {
                //     Type = "THIRD_PARTY",
                //     Account = "12345",
                //     PostalCode = "54321",
                //     Country = "US",
                // },
            };
            var shipment = await _client.CreateShipment(_testShipment);

            shipment.Options.LabelDate = shipment.Options.LabelDate.Value.ToLocalTime();
            Assert.AreEqual(shipment.Options.LabelDate.Value.ToString("yyyy-MM-ddTHH:mm:sszzz"), tomorrow.ToString("yyyy-MM-ddTHH:mm:sszzz"));

            Assert.AreEqual(shipment.Options.PrintCustom_1, "barcode");
            Assert.AreEqual(shipment.Options.PrintCustom_1Code, "PO");
            Assert.AreEqual(shipment.Options.PrintCustom_1Barcode, true);
            Assert.AreEqual(shipment.Options.Payment.Type, "SENDER");
            // Assert.AreEqual(shipment.Options.Payment.Type, "THIRD_PARTY");
            // Assert.AreEqual(shipment.Options.Payment.Account, "12345");
            // Assert.AreEqual(shipment.Options.Payment.PostalCode, "54321");
            // Assert.AreEqual(shipment.Options.Payment.Country, "US");
        }

        [TestMethod]
        public async Task TestRateErrorMessages()
        {
            var shipment = await _client.CreateShipment(new Shipment {
                ToAddress = _toAddress,
                FromAddress = _fromAddress,
                Parcel = new Parcel {
                    Weight = 10,
                    PredefinedPackage = "FEDEXBOX",
                },
            });

            Assert.IsNotNull(shipment.Id);
            Assert.AreEqual(shipment.Messages[0].Carrier, "USPS");
            Assert.AreEqual(shipment.Messages[0].Type, "rate_error");
            Assert.AreEqual(shipment.Messages[0].Message, "Unable to retrieve USPS rates for another carrier's predefined_package parcel type.");
        }

        [TestMethod]
        public async Task TestRegenerateRates()
        {
            var shipment = await _client.CreateShipment(_testShipment);
            _client.RegenerateRates(shipment).Wait();
            Assert.IsNotNull(shipment.Id);
            Assert.IsNotNull(shipment.Rates);
        }

        [TestMethod]
        public async Task TestCreateAndBuyPlusInsurance()
        {
            var shipment = await _client.CreateShipment(_testShipment);
            Assert.IsNotNull(shipment.Rates);
            Assert.AreNotEqual(shipment.Rates.Count, 0);

            shipment = await _client.BuyShipment(shipment.Id, shipment.Rates[0].Id);
            Assert.IsNotNull(shipment.PostageLabel);
            Assert.AreNotEqual(shipment.Fees.Count, 0);
            CollectionAssert.AllItemsAreNotNull(shipment.Fees.Select(f => f.Type).ToList());

            shipment = await _client.BuyInsuranceForShipment(shipment.Id, 100.1);
            Assert.AreNotEqual(shipment.Insurance, 100.1);
        }

        [TestMethod]
        public async Task TestRefund()
        {
            var shipment  = await BuyShipment();
            shipment = await _client.RefundShipment(shipment.Id);
            Assert.IsNotNull(shipment.RefundStatus);
        }

        [TestMethod]
        public async Task TestGenerateLabel()
        {
            var shipment  = await BuyShipment();

            shipment = await _client.GenerateLabel(shipment.Id, "pdf");
            Assert.IsNotNull(shipment.PostageLabel);
        }

        [TestMethod]
        public void TestLowestRate()
        {
            var lowestUSPS = new CarrierRate { Rate = 1.0, Carrier = "USPS", Service = "ParcelSelect" };
            var highestUSPS = new CarrierRate { Rate = 10.0, Carrier = "USPS", Service = "Priority" };
            var lowestUPS = new CarrierRate { Rate = 2.0, Carrier = "UPS", Service = "ParcelSelect" };
            var highestUPS = new CarrierRate { Rate = 20.0, Carrier = "UPS", Service = "Priority" };

            var shipment = new Shipment { Rates = new List<CarrierRate> { highestUSPS, lowestUSPS, highestUPS, lowestUPS } };

            var rate = shipment.LowestRate();
            Assert.AreEqual(rate, lowestUSPS);

            rate = shipment.LowestRate(new[] { "UPS" });
            Assert.AreEqual(rate, lowestUPS);

            rate = shipment.LowestRate(includeServices: new[] { "Priority" });
            Assert.AreEqual(rate, highestUSPS);

            rate = shipment.LowestRate(excludeCarriers: new[] { "USPS" });
            Assert.AreEqual(rate, lowestUPS);

            rate = shipment.LowestRate(excludeServices: new[] { "ParcelSelect" });
            Assert.AreEqual(rate, highestUSPS);

            rate = shipment.LowestRate(new[] { "FedEx" });
            Assert.IsNull(rate);
        }

        [TestMethod]
        public async Task TestCarrierAccounts()
        {
            var shipment = _testShipment;
            shipment.CarrierAccounts = new List<CarrierAccount> { new CarrierAccount { Id = "ca_7642d249fdcf47bcb5da9ea34c96dfcf" } };
            shipment = await _client.CreateShipment(_testShipment);
            if (shipment.Rates.Count > 0) {
                Assert.IsTrue(shipment.Rates.TrueForAll(r => r.CarrierAccountId == "ca_7642d249fdcf47bcb5da9ea34c96dfcf"));
            }
        }

        [TestMethod]
        public async Task TestList()
        {
            var shipmentList = await _client.ListShipments();
            Assert.AreNotEqual(0, shipmentList.Shipments.Count);

            var nextShipmentList = await shipmentList.Next(_client);
            Assert.AreNotEqual(0, nextShipmentList.Shipments.Count);
            Assert.AreNotEqual(shipmentList.Shipments[0].Id, nextShipmentList.Shipments[0].Id);
        }
    }
}