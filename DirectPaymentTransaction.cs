using System;
using com.paypal.sdk.services;
using System.Collections;
using com.paypal.soap.api;
//using com.paypal.sdk.services;
using com.paypal.sdk.profiles;
using ASPDotNetSamples;
using Braintree;
using System.Data.SqlClient;
using System.Web;


namespace CEUeducation.Model.Paypal
{
    public class DirectPaymentTransaction
    {
        private readonly CallerServices caller;

        public DirectPaymentTransaction()
        {
            //caller = new CallerServices();
            //string APIUsername = System.Configuration.ConfigurationManager.AppSettings["APIUsername"].ToString();
            //string APIPassword = System.Configuration.ConfigurationManager.AppSettings["APIPassword"].ToString();
            //string APISignature = System.Configuration.ConfigurationManager.AppSettings["APISignature"].ToString();

            //IAPIProfile profile = ProfileFactory.createSignatureAPIProfile();
            //profile.APIUsername = APIUsername;
            //profile.APIPassword = APIPassword;
            //profile.APISignature = APISignature;
            //profile.Environment = System.Configuration.ConfigurationManager.AppSettings["PayPalEnvironment"].ToString(); ;
            //caller.APIProfile = profile;

            //caller.APIProfile = SetProfile.CreateAPIProfile(APIUsername, APIPassword, "", "", APISignature, "", "", "sandbox", "");            
            //caller.APIProfile = SetProfile.CreateAPIProfile(APIUsername, APIPassword, "", "", "", "certificate.p12", "123456", "sandbox", "");            
        }

        #region Refunds

        public string DoDirectRefund(string TransCode, string paymentAmount, out string message, out string Refundid)
        {
            return DoBrainTreeRefund(TransCode, Convert.ToDecimal(paymentAmount), out message,out Refundid);
        }
        public string DoBrainTreeRefund(string TransCode, decimal Amnt, out string message,out string Refundid)
        {
            int OrganizationID;
            string OrganizationName;
            string MerchantAccountID = string.Empty;
            string MerchantID = string.Empty;
            string PublicKey = string.Empty;
            string PrivateKey = string.Empty;

            SqlConnection connection = mySQLConnection.Open();
            using (SqlDataReader dr = new EnterpriseDA().GetOrganizationCredentialByID(connection, UserInfo.OrganizationID))
            {
                while (dr.Read())
                {
                    OrganizationID = Convert.ToInt32(dr["OrganizationID"].ToString());
                    OrganizationName = dr["OrganizationName"].ToString();
                    MerchantAccountID = dr["MerchantAccountID"].ToString();
                    MerchantID = dr["MerchantID"].ToString();
                    PublicKey = dr["PublicKey"].ToString();
                    PrivateKey = dr["PrivateKey"].ToString();
                }
            }

            bool isExecute = true;
            if (String.IsNullOrEmpty(MerchantAccountID) || String.IsNullOrEmpty(MerchantID) || String.IsNullOrEmpty(PublicKey) || String.IsNullOrEmpty(PrivateKey))
            {
                isExecute = false;
            }

            message = string.Empty;
            Refundid = string.Empty;
            string TransactionId = "";

            if (isExecute)
            {
                var gateway = new BraintreeGateway
                {
                    Environment = Braintree.Environment.SANDBOX,
                    MerchantId = MerchantID,
                    PublicKey = PublicKey,
                    PrivateKey = PrivateKey
                };

                var request = new TransactionRequest
                {
                    Amount = Amnt,
                    MerchantAccountId = MerchantAccountID,
                    //CreditCard = new TransactionCreditCardRequest
                    //{
                    //    CVV = CVV2,
                    //    Number = CCNumber,
                    //    ExpirationDate = expiryDate
                    //},
                    //Options = new TransactionOptionsRequest
                    //{
                    //    SubmitForSettlement = true
                    //}
                };

                Result<Transaction> result = gateway.Transaction.Refund(TransCode,Amnt);

            

                if (result.IsSuccess())
                {
                    Transaction transaction = result.Target;
                    message = result.Message;
                    Refundid = result.Transaction.RefundId;
                    TransactionId = "Refunded";

                }
                else if (result.Transaction != null)
                {
                    Transaction transaction = result.Transaction;
                    message = result.Message + " | status : " + transaction.Status + " | code : " + transaction.ProcessorResponseCode + " | Text : " + transaction.ProcessorResponseText;
                    TransactionId = "-1";
                }
                else
                {
                    result = gateway.Transaction.Void(TransCode);

                    if (result.IsSuccess())
                    {
                        TransactionId = "Voided";
                    }
                    else
                    {
                        message = result.Message;
                        TransactionId = "-1";
                    }
                }
            }
            else
            {
                message = "Merchant account credential is missing for this transaction";
                TransactionId = "-1";
            }
            return TransactionId;
        }
        #endregion


