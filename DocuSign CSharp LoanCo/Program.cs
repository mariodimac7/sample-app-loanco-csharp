using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuSign.eSign.Api;
using DocuSign.eSign.Model;
using DocuSign.eSign.Client;
using System.IO;
using EdgeJs;
using System.Windows.Forms; //For troubleshooting with MessageBoxes
using System.Configuration;
using System.Reflection;

namespace DocuSign
{
    public class Startup
    {
        #region PeronalLoan
        public async Task<object> InvokePersonalLoan(object[] reqArray)
        {
            string accountId;

            accountId = InitializeDocuSignAPI();
            List<object> rtn = (List<object>)CreateEnvelope_PersonalLoan(reqArray, accountId);
            return rtn;
        }
        public static object CreateEnvelope_PersonalLoan(object[] reqArray, string accountId)
        {
            try
            {
                //list to store all objects that will be returned to Node.js
                List<object> rtn = new List<object>();

                object nodeReqBody = reqArray[0];       //extract req.body from array passed in
                object nodeReqSession = reqArray[1];    //extract req.session from array passed in
                string nodeCurrentPath = reqArray[2].ToString();    //extract current path from array passed in

                //create dictionary from Node object and extract object values from request body
                var bodyDictionary = (IDictionary<string, object>)nodeReqBody;
                var body = new Body()
                {
                    inputSigningLocation = (string)bodyDictionary["inputSigningLocation"],
                    inputEmail = (string)bodyDictionary["inputEmail"],
                    inputFirstName = (string)bodyDictionary["inputFirstName"],
                    inputLastName = (string)bodyDictionary["inputLastName"],
                    inputPhone = (string)bodyDictionary["inputPhone"],
                    inputAddress1 = (string)bodyDictionary["inputAddress1"],
                    inputAddress2 = (string)bodyDictionary["inputAddress2"],
                    inputCity = (string)bodyDictionary["inputCity"],
                    inputState = (string)bodyDictionary["inputState"],
                    inputZip = (string)bodyDictionary["inputZip"],

                    //the fields below may not exist (depending on the UI options selected, so test to see if they are in the dictionary first
                    inputLoanAmount = bodyDictionary.ContainsKey("inputLoanAmount") ? (string)bodyDictionary["inputLoanAmount"] : "",
                    inputLoanLength = bodyDictionary.ContainsKey("inputLoanLength") ? (string)bodyDictionary["inputLoanLength"] : "",
                    inputAccessCode = bodyDictionary.ContainsKey("inputAccessCode") ? (string)bodyDictionary["inputAccessCode"] : "",
                    inputAuthentication = bodyDictionary.ContainsKey("inputAuthentication") ? (string)bodyDictionary["inputAuthentication"] : ""
                };

                // create a new envelope which we will use to send the signature request
                EnvelopeDefinition envDef = new EnvelopeDefinition();
                envDef.EmailSubject = "Personal Loan Application";
                envDef.EmailBlurb = "Please sign the Loan application to start the application process.";

                // add a document to the envelope
                var doc = new Document();
                byte[] fileBytes = File.ReadAllBytes(nodeCurrentPath + "/pdfs/LoanPersonal.docx");
                doc.DocumentBase64 = System.Convert.ToBase64String(fileBytes);
                doc.Name = "Document"; // can be different from actual file name
                doc.FileExtension = "docx";
                doc.DocumentId = "1"; // hardcode so we can easily refer to this document later

                //add document to envelope
                envDef.Documents = new List<Document>();
                envDef.Documents.Add(doc);

                // Recipient
                Signer signer = new Signer();
                signer.Email = body.inputEmail;
                signer.Name = body.inputFirstName + " " + body.inputLastName;
                signer.RecipientId = "1";
                if (body.inputSigningLocation == "embedded")
                {
                    signer.ClientUserId = "1001";
                }

                // Note: using anchorStrings (in tabs below) makes documentId and pageNumber irrelevant (they affect all documents and pages)
                signer.Tabs = new Tabs();

                // FullName
                signer.Tabs.FullNameTabs = new List<FullName>();
                FullName fullName = new FullName();
                fullName.RecipientId = "1";
                fullName.AnchorString = "Name";
                fullName.AnchorXOffset = "58";
                fullName.AnchorYOffset = "-2";
                fullName.TemplateLocked = "false";
                signer.Tabs.FullNameTabs.Add(fullName);

                // Email
                signer.Tabs.EmailTabs = new List<Email>();
                Email eMail = new Email();
                eMail.RecipientId = "1";
                eMail.Name = "Email";
                eMail.TabLabel = "Email";
                eMail.AnchorString = "Email";
                eMail.AnchorXOffset = "55";
                eMail.AnchorYOffset = "-2";
                eMail.Value = body.inputEmail;
                signer.Tabs.EmailTabs.Add(eMail);

                // Phone
                signer.Tabs.TextTabs = new List<Text>();
                Text text1 = new Text();
                text1.RecipientId = "1";
                text1.Name = "Phone";
                text1.TabLabel = "Phone";
                text1.AnchorString = "Phone";
                text1.AnchorXOffset = "65";
                text1.AnchorYOffset = "-2";
                text1.Value = body.inputPhone;
                text1.Locked = "false";
                signer.Tabs.TextTabs.Add(text1);


                // Address Line 1
                //signer.Tabs.TextTabs = new List<Text>();
                Text text2 = new Text();
                text2.RecipientId = "1";
                text2.Name = "AddressLine1";
                text2.TabLabel = "AddressLine1";
                text2.AnchorString = "Address";
                text2.AnchorXOffset = "80";
                text2.AnchorYOffset = "-2";
                text2.Value = body.inputAddress1;
                text2.Locked = "false";
                signer.Tabs.TextTabs.Add(text2);

                // Address Line 2
                //signer.Tabs.TextTabs = new List<Text>();
                Text text3 = new Text();
                text3.RecipientId = "1";
                text3.Name = "AddressLine2";
                text3.TabLabel = "AddressLine2";
                text3.AnchorString = "Address";
                text3.AnchorXOffset = "80";
                text3.AnchorYOffset = "20";
                text3.Value = body.inputAddress2;
                text3.Required = "false";
                text3.Locked = "false";
                signer.Tabs.TextTabs.Add(text3);

                // Address city/state/zip
                //signer.Tabs.TextTabs = new List<Text>();
                Text text4 = new Text();
                text4.RecipientId = "1";
                text4.Name = "AddressCityStateZip";
                text4.TabLabel = "AddressCityStateZip";
                text4.AnchorString = "Address";
                text4.AnchorXOffset = "80";
                text4.AnchorYOffset = "40";
                text4.Value = body.inputCity + ", " + body.inputState + " " + body.inputZip;
                text4.Locked = "false";
                signer.Tabs.TextTabs.Add(text4);

                // Amount
                signer.Tabs.NumberTabs = new List<Number>();
                Number number1 = new Number();
                number1.RecipientId = "1";
                number1.Name = "Amount";
                number1.TabLabel = "Amount";
                number1.AnchorString = "Amount";
                number1.AnchorXOffset = "75";
                number1.AnchorYOffset = "-2";
                number1.Locked = "false";
                number1.Value = body.inputLoanAmount;
                signer.Tabs.NumberTabs.Add(number1);

                // Payment payback period (months) 
                //                signer.Tabs.NumberTabs = new List<Number>();
                Number number2 = new Number();
                number2.RecipientId = "1";
                number2.Name = "PaymentDuration";
                number2.TabLabel = "PaymentDuration";
                number2.AnchorString = "Payment Duration";
                number2.AnchorXOffset = "150";
                number2.AnchorYOffset = "-2";
                number2.Locked = "false";
                number2.Value = body.inputLoanLength;
                signer.Tabs.NumberTabs.Add(number2);

                // Monthly payments (calculated field)
                signer.Tabs.FormulaTabs = new List<FormulaTab>();
                FormulaTab formula = new FormulaTab();
                formula.RecipientId = "1";
                formula.Name = "MonthlyPayment";
                formula.TabLabel = "MonthlyPayment";
                formula.AnchorString = "Monthly Payment";
                formula.AnchorXOffset = "180";
                formula.AnchorYOffset = "-2";
                formula.Formula = "[Amount]/[PaymentDuration]";
                signer.Tabs.FormulaTabs.Add(formula);

                // SignHere
                signer.Tabs.SignHereTabs = new List<SignHere>();
                SignHere signHere = new SignHere();
                signHere.RecipientId = "1";
                signHere.AnchorString = "DocuSign API rocks";
                signHere.AnchorXOffset = "10";
                signHere.AnchorYOffset = "60";
                signer.Tabs.SignHereTabs.Add(signHere);

                // add recipients (in this case a single signer) to the envelope
                envDef.Recipients = new Recipients();
                envDef.Recipients.Signers = new List<Signer>();
                envDef.Recipients.Signers.Add(signer);

                if (body.inputSigningLocation == "embedded")
                {
                    signer.ClientUserId = "1001";
                }

                if (body.inputAuthentication == "phone")
                {
                    Helper.addPhoneAuthToRecipient(signer, body.inputPhone);
                }

                if (body.inputAccessCode != "" && body.inputAccessCode.Length > 0)
                {
                    signer.AccessCode = body.inputAccessCode;
                }

                // set envelope status to "sent" to immediately send the signature request
                envDef.Status = "sent";

                EnvelopesApi envelopesApi = new EnvelopesApi();
                EnvelopeSummary envelopeSummary = envelopesApi.CreateEnvelope(accountId, envDef);

                rtn.Add(envelopeSummary);   //Index 0: add envelopeSummary to the return object
                if (body.inputSigningLocation == "embedded")
                {
                    ViewUrl viewUrl = Helper.getRecipientUrl(envelopesApi, accountId, envDef, envelopeSummary.EnvelopeId.ToString(), signer);
                    rtn.Add(viewUrl);   //Index 1: add viewURL to the return object
                }
                return rtn;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw ex;
            }
        }
        #endregion

