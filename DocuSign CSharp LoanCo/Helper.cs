using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuSign.eSign.Api;
using DocuSign.eSign.Model;
using System.Windows.Forms; //ATM Temp for MessageBox
using System.IO;
using System.Reflection;

namespace DocuSign
{
    static class Helper
    {

        //static string ReturnUrl { get; set; }

        //public DocuSign.eSign.Model.Tabs MakeTab(object type, object data)
        //{
        //    // https://docs.docusign.com/esign/restapi/Envelopes/EnvelopeTabs/

        //    // SignHere
        //    // Custom
        //    // FullName
        //    // InitialHere
        //    // InitialHereOptional
        //    // etc.

        //    tab = new docusign[type]();
        //    tab.constructFromObject(data);
        //    return tab;
        //}

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

            //ATM TODO: Local App Config
            //returnUrl.ReturnUrl = app.config.auth.LocalReturnUrl + "pop/" + envelopeId;
            returnUrl.ReturnUrl = "http://localhost:3801/" + "pop/" + envelopeId;
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

            //ATM TODO: Local App Config
            //returnUrl.ReturnUrl = app.config.auth.LocalReturnUrl + "pop/" + envelopeId;
            returnUrl.ReturnUrl = "http://localhost:3801/" + "pop/" + envelopeId;
            returnUrl.AuthenticationMethod = "email";

            // recipient information must match embedded recipient info we provided
            returnUrl.UserName = templateRole.Name;
            returnUrl.Email = templateRole.Email;
            //ATM TODO
            //returnUrl.RecipientId = templateRole.RecipientId;
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
            //ATM: Determine what properties to use for templateRole object. These are correct for the recipient object
            //but in node.js, doesn't give a compile error

            // Not enabled in demo
            //templateRole.RequireIdLookup = "true"; //ATM TO CHECK
            //templateRole.IdCheckConfigurationName = "Phone Auth $"; ATM TO CHECK

            var phoneList = new List<string> { phonenumber };

            var phoneAuth = new RecipientPhoneAuthentication();
            phoneAuth.SenderProvidedNumbers = phoneList;
            phoneAuth.RecipMayProvideNumber = "true";
            phoneAuth.RecordVoicePrint = "true";

            //templateRole.PhoneAuthentication = phoneAuth; //ATM TO CHECK

            return templateRole;
        }

    }
}