        public string DoDirectPayment(string paymentAmount, string buyerLastName, string buyerFirstName, string buyerAddress1, string buyerAddress2, string buyerCity, string buyerState, string buyerZipCode, string creditCardType, string creditCardNumber, string CVV2, int expMonth, int expYear, out string message)
        {
            return DoBrainTreeTransaction(Convert.ToDecimal(paymentAmount), creditCardType, creditCardNumber, expMonth.ToString() + "/" + expYear.ToString(),CVV2,out message);
        }

        public string DoBrainTreeTransaction(decimal Amnt, string CardType, string CCNumber, string expiryDate, string CVV2, out string message)
        {
            int OrganizationID ;
            string OrganizationName;
            string MerchantAccountID = string.Empty;
            string MerchantID=string.Empty;
            string PublicKey = string.Empty;
            string PrivateKey = string.Empty;

            SqlConnection connection = mySQLConnection.Open();
            using (SqlDataReader dr = new EnterpriseDA().GetOrganizationCredentialByID(connection, UserInfo.OrganizationID))
            {
                while (dr.Read())
                {
                    OrganizationID = Convert.ToInt32(dr["OrganizationID"].ToString());
                    OrganizationName = dr["OrganizationName"].ToString();
                    MerchantAccountID = dr["MerchantAccountID"].ToString();
                    MerchantID = dr["MerchantID"].ToString();
                    PublicKey = dr["PublicKey"].ToString();
                    PrivateKey = dr["PrivateKey"].ToString();
                }
            }

            bool isExecute = true;
            if (String.IsNullOrEmpty(MerchantAccountID) || String.IsNullOrEmpty(MerchantID) || String.IsNullOrEmpty(PublicKey) || String.IsNullOrEmpty(PrivateKey))
            {
                isExecute = false;
            }

            message = string.Empty;
            string TransactionId="";

            if (isExecute)
            {
                var gateway = new BraintreeGateway
                {
                    Environment = Braintree.Environment.SANDBOX,
                    MerchantId = MerchantID,
                    PublicKey = PublicKey,
                    PrivateKey = PrivateKey
                };

                var request = new TransactionRequest
                {
                    Amount = Amnt,
                    MerchantAccountId = MerchantAccountID,
                    CreditCard = new TransactionCreditCardRequest
                    {
                        CVV = CVV2,
                        Number = CCNumber,
                        ExpirationDate = expiryDate
                    },
                    Options = new TransactionOptionsRequest
                    {
                        SubmitForSettlement = true
                    }
                };

                Result<Transaction> result = gateway.Transaction.Sale(request);

               

                if (result.IsSuccess())
                {
                    Transaction transaction = result.Target;
                    TransactionId = transaction.Id;

                }
                else if (result.Transaction != null)
                {
                    Transaction transaction = result.Transaction;
                    message = result.Message + " | status : " + transaction.Status + " | code : " + transaction.ProcessorResponseCode + " | Text : " + transaction.ProcessorResponseText;
                    TransactionId = "-1";
                }
                else
                {
                    message = result.Message;
                    TransactionId = "-1";
                    //foreach (ValidationError error in result.Errors.DeepAll())
                    //{
                    //Console.WriteLine("Attribute: " + error.Attribute);
                    // Console.WriteLine("  Code: " + error.Code);
                    //Console.WriteLine("  Message: " + error.Message);
                    //}
                }
            }
            else
            {
                message = "Merchant account credential is missing for this transaction";
                TransactionId = "-1";
            }
            return TransactionId;
        }

        //try
        //    {

        //    message=string.Empty;
 
        //    // Create the request object
        //    DoDirectPaymentRequestType pp_Request = new DoDirectPaymentRequestType();
        //    pp_Request.Version = "63.0";

        //    // Create the request details object
        //    pp_Request.DoDirectPaymentRequestDetails = new DoDirectPaymentRequestDetailsType();

        //    pp_Request.DoDirectPaymentRequestDetails.IPAddress = "10.244.43.106";
        //    pp_Request.DoDirectPaymentRequestDetails.MerchantSessionId = System.Configuration.ConfigurationManager.AppSettings["SecureMerchantID"].ToString();
        //    pp_Request.DoDirectPaymentRequestDetails.PaymentAction = PaymentActionCodeType.Sale;

        //    #region Credit Card

        //    pp_Request.DoDirectPaymentRequestDetails.CreditCard = new CreditCardDetailsType();

