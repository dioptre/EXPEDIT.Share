using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EXPEDIT.Share.Helpers
{
    public class ConstantsHelper
    {
        public static Guid ACCOUNT_TYPE_ONLINE = new Guid("5C329B8D-007D-435E-8261-4FA72D7DF28A");
        public static Guid DEVICE_TYPE_SOFTWARE = new Guid("3f526009-827a-41b0-a633-14b422bdf27f");
        public static Guid ROUTE_TYPE_STORE_INTERNAL = new Guid("1a01fc89-c014-433f-be04-39c2f956aeb2");
        public static Guid ROUTE_TYPE_STORE_EXTERNAL = new Guid("7c9f3a25-011b-4f5e-8b1e-d345da13f8b1");
        public static Guid UNIT_SI_SECONDS = new Guid("5AF72C77-A76E-4234-A16E-3F7898799EEA"); 
        public static int SQL_MAX_INT = 2147483647;
        public static string STAT_NAME_DOWNLOADS = "Downloads";
        public static string STAT_NAME_CLICKS_BUY = "ClicksBuy";
        public static string STAT_NAME_CLICKS_CONFIRM = "ClicksConfirm";
        public static string METADATA_ANTIFORGERY = "E_ANTIFORGERY";
        public static string REFERENCE_TYPE_LABOUR = "E_LABOUR";
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
                "EXPEDIT SOLUTIONS PTY LTD - MiningAppstore\r\n" +
                "ABN 93152456374\r\n" +
                "3 Fincastle Street, Moorooka, Brisbane\r\n" +
                "QLD, 4105 Australia\r\n\r\n" +
                "P: +61733460727\r\n" +
                "E: accounts@miningappstore.com\r\n" +
                "U: http://miningappstore.com";
        public static string APP_OWNER = "MINING APPSTORE";
        public static string PDF_LOGO = @"EXPEDIT.Share\Images\pdfheader.jpg";        

    }
}