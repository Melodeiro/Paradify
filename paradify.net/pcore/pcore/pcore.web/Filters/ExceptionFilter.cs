using web.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace web.Filters
{
    public class CustomHandleError : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            try
            {
                SlackService slackService = new SlackService("https://hooks.slack.com/services/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");

                string errorMessageBody = string.Format("{0} - {1}", context.Exception.Message, context.Exception.StackTrace);
                slackService.PostMessage(errorMessageBody, "volkanakinpasa", "alerts");

            }
            catch
            {

            }
        }


    }
}