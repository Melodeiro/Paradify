using log4net;
using System;
using System.Web.Mvc;
using web.Services;

namespace web.Filters
{
    public class CustomHandleError : HandleErrorAttribute
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(CustomHandleError));

       

        public override void OnException(ExceptionContext filterContext)
        {
            log.Error("", filterContext.Exception);

            try
            {
                SlackService slackService = new SlackService("https://hooks.slack.com/services/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");

                string errorMessageBody = string.Format("{0} - {1}", filterContext.Exception.Message, filterContext.Exception.StackTrace);
                slackService.PostMessage(errorMessageBody, "volkanakinpasa", "alerts");

            }
            catch (Exception exInner)
            {
                log.Error("Error during logging error", exInner);
            }
        }


    }
}