        //    pp_Request.DoDirectPaymentRequestDetails.CreditCard.CreditCardNumber = creditCardNumber;
        //    switch (creditCardType)
        //    {
        //        case "Visa":
        //            pp_Request.DoDirectPaymentRequestDetails.CreditCard.CreditCardType = CreditCardTypeType.Visa;
        //            break;
        //        case "MasterCard":
        //            pp_Request.DoDirectPaymentRequestDetails.CreditCard.CreditCardType = CreditCardTypeType.MasterCard;
        //            break;
        //        case "Discover":
        //            pp_Request.DoDirectPaymentRequestDetails.CreditCard.CreditCardType = CreditCardTypeType.Discover;
        //            break;
        //        case "AMEX":
        //            pp_Request.DoDirectPaymentRequestDetails.CreditCard.CreditCardType = CreditCardTypeType.Amex;
        //            break;
        //    }

        //    #endregion

        //    #region Fill Info

        //    pp_Request.DoDirectPaymentRequestDetails.CreditCard.CVV2 = CVV2;
        //    pp_Request.DoDirectPaymentRequestDetails.CreditCard.ExpMonth = expMonth;
        //    pp_Request.DoDirectPaymentRequestDetails.CreditCard.ExpYear = expYear;

        //    pp_Request.DoDirectPaymentRequestDetails.CreditCard.ExpMonthSpecified = true;
        //    pp_Request.DoDirectPaymentRequestDetails.CreditCard.ExpYearSpecified = true;

        //    pp_Request.DoDirectPaymentRequestDetails.CreditCard.CardOwner = new PayerInfoType();
        //    pp_Request.DoDirectPaymentRequestDetails.CreditCard.CardOwner.Payer = "";
        //    pp_Request.DoDirectPaymentRequestDetails.CreditCard.CardOwner.PayerID = "";
        //    pp_Request.DoDirectPaymentRequestDetails.CreditCard.CardOwner.PayerStatus = PayPalUserStatusCodeType.unverified;
        //    pp_Request.DoDirectPaymentRequestDetails.CreditCard.CardOwner.PayerCountry = CountryCodeType.US;

        //    pp_Request.DoDirectPaymentRequestDetails.CreditCard.CardOwner.Address = new AddressType();
        //    pp_Request.DoDirectPaymentRequestDetails.CreditCard.CardOwner.Address.Street1 = buyerAddress1;
        //    pp_Request.DoDirectPaymentRequestDetails.CreditCard.CardOwner.Address.Street2 = buyerAddress2;
        //    pp_Request.DoDirectPaymentRequestDetails.CreditCard.CardOwner.Address.CityName = buyerCity;
        //    pp_Request.DoDirectPaymentRequestDetails.CreditCard.CardOwner.Address.StateOrProvince = buyerState;
        //    pp_Request.DoDirectPaymentRequestDetails.CreditCard.CardOwner.Address.PostalCode = buyerZipCode;
        //    pp_Request.DoDirectPaymentRequestDetails.CreditCard.CardOwner.Address.CountryName = "USA";
        //    pp_Request.DoDirectPaymentRequestDetails.CreditCard.CardOwner.Address.Country = CountryCodeType.US;
        //    pp_Request.DoDirectPaymentRequestDetails.CreditCard.CardOwner.Address.CountrySpecified = true;

        //    pp_Request.DoDirectPaymentRequestDetails.CreditCard.CardOwner.PayerName = new PersonNameType();
        //    pp_Request.DoDirectPaymentRequestDetails.CreditCard.CardOwner.PayerName.FirstName = buyerFirstName;
        //    pp_Request.DoDirectPaymentRequestDetails.CreditCard.CardOwner.PayerName.LastName = buyerLastName;
        //    pp_Request.DoDirectPaymentRequestDetails.PaymentDetails = new PaymentDetailsType();
        //    pp_Request.DoDirectPaymentRequestDetails.PaymentDetails.OrderTotal = new BasicAmountType();
        //    // NOTE: The only currency supported by the Direct Payment API at this time is US dollars (USD).

        //    pp_Request.DoDirectPaymentRequestDetails.PaymentDetails.OrderTotal.currencyID = CurrencyCodeType.USD;
        //    pp_Request.DoDirectPaymentRequestDetails.PaymentDetails.OrderTotal.Value = paymentAmount;
            
        //    #endregion

        //    DoDirectPaymentResponseType response = new DoDirectPaymentResponseType();
        //    try
        //    {
        //        response = (DoDirectPaymentResponseType)caller.Call("DoDirectPayment", pp_Request);
        //        //Save Credit card transaction in database
        //    }
        //    catch (Exception ex)
        //    {
        //        message = ex.Message;
        //        return "-1";
        //    }

        //        if (response.Errors != null)
        //        {
        //            if (response.Errors.Length > 0)
        //            {
        //                message = response.Errors[0].ErrorCode + " | " + response.Errors[0].LongMessage;
        //                return "-1";
        //            }
        //            else
        //            {
        //                return "-1";
        //            }
        //        }
        //        else
        //        {
        //            return response.TransactionID.ToString();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        message = ex.Message + ":" + ex.StackTrace;
        //        return "-1";
        //    }
    }
}
