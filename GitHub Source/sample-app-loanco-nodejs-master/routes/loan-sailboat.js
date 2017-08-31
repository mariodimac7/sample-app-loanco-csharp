var express = require('express');
var router = express.Router();

var _ = require('lodash');

//Edge.js to marshall calls to .NET
var edge = require('edge');

var GoogleMapsAPI = require('googlemaps');

var docusign = require('docusign-esign'),
  async = require('async'),
  fs = require('fs'),
  path = require('path');

router.get('/loan/sailboat', function(req, res, next) {
	res.render('loan-sailboat', {
		signing_location_options: app.helpers.signing_location_options,
		authentication_options: app.helpers.authentication_options
	});
});

router.post('/loan/sailboat', function(req, res, next) {
    var body = req.body;

    //************** BEGIN ADDED FOR C# **************
    //get the DLL path starting 2 directories up from the current one - assumes solution structure is not changed
    var currentPath = process.cwd();
    var docuSignDll = process.env.EDGE_USE_CORECLR ? 'test' : path.join(currentPath + '/../../' + 'DocuSign CSharp LoanCo/bin/Debug', 'DocuSign.dll');

    //Create new object with the following child objects to send to Edge.js:
    //0: body
    //1: session
    //2: current path
    //3: gmapBase64Doc (Google Maps image)
    var edgeObj = [];
    edgeObj.push(req.body);
    edgeObj.push(req.session);
    edgeObj.push(currentPath);

    var func = edge.func({
        assemblyFile: docuSignDll,          //name declared above
        typeName: 'DocuSign.Startup',       //namespace.class
        methodName: 'InvokeSailboatLoan'    //method
    });
    //************** END ADDED FOR C# **************


	// Get Google map
	var gmAPI = new GoogleMapsAPI({
		key: app.config.google_maps_api_key,
		// stagger_time:       1000, // for elevationPath
		// encode_polylines:   false,
		// secure:             true, // use https
		// proxy:              'http://127.0.0.1:9999' // optional, set a proxy for HTTP requests
	});
	var params = {
	  center: '37.808546, -122.409767',
	  zoom: 15,
	  size: '500x400',
	  maptype: 'roadmap',
	  markers: [
		{
		  location: '37.808546, -122.409767',
		  icon: 'http://chart.apis.google.com/chart?chst=d_map_pin_icon&chld=cafe%7C996600'
		}
	  ],
	  style: [
		{
		  feature: 'road',
		  element: 'all',
		  rules: {
			hue: '0x00ff00'
		  }
		}
	  ]
	};
	
	// Download the map 
    var gmApiImageUrl = gmAPI.staticMap(params);
    var gmapBase64Doc = "";
	var request = require('request').defaults({ encoding: null });
	request.get(gmApiImageUrl, function (mapErr, response, imageBody) {
		if(mapErr){
			console.error('gmAPI.staticMap error');
			console.error(err);
		} else {
			gmapBase64Doc = new Buffer(imageBody).toString('base64');
		}
    });
    edgeObj.push(gmapBase64Doc);

        
    //************** BEGIN ADDED FOR C# **************
        func(edgeObj, function (error, result) {
            if (error) throw error;
            console.log(result);

            //indexes of objects in result list:
            //0: EnvelopeSummary
            //1: ViewURL
            //2: signer
            //3: appraiserSigner

            var envelopeSummary = result[0];
            var viewUrl = result[1];

            //save locally so it can be retrieved on the Status page (link) at the top of the UI
            app.helpers.createAndSaveLocal(req, envelopeSummary.EnvelopeId)

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