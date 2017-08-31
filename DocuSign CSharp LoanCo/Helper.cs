using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuSign.eSign.Api;
using DocuSign.eSign.Model;
using System.Windows.Forms; //Needed for debugging with a MessageBox
using System.IO;
using System.Reflection;

namespace DocuSign
{
    static class Helper
    {

        public static string getLocalDocument(string filepath)
        {

            // create a byte array that will hold our document bytes
            String returnFile = "";
            try
            {
                //MessageBox.Show("filepath:" + filepath);

                // read file from a local directory
                Byte[] bytes = File.ReadAllBytes(filepath);
                returnFile = Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                // handle error
                return ex.Message;
            }
            return returnFile;
        }

        public static ViewUrl getRecipientUrl(EnvelopesApi envelopesApi, string accountId, EnvelopeDefinition envDef, string envelopeId, Signer recipient)
        {

            // set the url where you want the recipient to go once they are done signing
            // - this can be used by your app to watch the URL and detect when signing has completed (or was canceled) 
            var returnUrl = new RecipientViewRequest();

            //Get Config settings from App.config
            DocuSignConfig configSettings = new DocuSignConfig();
            returnUrl.ReturnUrl = configSettings.LOCAL_RETURN_URL + "pop/" + envelopeId;

            returnUrl.AuthenticationMethod = "email";

            // recipient information must match embedded recipient info we provided
            returnUrl.UserName = recipient.Name;
            returnUrl.Email = recipient.Email;
            returnUrl.RecipientId = recipient.RecipientId;
            returnUrl.ClientUserId = recipient.ClientUserId;

            RecipientViewRequest recipipentViewRequest = new RecipientViewRequest();
            ViewUrl viewUrl = new ViewUrl();
            viewUrl = envelopesApi.CreateRecipientView(accountId, envelopeId, returnUrl);
            return viewUrl;
        }

        public static ViewUrl getRecipientUrl(EnvelopesApi envelopesApi, string accountId, EnvelopeDefinition envDef, string envelopeId, TemplateRole templateRole)
        {

            // set the url where you want the recipient to go once they are done signing
            // - this can be used by your app to watch the URL and detect when signing has completed (or was canceled) 
            var returnUrl = new RecipientViewRequest();

            //Get Config settings from App.config
            DocuSignConfig configSettings = new DocuSignConfig();
            returnUrl.ReturnUrl = configSettings.LOCAL_RETURN_URL + "pop/" + envelopeId;

            returnUrl.AuthenticationMethod = "email";

            // recipient information must match embedded recipient info we provided
            returnUrl.UserName = templateRole.Name;
            returnUrl.Email = templateRole.Email;
            returnUrl.ClientUserId = templateRole.ClientUserId;

            RecipientViewRequest recipipentViewRequest = new RecipientViewRequest();
            ViewUrl viewUrl = new ViewUrl();
            viewUrl = envelopesApi.CreateRecipientView(accountId, envelopeId, returnUrl);
            return viewUrl;
        }

        public static bool IsArrayOf<T>(this Type type)
        {
            return type == typeof(T[]);
        }

        public static Signer addPhoneAuthToRecipient(Signer recipient, string phonenumber)
        {
            // Not enabled in demo
            recipient.RequireIdLookup = "true";
            recipient.IdCheckConfigurationName = "Phone Auth $";

            var phoneList = new List<string> { phonenumber };

            var phoneAuth = new RecipientPhoneAuthentication();
            phoneAuth.SenderProvidedNumbers = phoneList;
            phoneAuth.RecipMayProvideNumber = "true";
            phoneAuth.RecordVoicePrint = "true";

            recipient.PhoneAuthentication = phoneAuth;

            return recipient;
        }

        public static TemplateRole addPhoneAuthToRecipient(TemplateRole templateRole, string phonenumber)
        {

            var phoneList = new List<string> { phonenumber };

            var phoneAuth = new RecipientPhoneAuthentication();
            phoneAuth.SenderProvidedNumbers = phoneList;
            phoneAuth.RecipMayProvideNumber = "true";
            phoneAuth.RecordVoicePrint = "true";

            return templateRole;
        }

    }
}