        #region AutoLoan
        public async Task<object> InvokeAutoLoan(object[] reqArray)
        {
            string accountId;
            accountId = InitializeDocuSignAPI();
            List<object> rtn = (List<object>)CreateEnvelope_AutoLoan(reqArray, accountId);
            return rtn;
        }
        public static object CreateEnvelope_AutoLoan(object[] reqArray, string accountId)
        {
            try
            {
                //list to store all objects that will be returned to Node.js
                List<object> rtn = new List<object>();

                object nodeReqBody = reqArray[0];       //extract req.body from array passed in
                object nodeReqSession = reqArray[1];    //extract req.session from array passed in
                string nodeCurrentPath = reqArray[2].ToString();    //extract current path from array passed in

                //create dictionary from Node object and extract object values from request body
                var bodyDictionary = (IDictionary<string, object>)nodeReqBody;
                var body = new Body()
                {
                    inputSigningLocation = (string)bodyDictionary["inputSigningLocation"],
                    inputEmail = (string)bodyDictionary["inputEmail"],
                    inputFirstName = (string)bodyDictionary["inputFirstName"],
                    inputLastName = (string)bodyDictionary["inputLastName"],
                    inputPhone = (string)bodyDictionary["inputPhone"],

                    //the fields below may not exist (depending on the UI options selected, so test to see if they are in the dictionary first
                    inputLoanAmount = bodyDictionary.ContainsKey("inputLoanAmount") ? (string)bodyDictionary["inputLoanAmount"] : "",
                    inputLoanLength = bodyDictionary.ContainsKey("inputLoanLength") ? (string)bodyDictionary["inputLoanLength"] : "",
                    inputAccessCode = bodyDictionary.ContainsKey("inputAccessCode") ? (string)bodyDictionary["inputAccessCode"] : "",
                    inputAuthentication = bodyDictionary.ContainsKey("inputAuthentication") ? (string)bodyDictionary["inputAuthentication"] : "",
                    inputCosignerCheckbox = bodyDictionary.ContainsKey("inputCosignerCheckbox") ? (string)bodyDictionary["inputCosignerCheckbox"] : "",
                    inputCosignerFirstName = bodyDictionary.ContainsKey("inputCosignerFirstName") ? (string)bodyDictionary["inputCosignerFirstName"] : "",
                    inputCosignerLastName = bodyDictionary.ContainsKey("inputCosignerLastName") ? (string)bodyDictionary["inputCosignerLastName"] : "",
                    inputCosignerEmail = bodyDictionary.ContainsKey("inputCosignerEmail") ? (string)bodyDictionary["inputCosignerEmail"] : "",
                    inputSigningLocationCosigner = bodyDictionary.ContainsKey("inputSigningLocationCosigner") ? (string)bodyDictionary["inputSigningLocationCosigner"] : "",
                    inputAccessCodeCosigner = bodyDictionary.ContainsKey("inputAccessCodeCosigner") ? (string)bodyDictionary["inputAccessCodeCosigner"] : "",
                    inputAuthenticationCosigner = bodyDictionary.ContainsKey("inputAuthenticationCosigner") ? (string)bodyDictionary["inputAuthenticationCosigner"] : "",
                    inputCosignerPhone = bodyDictionary.ContainsKey("inputCosignerPhone") ? (string)bodyDictionary["inputCosignerPhone"] : "",
                };

                // create a new envelope which we will use to send the signature request
                EnvelopeDefinition envDef = new EnvelopeDefinition();
                envDef.EmailSubject = "Auto Loan Application";
                envDef.EmailBlurb = "Please sign the Loan application to start the application process.";
                //envDef.TemplateId = app.config.templatesByKey.cosigner_on_auto_loan.id;
                //ATM: Don't hard code - get this working from the /PDFs/template-auto-loan.json file
                envDef.TemplateId = "2edae0a2-045a-48cd-9117-41d18b601369";
                //TODO: Ask Nick why this is in the json file vs. a key in the .env file

                // create a template role with a valid templateId and roleName and assign signer info
                var tRoleApplicant = new TemplateRole();
                tRoleApplicant.RoleName = "applicant";
                tRoleApplicant.Name = body.inputFirstName + " " + body.inputLastName;
                tRoleApplicant.Email = body.inputEmail;
                if (body.inputSigningLocation == "embedded")
                {
                    tRoleApplicant.ClientUserId = "1001";
                }
                if (body.inputAccessCode != "" && body.inputAccessCode.Length > 0)
                {
                    tRoleApplicant.AccessCode = body.inputAccessCode;
                }
                if (body.inputAuthentication == "phone")
                {
                    Helper.addPhoneAuthToRecipient(tRoleApplicant, body.inputPhone);
                }

                tRoleApplicant.Tabs = new Tabs();

                // Phone
                tRoleApplicant.Tabs.TextTabs = new List<Text>();
                Text text1Applicant = new Text();
                text1Applicant.TabLabel = "Phone";
                text1Applicant.Value = body.inputPhone;
                tRoleApplicant.Tabs.TextTabs.Add(text1Applicant);

                // Amount
                tRoleApplicant.Tabs.NumberTabs = new List<Number>();
                Number number1Applicant = new Number();
                number1Applicant.TabLabel = "Amount";
                number1Applicant.Value = body.inputLoanAmount;
                tRoleApplicant.Tabs.NumberTabs.Add(number1Applicant);

                // Payment payback period (months) 
                //                signer.Tabs.NumberTabs = new List<Number>();
                Number number2Applicant = new Number();
                number2Applicant.TabLabel = "PaymentDuration";
                number2Applicant.Value = body.inputLoanLength;
                tRoleApplicant.Tabs.NumberTabs.Add(number2Applicant);


                var tRoleCosigner = new TemplateRole();
                tRoleCosigner.RoleName = "cosigner";
                tRoleCosigner.Name = body.inputCosignerFirstName + " " + body.inputCosignerLastName;
                tRoleCosigner.Email = body.inputCosignerEmail;
                if (body.inputSigningLocationCosigner == "embedded")
                {
                    tRoleCosigner.ClientUserId = "2002";
                }
                if (body.inputAccessCodeCosigner != "" && body.inputAccessCodeCosigner.Length > 0)
                {
                    tRoleCosigner.AccessCode = body.inputAccessCodeCosigner;
                }
                if (body.inputAuthenticationCosigner == "phone")
                {
                    Helper.addPhoneAuthToRecipient(tRoleCosigner, body.inputCosignerPhone);
                }

                tRoleCosigner.Tabs = new Tabs();

                // Phone
                tRoleCosigner.Tabs.TextTabs = new List<Text>();
                Text text1Cosigner = new Text();
                text1Cosigner.TabLabel = "Phone";
                text1Cosigner.Value = body.inputCosignerPhone;
                tRoleCosigner.Tabs.TextTabs.Add(text1Cosigner);

                var tRoleEmployee = new TemplateRole();
                tRoleEmployee.RoleName = "employee";

                //Get Config settings from App.config
                DocuSignConfig configSettings = new DocuSignConfig();
                tRoleEmployee.Name = configSettings.EMPLOYEE_NAME;
                tRoleEmployee.Email = configSettings.EMPLOYEE_EMAIL;

                // set envelope status to "sent" to immediately send the signature request
                envDef.Status = "sent";

                List<TemplateRole> rolesList = new List<TemplateRole>() { tRoleApplicant, tRoleEmployee };

                //add the cosigner if the checkbox is selected on the web page
                if (body.inputCosignerCheckbox == "on")
                    rolesList.Add(tRoleCosigner);

                envDef.TemplateRoles = rolesList;

                EnvelopesApi envelopesApi = new EnvelopesApi();
                EnvelopeSummary envelopeSummary = envelopesApi.CreateEnvelope(accountId, envDef);

                rtn.Add(envelopeSummary);   //Index 0: add envelopeSummary to the return object
                if (body.inputSigningLocation == "embedded")
                {
                    ViewUrl viewUrl = Helper.getRecipientUrl(envelopesApi, accountId, envDef, envelopeSummary.EnvelopeId.ToString(), tRoleApplicant);
                    rtn.Add(viewUrl);   //Index 1: add viewURL to the return object
                }
                rtn.Add(tRoleApplicant);    //Index 2: add tRoleApplicant to the return object
                rtn.Add(tRoleCosigner);    //Index 3: add tRoleCosigner to the return object
                return rtn;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw ex;
            }
        }
        #endregion

