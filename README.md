
### DocuSign LoanCo Online Sample App 

[Online LoanCo Sample App](https://loancosample.docusign.com/) 

LoanCo is a sample loan app that shows some common ways an application might interact with the DocuSign eSignature API. Various switches (authentication, embedded signing, templates) can be changed to show additional platform and API features and how easy they are to add to your own solution. LoanCo offers three (3) different loan workflows that demonstrate various features and workflows available through the platform.

#### Requirements

- [Free Developer Sandbox](https://secure.docusign.com/signup/develop)
- [Node.js v.6.9.4](https://nodejs.org/download/release/v6.9.4/)
- [Visual Studio 2017 (any edition)](https://www.visualstudio.com/downloads/)

#### Approach 
This sample code uses both Node.js and C#. To maintain consistency with the existing Node.js LoanCo sample app, we continued to use Node.js as a front-end web server, but replaced the Node.js code with C# where it calls the DocuSign eSignature API. Please see [this blog post](https://www.docusign.com/blog/dsdev-loanco-sample-app-now-available-csharp) for more info.


#### Installation 

	git clone <repo> 
	cd <repo directory>
	npm install
	npm install edge


#### Running 

	Run the Visual Studio solution. It will already be set to start the Node.js web server by default. Open a browser and type http://localhost:3801.
	

#### Configuration 

> DocuSign has multiple ways of authenticating your app. This example (storing username/password/integrator-key locally and sending in a custom `X-DocuSign-Authentication` header) is known as [legacy auth](https://docs.docusign.com/esign/guide/authentication/legacy_auth.html). You can also authenticate using the [OAuth2](https://docs.docusign.com/esign/guide/authentication/auth_server.html) Authorization Code Grant and Implicit Grant methods.


We use environment variables to setup our configuration. Because the LoanCo C# application uses both Node.js and C#, you need configuration variables in two places. The Visual Studio 2017 solution already contains both configuration files.

.env file (used in the Node.js project) in the root of the samplesample-app-loanco-nodejs-master project:

	DOCUSIGN_ENVIRONMENT=demo  // use "www" for production  
	DOCUSIGN_USERNAME=         // account email address  
	DOCUSIGN_PASSWORD=         // account password
	DOCUSIGN_IK=               // Integrator Key 
	EMPLOYEE_EMAIL=            // used for final recipient of Personal Loan
	EMPLOYEE_NAME=             // used for final recipient of Personal Loan
	LOCAL_RETURN_URL=http://localhost:3801/   // change to the correct return url, with a trailing slash
	BRAND_ID=                  // not required, use to show a different Brand for the Sailboat example 
	GOOGLE_MAPS_API_KEY=       // required for Sailboat example to work
    GOOGLE_ANALYTICS=          // UA-XYZ-1
	DEFAULT_EMAIL=             // for autofilling email input fields
    FORCE_HTTPS=               // force https by setting to true

App.config file (used in the C# project) in the root of the DocuSign CSharp LoanCo project (replace the <replace> values with your values and reference the descriptions above):

    <appSettings>
    <add key="DOCUSIGN_ENVIRONMENT" value="demo" />
    <add key="DOCUSIGN_USERNAME" value="<replace>" />
    <add key="DOCUSIGN_PASSWORD" value="<replace>" />
    <add key="DOCUSIGN_IK" value="<replace>" />
    <add key="EMPLOYEE_EMAIL" value="<replace>" />
    <add key="EMPLOYEE_NAME" value="Internal LoanCo Account" />
    <add key="LOCAL_RETURN_URL" value="http://localhost:3801/" />
    <add key="AUTOLOAN_TEMPLATEID" value="<replace>" />
    <add key="BRAND_ID" value="f157069c-3828-4073-a298-0bf5749b27bb" />
    <add key="GOOGLE_MAPS_API_KEY" value="AIzaSyD1-3_ag-DVGsvLuHWfx-gOE4-F2y-rskk" />
    <add key="GOOGLE_ANALYTICS" value="UA-39550292-1" />
    <add key="DEFAULT_EMAIL" value="<replace>" />
    <add key="FORCE_HTTPS" value="0" />
    <add key="ClientSettingsProvider.ServiceUri" value="" />
    </appSettings>


##### Templates 

When initially run, the app will attempt to create a Template for the Auto Loan Application. This template is defined at `pdfs/template-auto-loan.json`. Upload this template into your DocuSign account. After doing so, it will generate a TemplateID, which needs to be placed in the AUTOLOAN_TEMPLATEID value in the C# configuration file above.


#### Errors you may encounter  

    { 
        errorCode: 'ACCOUNT_LACKS_PERMISSIONS',
        message: 'This Account lacks sufficient permissions. Document Visibility has been specified.  This account does not have document visibility turned on.' 
    }

Change this setting: http://imgur.com/j4VD6nd


#### API Tools and Links

__Developer Center__  
https://www.docusign.com/devcenter

__API Recipes (code walkthroughs)__  
https://www.docusign.com/developer-center/recipes

__API Documentation__  
https://docs.docusign.com/  

__API Explorer__  
https://apiexplorer.docusign.com/  



#### License 

The DocuSign LoanCo Sample App is licensed under the MIT [License](LICENSE).



