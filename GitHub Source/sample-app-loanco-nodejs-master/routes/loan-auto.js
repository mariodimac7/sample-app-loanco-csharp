var express = require('express');
var router = express.Router();

var _ = require('lodash');

//Edge.js to marshall calls to .NET
var edge = require('edge');

var docusign = require('docusign-esign'),
    async = require('async'),
    fs = require('fs'),
    path = require('path');

router.get('/loan/auto', function (req, res, next) {
    res.render('loan-auto', {
        signing_location_options: app.helpers.signing_location_options,
        authentication_options: app.helpers.authentication_options
    });
});

router.post('/loan/auto', function (req, res, next) {
    var body = req.body;

    //************** BEGIN ADDED FOR C# **************
    //get the DLL path starting 2 directories up from the current one - assumes solution structure is not changed
    var currentPath = process.cwd();
    var docuSignDll = process.env.EDGE_USE_CORECLR ? 'test' : path.join(currentPath + '/../../' + 'DocuSign CSharp LoanCo/bin/Debug', 'DocuSign.dll');

    //Create new object with the following child objects to send to Edge.js:
    //0: body
    //1: session
    //2: current path
    var edgeObj = [];
    edgeObj.push(req.body);
    edgeObj.push(req.session);
    edgeObj.push(currentPath);

    var func = edge.func({
        assemblyFile: docuSignDll,          //name declared above
        typeName: 'DocuSign.Startup',       //namespace.class
        methodName: 'InvokeAutoLoan'    //method
    });

    func(edgeObj, function (error, result) {
        if (error) throw error;
        console.log(result);

        //indexes of objects in result list:
        //0: EnvelopeSummary
        //1: ViewURL
        //2: tRoleApplicant
        //3: tRoleCosigner
        var envelopeSummary = result[0];
        var viewUrl= result[1];
        var tRoleApplicant = result[2];
        var tRoleCosigner = result[3];

        //save locally so it can be retrieved on the Status page (link) at the top of the UI
        app.helpers.createAndSaveLocal(req, envelopeSummary.EnvelopeId)

        if (body.inputSigningLocation == 'embedded') {
            var tApplicantRecipient = {
                recipientId: _.find(app.config.templatesByKey.cosigner_on_auto_loan.json.recipients.signers, { roleName: 'applicant' }).recipientId,
                clientUserId: tRoleApplicant.clientUserId,
                name: tRoleApplicant.name,
                email: tRoleApplicant.email
            };
        }

        if (body.inputSigningLocation == 'embedded') {
            req.session.signingUrl = viewUrl.Url;
            res.redirect('/sign/embedded');
        }
        else {
            res.redirect('/sign/remote');
        }
    });
    //************** END ADDED FOR C# **************

});


module.exports = router;