        #region SailboatLoan
        public async Task<object> InvokeSailboatLoan(object[] reqArray)
        {
            string accountId;
            accountId = InitializeDocuSignAPI();
            List<object> rtn = (List<object>)CreateEnvelope_SailboatLoan(reqArray, accountId);
            return rtn;
        }
        public static object CreateEnvelope_SailboatLoan(object[] reqArray, string accountId)
        {
            try
            {
                //list to store all objects that will be returned to Node.js
                List<object> rtn = new List<object>();

                object nodeReqBody = reqArray[0];       //extract req.body from array passed in
                object nodeReqSession = reqArray[1];    //extract req.session from array passed in
                string nodeCurrentPath = reqArray[2].ToString();    //extract current path from array passed in
                string gmapBase64Doc = reqArray[3].ToString();    //extract gmapBase64Doc from array passed in

                //create dictionary from Node object and extract object values from request body
                var bodyDictionary = (IDictionary<string, object>)nodeReqBody;
                var body = new Body()
                {
                    inputEmail = (string)bodyDictionary["inputEmail"],
                    inputFirstName = (string)bodyDictionary["inputFirstName"],
                    inputLastName = (string)bodyDictionary["inputLastName"],
                    inputAppraiserEmail = (string)bodyDictionary["inputAppraiserEmail"],
                    inputAppraiserFirstName = (string)bodyDictionary["inputAppraiserFirstName"],
                    inputAppraiserLastName = (string)bodyDictionary["inputAppraiserLastName"],
                    inputSigningLocation = (string)bodyDictionary["inputSigningLocation"],
                    inputSigningLocationAppraiser = (string)bodyDictionary["inputSigningLocationAppraiser"],

                    //the fields below may not exist (depending on the UI options selected, so test to see if they are in the dictionary first
                    inputAccessCode = bodyDictionary.ContainsKey("inputAccessCode") ? (string)bodyDictionary["inputAccessCode"] : "",
                    inputAccessCodeAppraiser = bodyDictionary.ContainsKey("inputAccessCodeAppraiser") ? (string)bodyDictionary["inputAccessCodeAppraiser"] : "",
                    inputAuthentication = bodyDictionary.ContainsKey("inputAuthentication") ? (string)bodyDictionary["inputAuthentication"] : "",
                    inputAuthenticationAppraiser = bodyDictionary.ContainsKey("inputAuthenticationAppraiser") ? (string)bodyDictionary["inputAuthenticationAppraiser"] : "",
                    inputPhone = bodyDictionary.ContainsKey("inputPhone") ? (string)bodyDictionary["inputPhone"] : "",
                    inputAppraiserPhone = bodyDictionary.ContainsKey("inputAppraiserPhone") ? (string)bodyDictionary["inputAppraiserPhone"] : ""
                };

                var file1Base64 = Helper.getLocalDocument(nodeCurrentPath + "/pdfs/LoanSailboat.docx");
                var file2Base64 = Helper.getLocalDocument(nodeCurrentPath + "/pdfs/LoanSailboatAppraiser.docx");

                // create an envelope that will store the document(s), field(s), and recipient(s)
                EnvelopeDefinition envDef = new EnvelopeDefinition();
                envDef.EmailSubject = "Sailboat Loan Application";
                envDef.EmailBlurb = "Please sign the Loan application to start the application process.";

                // add documents to the envelope
                Document doc1 = new Document();
                doc1.DocumentBase64 = file1Base64;
                doc1.Name = "Application"; // can be different from actual file name
                doc1.FileExtension = "docx";
                doc1.DocumentId = "1"; // hardcode so we can easily refer to this document later

                Document doc2 = new Document();
                //                MessageBox.Show(gmapBase64Doc);
                if (gmapBase64Doc != "")
                {
                    doc2.DocumentBase64 = gmapBase64Doc;
                    doc2.Name = "Map"; // can be different from actual file name
                    doc2.FileExtension = "png";
                    doc2.DocumentId = "2"; // hardcode so we can easily refer to this document later
                }

                Document doc3 = new Document();
                doc3.DocumentBase64 = file2Base64;
                doc3.Name = "Appraiser"; // can be different from actual file name
                doc3.FileExtension = "docx";
                doc3.DocumentId = "3"; // hardcode so we can easily refer to this document later

                envDef.Documents = new List<Document>();
                envDef.Documents.Add(doc1);
                if (gmapBase64Doc != "")
                {
                    envDef.Documents.Add(doc2);
                }
                envDef.Documents.Add(doc3);
                envDef.EnforceSignerVisibility = "true";

                // Recipients
                Signer signer = new Signer();
                signer.RoutingOrder = "1";
                signer.Email = body.inputEmail;
                signer.Name = body.inputFirstName + " " + body.inputLastName;
                signer.RecipientId = "1";
                signer.ExcludedDocuments = new List<String>();
                signer.ExcludedDocuments.Add("3");

                if (body.inputSigningLocation == "embedded")
                {
                    signer.ClientUserId = "1001";
                }
                if (body.inputAccessCode != "" && body.inputAccessCode.Length > 0)
                {
                    signer.AccessCode = body.inputAccessCode;
                }
                if (body.inputAuthentication == "phone")
                {
                    Helper.addPhoneAuthToRecipient(signer, body.inputPhone);
                }

                Signer appraiserSigner = new Signer();
                appraiserSigner.RoutingOrder = "2";
                appraiserSigner.Email = body.inputAppraiserEmail;
                appraiserSigner.Name = body.inputAppraiserFirstName + " " + body.inputAppraiserLastName;
                appraiserSigner.RecipientId = "2";

                if (body.inputSigningLocationAppraiser == "embedded")
                {
                    appraiserSigner.ClientUserId = "2002";
                }
                if (body.inputAccessCodeAppraiser != "" && body.inputAccessCodeAppraiser.Length > 0)
                {
                    appraiserSigner.AccessCode = body.inputAccessCodeAppraiser;
                }
                if (body.inputAuthenticationAppraiser == "phone")
                {
                    Helper.addPhoneAuthToRecipient(appraiserSigner, body.inputAppraiserPhone);
                }

                // Signer Tabs
                signer.Tabs = new Tabs();
                signer.Tabs.TextTabs = new List<Text>();
                signer.Tabs.NumberTabs = new List<Number>();
                signer.Tabs.FormulaTabs = new List<FormulaTab>();
                signer.Tabs.EmailTabs = new List<Email>();
                signer.Tabs.FullNameTabs = new List<FullName>();
                signer.Tabs.SignerAttachmentTabs = new List<SignerAttachment>();
                signer.Tabs.SignHereTabs = new List<SignHere>();
                signer.Tabs.InitialHereTabs = new List<InitialHere>();
                signer.Tabs.DateSignedTabs = new List<DateSigned>();

                // Note: using anchorStrings (in tabs below) makes documentId and pageNumber irrelevant (they affect all documents and pages)

                // Email
                Email signerEmail = new Email();
                signerEmail.RecipientId = "1";
                signerEmail.AnchorString = "Applicant Email";
                signerEmail.AnchorXOffset = "0";
                signerEmail.AnchorYOffset = "0";
                signerEmail.Value = body.inputEmail;
                signer.Tabs.EmailTabs.Add(signerEmail);

                // FullName
                FullName signerFullName = new FullName();
                signerFullName.RecipientId = "1";
                signerFullName.AnchorString = "Applicant Full Name";
                signerFullName.AnchorXOffset = "0";
                signerFullName.AnchorYOffset = "0";
                signer.Tabs.FullNameTabs.Add(signerFullName);


                // Attachment
                SignerAttachment signerAttachment = new SignerAttachment();
                signerAttachment.RecipientId = "1";
                signerAttachment.AnchorString = "Please attach";
                signerAttachment.AnchorXOffset = "0";
                signerAttachment.AnchorYOffset = "40";
                signerAttachment.Optional = "true";
                signer.Tabs.SignerAttachmentTabs.Add(signerAttachment);

                // SignHere
                SignHere signerSignHere = new SignHere();
                signerSignHere.RecipientId = "1";
                signerSignHere.AnchorString = "Applicant Signature";
                signerSignHere.AnchorXOffset = "0";
                signerSignHere.AnchorYOffset = "4";
                signer.Tabs.SignHereTabs.Add(signerSignHere);

                // InitialHere
                InitialHere signerInitialHere = new InitialHere();
                signerInitialHere.RecipientId = "1";
                signerInitialHere.AnchorString = "Applicant Initial";
                signerInitialHere.AnchorXOffset = "0";
                signerInitialHere.AnchorYOffset = "0";
                signer.Tabs.InitialHereTabs.Add(signerInitialHere);

                // Appraiser Tabs
                appraiserSigner.Tabs = new Tabs();
                appraiserSigner.Tabs.TextTabs = new List<Text>();
                appraiserSigner.Tabs.EmailTabs = new List<Email>();
                appraiserSigner.Tabs.FullNameTabs = new List<FullName>();
                appraiserSigner.Tabs.SignHereTabs = new List<SignHere>();
                appraiserSigner.Tabs.InitialHereTabs = new List<InitialHere>();
                appraiserSigner.Tabs.DateSignedTabs = new List<DateSigned>();
                appraiserSigner.Tabs.FormulaTabs = new List<FormulaTab>();
                appraiserSigner.Tabs.SignerAttachmentTabs = new List<SignerAttachment>();
                appraiserSigner.Tabs.NumberTabs = new List<Number>();

                // Email
                Email appraiserSignerEmail = new Email();
                appraiserSignerEmail.RecipientId = "2";
                appraiserSignerEmail.AnchorString = "Appraiser Email";
                appraiserSignerEmail.AnchorXOffset = "0";
                appraiserSignerEmail.AnchorYOffset = "0";
                appraiserSignerEmail.Value = body.inputAppraiserEmail;
                appraiserSigner.Tabs.EmailTabs.Add(appraiserSignerEmail);

                // FullName
                FullName appraiserSignerFullName = new FullName();
                appraiserSignerFullName.RecipientId = "2";
                appraiserSignerFullName.AnchorString = "Appraiser Full Name";
                appraiserSignerFullName.AnchorXOffset = "0";
                appraiserSignerFullName.AnchorYOffset = "0";
                appraiserSigner.Tabs.FullNameTabs.Add(appraiserSignerFullName);

                // Appraisal amount
                Number appraiserSignerAmount = new Number();
                appraiserSignerAmount.RecipientId = "2";
                appraiserSignerAmount.AnchorString = "Appraiser Estimate";
                appraiserSignerAmount.AnchorXOffset = "0";
                appraiserSignerAmount.AnchorYOffset = "0";
                appraiserSignerAmount.Locked = "false";
                appraiserSigner.Tabs.NumberTabs.Add(appraiserSignerAmount);

                // SignHere
                SignHere appraisalSignerSignHere = new SignHere();
                appraisalSignerSignHere.RecipientId = "2";
                appraisalSignerSignHere.AnchorString = "Appraiser Signature";
                appraisalSignerSignHere.AnchorXOffset = "0";
                appraisalSignerSignHere.AnchorYOffset = "4";
                appraiserSigner.Tabs.SignHereTabs.Add(appraisalSignerSignHere);

                // BLANK TEXT (on first document, to make it visible to our Appraiser) 
                Text appraisalSignerText = new Text();
                appraisalSignerText.RecipientId = "2";
                appraisalSignerText.DocumentId = "1";
                appraisalSignerText.PageNumber = "1";
                appraisalSignerText.XPosition = "0";
                appraisalSignerText.YPosition = "0";
                appraisalSignerText.Value = "";
                appraisalSignerText.Locked = "true";
                appraiserSigner.Tabs.TextTabs.Add(appraisalSignerText);

                // add recipients
                envDef.Recipients = new Recipients();
                envDef.Recipients.Signers = new List<Signer>();
                envDef.Recipients.Signers.Add(signer);
                envDef.Recipients.Signers.Add(appraiserSigner);

                // send the envelope by setting |status| to "sent". To save as a draft set to "created"
                // - note that the envelope will only be 'sent' when it reaches the DocuSign server with the 'sent' status (not in the following call)
                envDef.Status = "sent";

                //Get Config settings from App.config
                DocuSignConfig configSettings = new DocuSignConfig();

                if (configSettings.BRAND_ID != "" && configSettings.BRAND_ID.Length > 0)
                {
                    envDef.BrandId = configSettings.BRAND_ID;
                }

                // instantiate a new EnvelopesApi object
                EnvelopesApi envelopesApi = new EnvelopesApi();
                EnvelopeSummary envelopeSummary = envelopesApi.CreateEnvelope(accountId, envDef);

                rtn.Add(envelopeSummary);   //Index 0: add envelopeSummary to the return object

                // Create and save envelope locally (temporary)
                //app.helpers.createAndSaveLocal(req, envelopeSummary.envelopeId)
                //.then(function(){

                List<object> remainingSigners = new List<object>();

                if (body.inputSigningLocationAppraiser == "embedded")
                {
                    remainingSigners.Add(appraiserSigner);
                }
                else
                {
                    remainingSigners.Add("remote-signer");
                }

                remainingSigners.Add("remote-signer"); // last signer is remote (employee) 

                if (body.inputSigningLocation == "embedded")
                {
                    ViewUrl viewUrl = Helper.getRecipientUrl(envelopesApi, accountId, envDef, envelopeSummary.EnvelopeId.ToString(), signer);
                    rtn.Add(viewUrl);       //Index 1: add viewURL to the return object
                }
                rtn.Add(signer);            //Index 2: add signer to the return object
                rtn.Add(appraiserSigner);   //Index 3: add appraiserSigner to the return object

                return rtn;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw ex;
            }
        }
        #endregion

