var formData = {"fields":[{"label":"Untitled","field_type":"address","required":true,"field_options":{},"cid":"c9"}]} 

$(document).ready(function () {
	$.each(formData.fields, function(i,d){
	  var source   = $("#temp-" + d.field_type).html();
	  var template = Handlebars.compile(source);
	  var html     = template(d);
	  $('#formed').append(html);
	})



	$('#formed').bootstrapValidator();
	
	$('<div>').attr({
		id:''
	})
	//$('#formed').id = "DC85F2C1-5CE6-4BC9-ACB4-7AD65004B15E"

});

$(document).ready(function() {
    $('#attributeForm').bootstrapValidator();
});