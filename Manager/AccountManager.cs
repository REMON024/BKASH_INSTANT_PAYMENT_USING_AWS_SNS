using NybSys.WASA.Account.BLL;
using System;
using System.Threading.Tasks;
using NybSys.WASA.DTO;
using NybSys.WASA.Common.ExceptionHandle;
using System.Collections.Generic;
using System.Linq;
using NybSys.WASA.Models.VmModels;
using NybSys.WASA.Common.Utilities;
using NybSys.WASA.ExceptionLogManger.BLL;
using NybSys.WASA.WorkerBackEnd.Handler;
using Newtonsoft.Json;
using NybSys.WASA.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace NybSys.WASA.BkashApi
{
    public class AccountManager : IAccountManager, IDisposable
    {
        private readonly ICardBLLManager _cardBLLManager;
        private readonly IAccountBLLManager _accountBLLManager;
        private readonly IRechargeDetailBLLManager _rechargeDetailBLLManager;
        private readonly ITransactionBLLManager _transactionBLLManager;
        private readonly IPaymentBLLManager _paymentBLLManager;
        private readonly IExceptionLogBLLManager _exceptionLogBLLManager;

        public AccountManager
            (
                ICardBLLManager cardBLLManager,
                IAccountBLLManager accountBLLManager,
                IRechargeDetailBLLManager rechargeDetailBLLManager,
                ITransactionBLLManager transactionBLLManager,
                IPaymentBLLManager paymentBLLManager,
                IExceptionLogBLLManager exceptionLogBLLManager
            )
        {
            _cardBLLManager = cardBLLManager;
            _accountBLLManager = accountBLLManager;
            _rechargeDetailBLLManager = rechargeDetailBLLManager;
            _transactionBLLManager = transactionBLLManager;
            _paymentBLLManager = paymentBLLManager;
            _exceptionLogBLLManager = exceptionLogBLLManager;
        }

        public void Dispose()
        {
            _cardBLLManager.Dispose();
        }

        //public async dynamic RecharegByBkash(BkashTransaction model)
        //{
        //    try
        //    {
        //        await _paymentBLLManager.AddBkashTransaction(model);


        //        NybSys.WASA.DTO.Account account = await _accountBLLManager.GetDetails(x => x.AccountDetail.MobileNo == model.DebitMSISDN);

        //        NybSys.WASA.Models.VmModels.AccountAddVm accountAddVm = await _accountBLLManager.GetAccountView(account.AccountNo);

        //        //DTO.User loggedInUser = await _userBLLManager.GetDetails(x => x.UserName == userid);

        //        //if (loggedInUser.PumpId.IsLessThanZeroOrNull())
        //        //{
        //        //    throw new BadRequestException(Common.Constant.Message.ErrorMessages.USER_NO_ASSIGNED_ANY_PUMP);
        //        //}

        //        Transaction transaction = new Transaction
        //        {
        //            AccountNo = account.AccountNo,
        //            DebitAmount = model.Amount,
        //            CreatedBy = accountAddVm.AccountDetail.CustomerName,
        //            CreatedDate = DateTime.Now,
        //            Status = (int)NybSys.WASA.Common.Enums.EnumStatus.Active
        //        };

        //        transaction = await _transactionBLLManager.DebitTransaction(transaction);
        //        CardDetail cardDetail = await _cardBLLManager.GetDetails(x => x.AccountNo == account.AccountNo);
        //        RechargeDetail rechargeDetail = new RechargeDetail
        //        {
        //            Transaction = transaction,
        //            RechargeType = (int)NybSys.WASA.Common.Enums.RechargeType.Bkash,
        //            CreatedBy = accountAddVm.AccountDetail.CustomerName,
        //            CreatedDate = DateTime.Now,
        //            CardNumber = cardDetail.CardNumber,
        //            ReferenceNumber = model.TrxID,
        //            ToAccount = account.AccountNo,
        //            PumpAccountId = -2,
        //            Status = (int)NybSys.WASA.Common.Enums.EnumStatus.Active
        //        };

        //        return await _rechargeDetailBLLManager.AddEntity(rechargeDetail);
        //    }
        //    catch (NotFoundException ex)
        //    {
        //        await _exceptionLogBLLManager.AddExceptionLog(ex.Message, " ", "AccountManager" + "_" + "RecharegBKash", (int)NybSys.WASA.Common.Enums.ExceptionType.BKashPayment, (int)NybSys.WASA.Common.Enums.ActionName.Recharge, (int)NybSys.WASA.Common.Enums.ActionType.Add);

        //        return ex.Message;
        //    }
        //    catch (BadRequestException ex)
        //    {
        //        await _exceptionLogBLLManager.AddExceptionLog(ex.Message, " ", "AccountManager" + "_" + "RecharegBKash", (int)NybSys.WASA.Common.Enums.ExceptionType.BKashPayment, (int)NybSys.WASA.Common.Enums.ActionName.Recharge, (int)NybSys.WASA.Common.Enums.ActionType.Add);

        //        return ex.Message;
        //    }
        //    catch (Exception ex)
        //    {
        //        await _exceptionLogBLLManager.AddExceptionLog(ex.Message, " ", "AccountManager" + "_" + "RecharegBKash", (int)NybSys.WASA.Common.Enums.ExceptionType.BKashPayment, (int)NybSys.WASA.Common.Enums.ActionName.Recharge, (int)NybSys.WASA.Common.Enums.ActionType.Add);

        //        return "";
        //    }
        //}


        public async Task<dynamic> BkashRecharge(string message,string type)
        {
            try
            {
                BkashPaymentRequest request = JsonConvert.DeserializeObject<BkashPaymentRequest>(message);
                NybSys.WASA.Common.Utilities.Messages messages = new NybSys.WASA.Common.Utilities.Messages();

                if (type.ToLower() == "SubscriptionConfirmation".ToLower())
                {
                    SubscribeVM subscribeVM = JsonConvert.DeserializeObject<SubscribeVM>(message);
                    BkashLog bkashTest = new BkashLog();
                    //if (messages.IsMessageSignatureValids(request.body))
                    //{
                    messages.SubscribeToTopics(subscribeVM.SubscribeURL);

                    //}
                    bkashTest.Payload = "SubscriptionConfirmation" + "_" + JsonConvert.SerializeObject(message);
                    bkashTest.CreatedAt = DateTime.Now;

                    return _paymentBLLManager.AddBkashPaymentLog(bkashTest);
                    
                }
                else if (type.ToLower() == "Notification".ToLower())
                {
                    BKMessege bkashMessege = JsonConvert.DeserializeObject<BKMessege>(message);
                    bKashMessege bKashMesseges = JsonConvert.DeserializeObject<bKashMessege>(bkashMessege.Message) ;

                    BkashLog bkashTest = new BkashLog();
                    bkashTest.Payload = "Notification" + "_" + JsonConvert.SerializeObject(bKashMesseges);
                    bkashTest.CreatedAt = DateTime.Now;
                    await _paymentBLLManager.AddBkashPaymentLog(bkashTest);
                    BKashTransactionFilterPagination bKashTransactionFilterPagination = new BKashTransactionFilterPagination();

                    DateTime dt = new DateTime();
                    dt = DateTime.ParseExact(bKashMesseges.dateTime, "yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);

                    var res =await _paymentBLLManager.GetBKashTransaction(bKashMesseges.trxID);
                    
                    if (res.Count==0)
                    {

                        BkashTransaction bkashTransaction = new BkashTransaction
                        {
                            Amount = Convert.ToDecimal(bKashMesseges.amount),
                            CreditOrganizationName = bKashMesseges.creditOrganizationName,
                            CreditShortCode = bKashMesseges.creditShortCode,
                            Currency = bKashMesseges.currency,
                            DateTime = dt,
                            DebitMSISDN = bKashMesseges.debitMSISDN,
                            TransactionReference = bKashMesseges.transactionReference,
                            TransactionStatus = bKashMesseges.transactionStatus,
                            TransactionType = Convert.ToInt32(bKashMesseges.transactionType),
                            TrxID = bKashMesseges.trxID,
                        };
                        await _paymentBLLManager.AddBkashTransaction(bkashTransaction);


                        NybSys.WASA.DTO.Account account = await _accountBLLManager.GetDetails(x => x.AccountDetail.MobileNo == bkashTransaction.DebitMSISDN);

                        NybSys.WASA.Models.VmModels.AccountAddVm accountAddVm = await _accountBLLManager.GetAccountView(account.AccountNo);

                        //DTO.User loggedInUser = await _userBLLManager.GetDetails(x => x.UserName == userid);

                        //if (loggedInUser.PumpId.IsLessThanZeroOrNull())
                        //{
                        //    throw new BadRequestException(Common.Constant.Message.ErrorMessages.USER_NO_ASSIGNED_ANY_PUMP);
                        //}

                        Transaction transaction = new Transaction
                        {
                            AccountNo = account.AccountNo,
                            DebitAmount = bkashTransaction.Amount,
                            CreatedBy = accountAddVm.AccountDetail.CustomerName,
                            CreatedDate = DateTime.Now,
                            Status = (int)NybSys.WASA.Common.Enums.EnumStatus.Active
                        };

                        transaction = await _transactionBLLManager.DebitTransaction(transaction);
                        CardDetail cardDetail = await _cardBLLManager.GetDetails(x => x.AccountNo == account.AccountNo);
                        RechargeDetail rechargeDetail = new RechargeDetail
                        {
                            Transaction = transaction,
                            RechargeType = (int)NybSys.WASA.Common.Enums.RechargeType.Bkash,
                            CreatedBy = accountAddVm.AccountDetail.CustomerName,
                            CreatedDate = DateTime.Now,
                            CardNumber = cardDetail.CardNumber,
                            ReferenceNumber = bkashTransaction.TrxID,
                            ToAccount = account.AccountNo,
                            PumpAccountId = accountAddVm.AccountDetail.PumpId,
                            Status = (int)NybSys.WASA.Common.Enums.EnumStatus.Active
                        };

                        return await _rechargeDetailBLLManager.AddEntity(rechargeDetail);
                    }
                    else
                    {
                        BkashLog bk = new BkashLog();
                        bk.Payload = "Duplicate Transaction" + "_" + JsonConvert.SerializeObject(bKashMesseges);
                        bk.CreatedAt = DateTime.Now;
                       return new JsonResult(await _paymentBLLManager.AddBkashPaymentLog(bk)){StatusCode=(int)Common.Enums.StatusCode.Duplicate } ;
                    }
                    //return BuildResponseMessage.OK(await _accountManager.RecharegByBkash(bkashTransaction));

                }
                else if (type.ToLower() == "UnsubscribeConfirmation".ToLower())
                {
                    BkashLog bkashTest = new BkashLog();
                    bkashTest.Payload = "UnsubscribeConfirmation" + "_" + JsonConvert.SerializeObject(request); ;
                    bkashTest.CreatedAt = DateTime.Now;
                    return await  _paymentBLLManager.AddBkashPaymentLog(bkashTest);
                }

                else
                {
                    BkashLog bkashTest = new BkashLog();
                    bkashTest.Payload = "Unknown message type" + "_" + JsonConvert.SerializeObject(request); ;
                    bkashTest.CreatedAt = DateTime.Now;
                    await _paymentBLLManager.AddBkashPaymentLog(bkashTest);
                    return new JsonResult("Unknown Erro") {StatusCode=(int)Common.Enums.StatusCode.NotFound };
                }

            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message) { StatusCode = (int)Common.Enums.StatusCode.NotFound };
                    //return BuildResponseMessage.InternalServerError();
            }
        }
    }

    public interface IAccountManager : IDisposable
    {
        // Recharge Detail
        //Task<dynamic> RecharegByBkash(BkashTransaction model);
        Task<dynamic> BkashRecharge(string message, string type);
    }
}