        #region Generic
        public static string InitializeDocuSignAPI()
        {
            try
            {
                //Get Config settings from App.config
                DocuSignConfig configSettings = new DocuSignConfig();

                string userName = configSettings.DOCUSIGN_USERNAME;
                string password = configSettings.DOCUSIGN_PASSWORD;
                string integratorKey = configSettings.DOCUSIGN_IK;

                // initialize client for desired environment (for production change to www)
                ApiClient apiClient = new ApiClient("https://demo.docusign.net/restapi");
                eSign.Client.Configuration.Default.ApiClient = apiClient;

                // configure 'X-DocuSign-Authentication' header
                string authHeader = "{\"Username\":\"" + userName + "\", \"Password\":\"" + password + "\", \"IntegratorKey\":\"" + integratorKey + "\"}";
                eSign.Client.Configuration.Default.AddDefaultHeader("X-DocuSign-Authentication", authHeader);

                // we will retrieve this from the login API call
                string accountId = null;

                // login call is available in the authentication api 
                AuthenticationApi authApi = new AuthenticationApi();
                LoginInformation loginInfo = authApi.Login();

                // parse the first account ID that is returned (user might belong to multiple accounts)
                accountId = loginInfo.LoginAccounts[0].AccountId;

                // Update ApiClient with the new base url from login call, but separate string to extract the base path
                string baseUrl = loginInfo.LoginAccounts[0].BaseUrl;
                string[] separator = new string[] { "/restapi" };
                string basePath = baseUrl.Split(separator, StringSplitOptions.None)[0] + "/restapi";
                apiClient = new ApiClient(basePath);

                //IMPORTANT: Reassign apiClient object - if this isn't done, the call will fail in production
                eSign.Client.Configuration.Default.ApiClient = apiClient;

                return accountId;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw ex;
            }

        }
        #endregion
    }

