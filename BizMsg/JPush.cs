using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using cn.jpush.api;
using cn.jpush.api.push;
using cn.jpush.api.report;
using cn.jpush.api.common;
using cn.jpush.api.util;
using cn.jpush.api.push.mode;
using cn.jpush.api.push.notification;
using cn.jpush.api.common.resp;
using DC;

namespace BizMsg
{
    public class JPush
    {
        public static String TITLE = "Test from C# v3 sdk";
        public static String ALERT = "Test from  C# v3 sdk - alert";
        public static String MSG_CONTENT = "Test from C# v3 sdk - msgContent";
        public static String REGISTRATION_ID = "0900e8d85ef";
        public static String TAG = "tag_api";
        public static String app_key = "98df3fcfc1935297332790b8";
        public static String master_secret = "3878006c9ea6084c5df8d24c";

        static JPushClient client = new JPushClient(app_key, master_secret);
        static object clientLocker = new object();
        
        static void Push(PushPayload payload)
        {
            //DCLogger.LogTrace("*****Start JPush.Push******");
            //try
            //{
                var result = client.SendPush(payload);
            //}
            //catch (APIRequestException e)
            //{
            //    DCLogger.LogError("Error response from JPush server. Should review and fix it. {0}  {1}  {2}", e.Status, e.ErrorCode, e.ErrorMessage);
            //}
            //catch (APIConnectionException e)
            //{
            //    DCLogger.LogError(e.Message);
            //}
            //DCLogger.LogTrace("*****End JPush.Push******");
        }

        public static void Push_all_alias_notification(string[] alias, string alert, Dictionary<string, object> extras)
        {

            PushPayload pushPayload = new PushPayload();
            pushPayload.platform = Platform.android_ios();

            pushPayload.audience = Audience.s_alias(alias);

            pushPayload.notification = new Notification();
            pushPayload.notification.AndroidNotification = new AndroidNotification().setAlert(alert).setBuilderID(1);
            pushPayload.notification.IosNotification = new IosNotification().setAlert(alert).incrBadge(1);
            if (extras != null)
            {
                foreach (string key in extras.Keys)
                {
                    pushPayload.notification.AndroidNotification.AddExtra(key, extras[key]);
                    pushPayload.notification.IosNotification.AddExtra(key, extras[key]);
                }
            }
            Push(pushPayload);
        }
        public static void Push_all_tag_notification(string[] tag, string alert, Dictionary<string, object> extras)
        {

            PushPayload pushPayload = new PushPayload();
            pushPayload.platform = Platform.android_ios();

            pushPayload.audience = Audience.s_tag(tag);

            pushPayload.notification = new Notification();
            pushPayload.notification.AndroidNotification = new AndroidNotification().setAlert(alert).setBuilderID(1);
            pushPayload.notification.IosNotification = new IosNotification().setAlert(alert).incrBadge(1);
            if (extras != null)
            {
                foreach (string key in extras.Keys)
                {
                    pushPayload.notification.AndroidNotification.AddExtra(key, extras[key]);
                    pushPayload.notification.IosNotification.AddExtra(key, extras[key]);
                }
            }
            Push(pushPayload);
        }
        public static void Push_all_alias_message(string[] alias, string message)
        {

            PushPayload pushPayload = new PushPayload();
            pushPayload.platform = Platform.android_ios();
            pushPayload.audience = Audience.s_alias(alias);
            pushPayload.message = Message.content(message);

            Push(pushPayload);
        }
        public static void Push_all_tag_message(string[] tag, string message)
        {

            PushPayload pushPayload = new PushPayload();
            pushPayload.platform = Platform.android_ios();
            pushPayload.audience = Audience.s_tag(tag);
            pushPayload.message = Message.content(message);

            Push(pushPayload);
        }
        public static void Push_all_alias_alert_message(string alert, string message, params string[] alias)
        {

            PushPayload pushPayload = new PushPayload();
            pushPayload.platform = Platform.android_ios();
            pushPayload.audience = Audience.s_alias(alias);
            pushPayload.notification = new Notification().setAlert(alert);
            pushPayload.message = Message.content(message);

            Push(pushPayload);
        }
        public static void Push_all_tag_alert_message(string alert, string message, params string[] tag)
        {

            PushPayload pushPayload = new PushPayload();
            pushPayload.platform = Platform.android_ios();
            pushPayload.audience = Audience.s_tag(tag);
            pushPayload.notification = new Notification().setAlert(alert);
            pushPayload.message = Message.content(message);

            Push(pushPayload);
        }
        public static PushPayload Push_All_All_Alert()
        {
            PushPayload pushPayload = new PushPayload();
            pushPayload.platform = Platform.all();
            pushPayload.audience = Audience.all();
            pushPayload.notification = new Notification().setAlert(ALERT);
            return pushPayload;
        }
        public static PushPayload Push_Android_Tag_AlertWithTitle()
        {
            PushPayload pushPayload = new PushPayload();

            pushPayload.platform = Platform.android();
            pushPayload.audience = Audience.s_tag("tag1");
            pushPayload.notification = Notification.android(ALERT, TITLE);

            return pushPayload;
        }
        public static PushPayload Push_android_and_ios()
        {
            PushPayload pushPayload = new PushPayload();
            pushPayload.platform = Platform.android_ios();
            var audience = Audience.s_tag("tag1");
            pushPayload.audience = audience;
            var notification = new Notification().setAlert("alert content");
            notification.AndroidNotification = new AndroidNotification().setTitle("Android Title");
            notification.IosNotification = new IosNotification();
            notification.IosNotification.incrBadge(1);
            notification.IosNotification.AddExtra("extra_key", "extra_value");

            pushPayload.notification = notification.Check();


            return pushPayload;
        }
        public static PushPayload Push_ios_tagAnd_alertWithExtrasAndMessage()
        {
            PushPayload pushPayload = new PushPayload();
            pushPayload.platform = Platform.android_ios();
            pushPayload.audience = Audience.s_tag_and("tag1", "tag_all");
            var notification = new Notification();
            notification.IosNotification = new IosNotification().setAlert(ALERT).setBadge(5).setSound("happy").AddExtra("from", "JPush");

            pushPayload.notification = notification;
            pushPayload.message = Message.content(MSG_CONTENT);
            return pushPayload;

        }
        public static PushPayload Push_ios_audienceMore_messageWithExtras()
        {

            var pushPayload = new PushPayload();
            pushPayload.platform = Platform.android_ios();
            pushPayload.audience = Audience.s_tag("tag1", "tag2");
            pushPayload.message = Message.content(MSG_CONTENT).AddExtras("from", "JPush");
            return pushPayload;

        }

    }
}
