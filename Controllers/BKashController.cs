using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NybSys.WASA.Account.BLL;
using NybSys.WASA.DTO;
using NybSys.WASA.ExceptionLogManger.BLL;

namespace NybSys.WASA.BkashApi.Controllers
{
    [Produces("application/json")]
    [Route("api")]
    public class BKashController : Controller
    {
        private readonly IExceptionLogBLLManager _exceptionLogBLLManager;
        private readonly IPaymentBLLManager _paymentBLLManager;
        private readonly IAccountManager _accountManager;

        public BKashController(IExceptionLogBLLManager exceptionLogBLLManager,
            IPaymentBLLManager paymentBLLManager, IAccountManager accountManager)
        {
            _exceptionLogBLLManager = exceptionLogBLLManager;
            _paymentBLLManager = paymentBLLManager;
            _accountManager = accountManager;
        }
        [HttpPost]
        public async Task<JsonResult> Post([FromForm] dynamic body)
        {
            try
            {
                string messageType = HttpContext.Request.Headers["x-amz-sns-message-type"].FirstOrDefault();

                string content = string.Empty;
                using (var reader = new StreamReader(Request.Body))
                {
                    content = await reader.ReadToEndAsync();
                }
                

                    BkashLog bkashLog = new BkashLog();
                    bkashLog.CreatedAt = DateTime.Now;
                    bkashLog.Payload = content;
                    await _paymentBLLManager.AddBkashPaymentLog(bkashLog);

                 return  new JsonResult(await _accountManager.BkashRecharge(content, messageType)) { StatusCode=(int)Common.Enums.StatusCode.Ok};

            }
            catch (Exception ex)
            {
                try
                {
                    await _exceptionLogBLLManager.AddExceptionLog(ex.Message, "api", "", (int)NybSys.WASA.Common.Enums.ExceptionType.Deshboard, (int)NybSys.WASA.Common.Enums.ActionName.Payment, (int)NybSys.WASA.Common.Enums.ActionType.Add);
                }
                catch (Exception)
                {


                }


                return new JsonResult(NybSys.WASA.Common.Constant.Message.ErrorMessages.INTERNAL_SERVER_PROBLEM) { StatusCode =NybSys.WASA.Common.Constant.StatusCode.INTERNAL_SERVER_ERROR };
            }

        }
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }
    }
}