    public class Body
    {
        //create body class with the same signature as the Node.JS code
        public string inputSigningLocation { get; set; } = "";
        public string inputSigningLocationAppraiser { get; set; } = "";
        public string inputEmail { get; set; } = "";
        public string inputAppraiserEmail { get; set; } = "";
        public string inputFirstName { get; set; } = "";
        public string inputAppraiserFirstName { get; set; } = "";
        public string inputLastName { get; set; } = "";
        public string inputAppraiserLastName { get; set; } = "";
        public string inputPhone { get; set; } = "";
        public string inputAppraiserPhone { get; set; } = "";
        public string inputAddress1 { get; set; } = "";
        public string inputAddress2 { get; set; } = "";
        public string inputCity { get; set; } = "";
        public string inputState { get; set; } = "";
        public string inputZip { get; set; } = "";
        public string inputLoanAmount { get; set; } = "";
        public string inputLoanLength { get; set; } = "";
        public string inputAccessCode { get; set; } = "";
        public string inputAccessCodeAppraiser { get; set; } = "";
        public string inputAuthentication { get; set; } = "";
        public string inputAuthenticationAppraiser { get; set; } = "";
        public string inputCosignerCheckbox { get; set; } = "";
        public string inputCosignerFirstName { get; set; } = "";
        public string inputCosignerLastName { get; set; } = "";
        public string inputCosignerEmail { get; set; } = "";
        public string inputSigningLocationCosigner { get; set; } = "";
        public string inputAccessCodeCosigner { get; set; } = "";
        public string inputAuthenticationCosigner { get; set; } = "";
        public string inputCosignerPhone { get; set; } = "";
    }
    public class DocuSignConfig
    {
        public string DOCUSIGN_ENVIRONMENT { get; set; } = "";
        public string DOCUSIGN_USERNAME { get; set; } = "";
        public string DOCUSIGN_PASSWORD { get; set; } = "";
        public string DOCUSIGN_IK { get; set; } = "";
        public string EMPLOYEE_EMAIL { get; set; } = "";
        public string EMPLOYEE_NAME { get; set; } = "";
        public string LOCAL_RETURN_URL { get; set; } = "";
        public string BRAND_ID { get; set; } = "";
        public string GOOGLE_MAPS_API_KEY { get; set; } = "";
        public string GOOGLE_ANALYTICS { get; set; } = "";
        public string DEFAULT_EMAIL { get; set; } = "";
        public string FORCE_HTTPS { get; set; } = "";

