using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace EXPEDIT.Share.Helpers
{
    public class ConstantsHelper
    {
        public static Regex REGEX_JS_CLEANER = new Regex(@"function|;/ig", RegexOptions.Compiled);
        public static int WORKFLOW_INSTANCE_TIMEOUT_IDLE_SECONDS = 3600;
        public static int WORKFLOW_INSTANCE_TIMEOUT_EXECUTION_SECONDS = 3600;
        public static int WORKFLOW_INSTANCE_RESUME_ATTEMPTS_LEFT = 2;
        public static string REFERENCE_TYPE_PROJECTPLANTASKRESPONSE = "X_ProjectPlanTaskResponse";
        public static Guid WORKFLOW_APP_FLOWPRO = new Guid("71a2e288-6271-4b6b-9862-6de9f0749594");
        public static Guid PROJECT_TYPE_FLOWPRO = new Guid("0536822f-7fea-47b7-beb3-443a17af324c");
        public static Guid TRIGGER_TYPE_SEND_SMS = new Guid("70cdc18f-a3bd-4926-96cb-00d7a38b4071");
        public static Guid TRIGGER_TYPE_SEND_EMAIL = new Guid("caf50d10-0c7f-4fa9-948d-4224e4f1ea7a");
        public static Guid TRIGGER_TYPE_WORKFLOW_CANCEL = new Guid("0863c911-9098-4f4a-a72d-4b11779d523d");
        public static Guid TRIGGER_TYPE_WORKFLOW_INSTANTIATE = new Guid("ce40a5aa-b27b-496b-8c7e-798d13833044");
        public static Guid TRIGGER_TYPE_WORKFLOW_TRANSITION = new Guid("22bae812-e943-4844-9543-c6440a32bbff");
        public static Guid LOCATION_TYPE_UNCLASSIFIED = new Guid("dab2d152-398e-40f8-86ef-7431a20efa19");
        public static Guid WORK_TYPE_FEEDBACK_MODEL = new Guid("4E796115-62B0-441A-B29C-4652E7A1557C");
        public static Guid WORK_TYPE_SUPPORT_REGARDING = new Guid("57ebd6b8-980f-4116-90f5-574765c9766d");
        public static Guid WORK_TYPE_SUPPORT_STATUS = new Guid("72e99319-3a7b-4338-8d97-856678fe7e31");
        public static Guid WORK_TYPE_SUPPORT = new Guid("37EADA26-1CCC-4BCF-AFA9-4AA0D5946615");
        public static Guid WORK_TYPE_TICKET_CLOSED = new Guid("dfe87d2c-e577-4060-becb-ffbda6bee547");
        public static string DOCUMENT_TYPE_CONTENT_SUBMISSION = "Content Submission";
        public static string DOCUMENT_TYPE_SOFTWARE_SUBMISSION = "Software Submission";
        public static string DOCUMENT_TYPE_TICKET_SUBMISSION = "Ticket Submission";
        public static string DOCUMENT_TYPE_BASE64PNG = "Base64 PNG";    
        public static string DOCUMENT_TYPE_INVOICE = "Invoice";
        public static Guid COMPANY_DEFAULT = new Guid("6887ABC9-E2D8-4A2D-B143-6C3E5245C565");
        public static Guid ACCOUNT_TYPE_ONLINE = new Guid("5C329B8D-007D-435E-8261-4FA72D7DF28A");
        public static Guid DEVICE_TYPE_SOFTWARE = new Guid("3f526009-827a-41b0-a633-14b422bdf27f");
        public static Guid ROUTE_TYPE_STORE_INTERNAL = new Guid("1a01fc89-c014-433f-be04-39c2f956aeb2");
        public static Guid ROUTE_TYPE_STORE_EXTERNAL = new Guid("7c9f3a25-011b-4f5e-8b1e-d345da13f8b1");
        public static Guid UNIT_SI_SECONDS = new Guid("5AF72C77-A76E-4234-A16E-3F7898799EEA");
        public static Guid UNIT_SI_UNARY = new Guid("9D3C7BDA-AA32-44EB-9BCA-EFB86C65A2FA");
        public static Guid CONTRACT_PARTNER = new Guid("e8ed2f94-1100-43a2-90cd-206d228090e2");
        public static Guid PAYMENT_PROVIDER_DEFAULT = new Guid("5445D30E-BCD5-4F66-82C6-20CE53C47000");
        public static int SQL_MAX_INT = 2147483647;
        public static string STAT_NAME_DOWNLOADS = "Downloads";
        public static string STAT_NAME_ROUTES = "Routes";
        public static string STAT_NAME_CLICKS_BUY = "ClicksBuy";
        public static string STAT_NAME_REFERRAL = "Referral";
        public static string STAT_NAME_CHECKIN = "Checkin";
        public static string STAT_NAME_CLICKS_CONFIRM = "ClicksConfirm";
        public static string METADATA_ANTIFORGERY = "E_ANTIFORGERY";
        public static string METADATA_CAPTCHA = "E_CAPTCHA";
        public static string REFERENCE_TYPE_LABOUR = "E_LABOUR";
        public static string REFERENCE_TYPE_CONTRACT = "X_Contract";
        public const decimal GST_AU = 0.1m;
        public static decimal TAX_DEFAULT = GST_AU;
        public static string LICENSE_SERVER_AUTH_METHOD = "Simple";
        public static int DOWNLOADS_REMAINING_DEFAULT = 10;
        public static Guid FILE_TYPE_EXTERNAL= new Guid("a7d379b3-b4fe-40a1-bebd-19e3d61f3477");
        public static Guid FILE_TYPE_USER_GUIDE= new Guid("5f14ae60-6ca3-46e7-973e-1fb45e7b7362");
        public static Guid FILE_TYPE_SOFTWARE= new Guid("b2df1ccc-cd61-4d80-859a-40dd06b10e63");
        public static Guid FILE_TYPE_GLOBAL= new Guid("3e3ebe72-3a3d-4ad5-946b-52befb8b483f");
        public static Guid FILE_TYPE_INVOICE= new Guid("26c6a363-1a75-4530-ad44-cd18a47b69f1");
        public static Guid FILE_TYPE_INTERNAL= new Guid("b8491f60-8ab2-444d-8d88-e0a34cddeafe");        
        public static string ADDRESS_APP_OWNER =
                "EXPEDIT SOLUTIONS PTY LTD\r\n" + 
                "ABN 93152456374\r\n" +
                "80/120 Meiers Rd, Indooroopilly, Brisbane\r\n" +
                "QLD, 4068, Australia\r\n\r\n" +
                "P: +61733460727\r\n" +
                "E: accounts@expedit.com.au\r\n" +
                "U: http://expedit.com.au";
        public static string APP_OWNER = "MINING APPSTORE";
        public static string PDF_LOGO = @"EXPEDIT.Share\Images\pdfheader.jpg";
        public static string APP_VERIFY_PREFIX = @"Your Mining Appstore code is";
        public static string APP_VERIFY_ID = "80d249a9eecc9232fb6ed0f843e7f230";
        public static string APP_VERIFY_SECRET = "6239e342d7abb35c853ad33e65931f64";
        public static string APP_VERIFY_REPLYTO = "+61400970789";
        public static string APP_XMLRPC_URL = @"http://miningappstore.com/xmlrpc";

        public static string EMAIL_FOLDER_INBOX = "Inbox";
        public static string EMAIL_FOLDER_SUPPORT = "Support";
        public static string EMAIL_FOOTER =
              "<br/>" +
              "___________________________________________<br/>" +
              "Mining Appstore Ticketing System<br/>" +
              "E: help@support.miningappstore.com<br/>" +
              "U: http://miningappstore.com<br/>" +
              "<p><strong>Disclaimer</strong></p>" +
              "<div width=500px><p>Materials provided in this email are provided &quot;as is&quot;, without warranty of any kind, either express or implied, including, without limitation, warranties of merchantability, fitness for a particular purpose and non-infringement.</p></div>" +
              "<div width=500px><p>This email may contain advice, opinions and statements of various information providers. Mining Appstore does not represent or endorse the accuracy or reliability of any advice, opinion, statement or other information provided by any information provider, any User of this content or any other person or entity. Reliance upon any such advice, opinion, statement, or other information shall also be at the User&rsquo;s own risk.</p></div>"
              ;
        public static string EMAIL_FROM_NAME = "Mining Appstore";
        public static string FORM_BODY_TEMPLATE = "<h3>New form submission recieved:</h3> <table cellpadding='4'><tr><td><strong>Form name</strong></td><td id='formname'>{0}</td></tr> <tr><td><strong>Submitted by</strong></td><td id='updatedby'>{2}</td></tr> <tr><td><strong>Time</strong></td><td id='updated'>{3}</td></tr> <tr><td><strong>Form ID</strong></td><td id='formid'>{4}</td></tr></table>  <h4>Data:</h4> <table border='1' cellpadding='4' id='datatbl'>{1}</table><p></p>";

        private static string mailHost = null;
        public static string MailHost { get { if (mailHost == null) { mailHost = System.Configuration.ConfigurationManager.AppSettings["MailHost"] ?? "support.miningappstore.com"; } return mailHost; } }
        private static string mailUserEmail = null;
        public static string MailUserEmail { get { if (mailUserEmail == null) { mailUserEmail = System.Configuration.ConfigurationManager.AppSettings["MailUserEmail"] ?? @"help@support.miningappstore.com"; } return mailUserEmail; } }
        private static string mailPassword = null;
        public static string MailPassword { get { if (mailPassword == null) { mailPassword = System.Configuration.ConfigurationManager.AppSettings["MailPassword"] ?? @"F5C6DA7CF26F45D4A3F2DDDADE5BD256"; } return mailPassword; } }
        private static int? mailPort = null;
        public static int MailPort
        {
            get
            {
                if (!mailPort.HasValue)
                {
                    int temp;
                    if (!int.TryParse(System.Configuration.ConfigurationManager.AppSettings["MailPort"], out temp))
                        mailPort = 993;
                    else
                        mailPort = temp;
                }
                return mailPort.Value;
            }
        }

        private static string mailSuffix = null;
        public static string MailSuffix
        {
            get
            {
                if (mailSuffix == null)
                {
                    if (MailUserEmail != null)
                    {
                        int temp = MailUserEmail.IndexOf('@');
                        mailSuffix = mailUserEmail.Substring(temp);
                    }
                    else
                    {
                        mailSuffix = "@support.miningappstore.com";
                    }
                }
                return mailSuffix;
            }
        }

        private static bool _productCategoriesChecked = false;
        private static string _productCategories = null;
        public static string ProductCategories
        {
            get
            {
                if (!_productCategoriesChecked)
                {
                    _productCategories = string.Format("{0}", System.Configuration.ConfigurationManager.AppSettings["ProductCategories"]);
                    if (string.IsNullOrWhiteSpace(_productCategories))
                        _productCategories = null;
                    _productCategoriesChecked = true;
                }
                return _productCategories;
            }
        }

    }
}