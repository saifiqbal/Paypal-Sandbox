/*
 * Copyright 2005, 2008 PayPal, Inc. All Rights Reserved.
 *
 * SetExpressCheckout NVP example; last modified 08MAY23. 
 *
 * Initiate an Express Checkout transaction.  
 */
using System;
using com.paypal.sdk.services;
using com.paypal.sdk.profiles;
using com.paypal.sdk.util;
/**
 * PayPal .NET SDK sample code
 */
namespace CEUeducation.PayPal
{
	public class ECSetExpressCheckout
	{
		public ECSetExpressCheckout()
		{
		}
		public string ECSetExpressCheckoutCode(string returnURL,string cancelURL,string amount,string paymentType,string currencyCode)
		{
			NVPCallerServices caller = new NVPCallerServices();
			IAPIProfile profile = ProfileFactory.createSignatureAPIProfile();
			/*
			 WARNING: Do not embed plaintext credentials in your application code.
			 Doing so is insecure and against best practices.
			 Your API credentials must be handled securely. Please consider
			 encrypting them for use in any production environment, and ensure
			 that only authorized individuals may view or modify them.
			 */

            // Set up your API credentials, PayPal end point, API operation and version.
			profile.APIUsername = "sdk-three_api1.sdk.com";
			profile.APIPassword = "QFZCWN5HZM8VBG7Q";
			profile.APISignature = "AVGidzoSQiGWu.lGj3z15HLczXaaAcK6imHawrjefqgclVwBe8imgCHZ";
			profile.Environment="sandbox";
			caller.APIProfile = profile;

			NVPCodec encoder = new NVPCodec();
			encoder["VERSION"] =  "51.0";	
			encoder["METHOD"] =  "SetExpressCheckout";

            // Add request-specific fields to the request.
			encoder["RETURNURL"] =  returnURL;
			encoder["CANCELURL"] =  cancelURL;	
			encoder["AMT"] =  amount;
			encoder["PAYMENTACTION"] =  paymentType;
			encoder["CURRENCYCODE"] =  currencyCode;	

            // Execute the API operation and obtain the response.
			string pStrrequestforNvp= encoder.Encode();
			string pStresponsenvp=caller.Call(pStrrequestforNvp);

			NVPCodec decoder = new NVPCodec();
			decoder.Decode(pStresponsenvp);
			return decoder["ACK"];
		}
	}
}