        public DocuSignConfig()
        {
            System.Configuration.Configuration myDllConfig = ConfigurationManager.OpenExeConfiguration(this.GetType().Assembly.Location);
            AppSettingsSection myDllConfigAppSettings = (AppSettingsSection)myDllConfig.GetSection("appSettings");

            DOCUSIGN_ENVIRONMENT = myDllConfigAppSettings.Settings["DOCUSIGN_ENVIRONMENT"].Value;
            DOCUSIGN_USERNAME = myDllConfigAppSettings.Settings["DOCUSIGN_USERNAME"].Value;
            DOCUSIGN_PASSWORD = myDllConfigAppSettings.Settings["DOCUSIGN_PASSWORD"].Value;
            DOCUSIGN_IK = myDllConfigAppSettings.Settings["DOCUSIGN_IK"].Value;
            EMPLOYEE_EMAIL = myDllConfigAppSettings.Settings["EMPLOYEE_EMAIL"].Value;
            EMPLOYEE_NAME = myDllConfigAppSettings.Settings["EMPLOYEE_NAME"].Value;
            LOCAL_RETURN_URL = myDllConfigAppSettings.Settings["LOCAL_RETURN_URL"].Value;
            BRAND_ID = myDllConfigAppSettings.Settings["BRAND_ID"].Value;
            GOOGLE_MAPS_API_KEY = myDllConfigAppSettings.Settings["GOOGLE_MAPS_API_KEY"].Value;
            GOOGLE_ANALYTICS = myDllConfigAppSettings.Settings["GOOGLE_ANALYTICS"].Value;
            DEFAULT_EMAIL = myDllConfigAppSettings.Settings["DEFAULT_EMAIL"].Value;
            FORCE_HTTPS = myDllConfigAppSettings.Settings["FORCE_HTTPS"].Value;
        }
    }
    class Program
    {
        //Main() is used to test running the program as an exe, not as a DLL
        static void Main(string[] args)
        {
            //IMPORTANT NOTE: TESTING WITH THIS METHOD INCLUDES NO WEB UI, SO EMBEDDED SIGNING WON'T WORK.
            //THIS METHOD IS USED ONLY TO TEST/DEBUG THE WINDOWS CODE
            string accountId = DocuSign.Startup.InitializeDocuSignAPI();
            object[] reqArray = new object[4];

            DocuSignConfig configSettings = new DocuSignConfig();

            //create dictionary for all possible combinations in every loan scenario
            Dictionary<string, object> nodeBody = new Dictionary<string, object>();
            nodeBody = new Dictionary<string, object>();
            nodeBody.Add("inputEmail", configSettings.EMPLOYEE_EMAIL);
            nodeBody.Add("inputFirstName", "FirstName");
            nodeBody.Add("inputLastName", "LastName");
            nodeBody.Add("inputAppraiserEmail", configSettings.EMPLOYEE_EMAIL);
            nodeBody.Add("inputAppraiserFirstName", "AppraiserFirstName");
            nodeBody.Add("inputAppraiserLastName", "AppraiserLastName");
            nodeBody.Add("inputSigningLocation", "");
            nodeBody.Add("inputSigningLocationAppraiser", "");
            nodeBody.Add("inputPhone", "(877) 720-2040");
            nodeBody.Add("inputAddress1", "221 Main Street");
            nodeBody.Add("inputAddress2", "#1000");
            nodeBody.Add("inputCity", "San Francisco");
            nodeBody.Add("inputState", "CA");
            nodeBody.Add("inputZip", "94105");
            nodeBody.Add("inputLoanAmount", "1000.00");
            nodeBody.Add("inputLoanLength", "36");
            nodeBody.Add("inputAccessCode", "");
            nodeBody.Add("inputAuthentication", "");
            nodeBody.Add("inputCosignerCheckbox", "on");    //will be "on" if checked
            nodeBody.Add("inputCosignerFirstName", "CoSignerFirstName");
            nodeBody.Add("inputCosignerLastName", "CoSignerLastName");
            nodeBody.Add("inputCosignerEmail", configSettings.EMPLOYEE_EMAIL);
            nodeBody.Add("inputSigningLocationCosigner", "");
            nodeBody.Add("inputAccessCodeCosigner", "");
            nodeBody.Add("inputAuthenticationCosigner", "");
            nodeBody.Add("inputCosignerPhone", "(425)111-1111");


            reqArray[0] = nodeBody; //body
            reqArray[1] = null; //session

            //get path of assembly, but this is too deep from where the node.js path will be at runtime
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
            for (int i = 1; i < 5; i++)
            {
                //get parent path 5 times
                path = Directory.GetParent(path).FullName;
            }
            //append path from solution structure (assumes this didn't change)
            path += "\\GitHub Source\\sample-app-loanco-nodejs-master";

            reqArray[2] = path; //current path
            reqArray[3] = ""; //gmapBase64Doc

            object rtn = DocuSign.Startup.CreateEnvelope_PersonalLoan(reqArray, accountId);
            //object rtn = DocuSign.Startup.CreateEnvelope_AutoLoan(reqArray, accountId);
            //object rtn = DocuSign.Startup.CreateEnvelope_SailboatLoan(reqArray, accountId);

        }
    }
}

