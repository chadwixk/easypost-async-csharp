/*
 * Licensed under The MIT License (MIT)
 *
 * Copyright (c) 2014 EasyPost
 * Copyright (C) 2017 AMain.com, Inc.
 * All Rights Reserved
 */

using System;
using System.Net;
using System.Threading.Tasks;
using EasyPost;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyPostTest
{
    [TestClass]
    public class AddressTest
    {
        private EasyPostClient _client;
        private Address _testAddress;

        [TestInitialize]
        public void Initialize()
        {
            _client = new EasyPostClient(Environment.GetEnvironmentVariable("EASYPOST_TEST_API_KEY"));
            _testAddress = new Address {
                Company = "Simpler Postage Inc",
                Street1 = "164 Townsend Street",
                Street2 = "Unit 1",
                City = "San Francisco",
                State = "CA",
                Country = "US",
                Zip = "94107"
            };
        }

        [TestMethod]
        public async Task TestRetrieveInvalidId()
        {
            var address = await _client.GetAddress("not-an-id");
            Assert.IsNotNull(address.RequestError);
            Assert.AreEqual(address.RequestError.StatusCode, HttpStatusCode.NotFound);
            Assert.AreEqual(address.RequestError.Code, "NOT_FOUND");
            Assert.AreEqual(address.RequestError.Message, "The requested resource could not be found.");
            Assert.AreEqual(address.RequestError.Content, "{\"error\":{\"code\":\"NOT_FOUND\",\"message\":\"The requested resource could not be found.\",\"errors\":[]}}");
        }

        [TestMethod]
        public async Task TestCreateAndRetrieve()
        {
            var address = await _client.CreateAddress(_testAddress);
            Assert.IsNotNull(address.Id);
            Assert.AreEqual(address.Company, "Simpler Postage Inc");
            Assert.IsNull(address.Name);

            var retrieved = await _client.GetAddress(address.Id);
            Assert.AreEqual(address.Id, retrieved.Id);
        }

        [TestMethod]
        public async Task TestCreateWithVerifications()
        {
            var address = await _client.CreateAddress(_testAddress, VerificationFlags.Delivery | VerificationFlags.Zip4);
            Assert.IsNotNull(address.Verifications.Delivery);
            Assert.AreEqual(address.Verifications.Delivery.Success, true);
            Assert.IsNotNull(address.Verifications.Zip4);
            Assert.AreEqual(address.Verifications.Zip4.Success, true);

            address = await _client.CreateAddress(new Address {
                    Company = "Simpler Postage Inc",
                    Street1 = "123 Fake Street",
                    Zip = "94107"
                },
                VerificationFlags.Delivery | VerificationFlags.Zip4);
            Assert.AreEqual(address.Verifications.Delivery.Success, false);
            Assert.AreEqual(address.Verifications.Zip4.Success, false);
        }

        [TestMethod]
        public async Task TestCreateWithStrictVerifications()
        {
            var address = await _client.CreateAddress(new Address {
                    Company = "Simpler Postage Inc",
                    Street1 = "123 Fake Street",
                    Zip = "94107"
                },
                VerificationFlags.DeliveryStrict | VerificationFlags.Zip4Strict);
            Assert.IsNotNull(address.RequestError);
            Assert.AreEqual(address.RequestError.StatusCode, (HttpStatusCode)422);
            Assert.AreEqual(address.RequestError.Code, "ADDRESS.VERIFY.FAILURE");
            Assert.AreEqual(address.RequestError.Message, "Unable to verify address.");
            Assert.AreEqual(address.RequestError.Errors.Count, 1);
            Assert.AreEqual(address.RequestError.Errors[0].Code, "E.ADDRESS.NOT_FOUND");
            Assert.AreEqual(address.RequestError.Errors[0].Field, "address");
            Assert.AreEqual(address.RequestError.Errors[0].Message, "Address not found");
            Assert.AreEqual(address.RequestError.Errors[0].Suggestion, null);
        }

        [TestMethod]
        public async Task TestVerify()
        {
            var address = await _client.CreateAddress(_testAddress);
            address = await _client.VerifyAddress(address);
            Assert.IsNotNull(address.Id);
            Assert.AreEqual(address.Company, "SIMPLER POSTAGE INC");
            Assert.AreEqual(address.Street1, "164 TOWNSEND ST UNIT 1");
            Assert.IsNull(address.Name);
            Assert.IsFalse((bool)address.Residential);
        }

        [TestMethod]
        public async Task TestVerifyCarrier()
        {
            var address = await _client.CreateAddress(_testAddress);
            address = await _client.VerifyAddress(address, "usps");
            Assert.IsNotNull(address.Id);
            Assert.AreEqual(address.Company, "SIMPLER POSTAGE INC");
            Assert.AreEqual(address.Street1, "164 TOWNSEND ST UNIT 1");
            Assert.IsNull(address.Name);
        }

        [TestMethod]
        public async Task TestVerifyBeforeCreate()
        {
            var address = await _client.VerifyAddress(_testAddress);
            Assert.IsNotNull(address.Id);
            Assert.AreEqual(address.Company, "SIMPLER POSTAGE INC");
        }

        [TestMethod]
        public async Task TestVerificationFailure()
        {
            _testAddress.Street1 = "1645456 Townsend Street";
            var address = await _client.CreateAddress(_testAddress);
            address = await _client.VerifyAddress(address);
            Assert.IsNotNull(address.RequestError);
            Assert.AreEqual(address.RequestError.StatusCode, (HttpStatusCode)422);
            Assert.AreEqual(address.RequestError.Code, "ADDRESS.VERIFY.FAILURE");
            Assert.AreEqual(address.RequestError.Message, "Unable to verify address.");
            Assert.AreEqual(address.RequestError.Errors.Count, 1);
            Assert.AreEqual(address.RequestError.Errors[0].Code, "E.ADDRESS.NOT_FOUND");
            Assert.AreEqual(address.RequestError.Errors[0].Field, "address");
            Assert.AreEqual(address.RequestError.Errors[0].Message, "Address not found");
        }
    }
}