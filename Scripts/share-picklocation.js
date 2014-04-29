function MapInitialize() {
    if (drawing)
        smap = SetupDrawingMap('map-search');
    else
        smap = SetupMap('map-search');
    RedrawMap(smap);

}


function OnMapUpdate(map, event, center, viewport) {
    cmap = map;
    if (event.eventType == "EDITED") {
        if (event.eventSource.type == "marker") {
            DeleteExceptedShape(map, event.eventSource)
            $("#Geography").val(MapObjectsToString(map));
        }
        else {
            DeleteExceptedShapeTypes(map);
            $("#Geography").val(MapObjectsToString(map));
        }
    }
    if (event.eventType == "MARKER_DRAGEND") {
        $("#Geography").val(MapObjectsToString(map));
    }
    if (event.eventType == "MARKER_CLICKED" && !drawing && event.eventSource && event.eventSource.type === "marker") {
        if (typeof pickLocation !== 'undefined') 
            pickLocation(event.eventSource.uniqueid, event.eventSource.title);
    }    
}

var drawing = false;
var smap;
var cmap;
LoadScript('http://maps.googleapis.com/maps/api/js?libraries=drawing&sensor=true&callback=MapInitialize');

App = Ember.Application.create();

App.Router.map(function () {
    this.route('index', { path: '/' });
    this.route('index', { path: '/:page' });
    this.route('search', { path: '/:page/:keywords' });
});

App.IndexRoute = Ember.Route.extend({
    beforeModel: function () {
        this.transitionTo("search", 0, '');
    }
})

var timer;
App.ApplicationController = Ember.Controller.extend({
    q: '',
    qm: function () {
        var controller = this;
        var temp = this.get('q');

        if (temp.length > 2 || temp == '') {
            if (timer) {
                clearTimeout(timer);
            }
            timer = setTimeout(function () {
                controller.transitionToRoute('search', 0, temp)
            }, 300);
        }
    }.observes('q'),
    submitAction: function () {
        alert('hi');
    }
});

var pfPageSize = 20;
var tempx;
App.SearchRoute = Ember.Route.extend({
    model: function (params) {
        return Ember.RSVP.hash({
            results: this.store.find('myLocation', { page: params.page, keywords: params.keywords, pagesize: pfPageSize }),
            params: params
        });
    },
    afterModel: function (model, transition, params){
        //console.log(model);
        //alert('hi');
        tempx = model;
        DeleteShapes(smap);
        HideDrawingMap(smap);
        drawing = false;
        $.each(model.results.content, function (i, a) {
            //alert(i + a);
            //console.log(a.get('SpatialJSON'));
            //var t = eval(a.get('SpatialJSON'));
            //console.log(t);
            var geoData = ParseGeographyData(JSON.parse(a.get('SpatialJSON')).data);
            AddMarkerSingle(smap, GetFirstLocation(geoData), false, a.get('Title'), a.get('id'));
            $("#LocationID").val(a.get('id'))
            RefocusMap(smap);
        });

        if (model.results.content.length == 0) {
            alertify.log("Couldn't locate a match. You'll need to add a new location using the map tools.")
            if (!smap.drawingManager)
                SetupDrawingMap(null, null, smap);
            ShowDrawingMap(smap);
            $("#updateLocation").show();
            drawing = true;
            GetAddressLocation(model.params.keywords, function (latlng) {
                var guid = NewGUID();
                $("#LocationID").val(guid)
                AddMarkerSingle(smap, latlng, true, model.params.keywords, guid);
                RefocusMap(smap);
                smap.setZoom(15);
                $("#Geography").val(MapObjectsToString(smap));
            });
        }
        else {
            $("#updateLocation").hide();
        }

        if ($("#searchLocation").val() === "")
            $("#searchLocation").val(model.params.keywords);

        $("#LocationName").val(model.params.keywords);        

    }
});

App.SearchController = Ember.Controller.extend({
    next: function () {
        var first = this.get('model.results').objectAt(0);
        if (typeof first === 'undefined')
            return false;        
        return (((this.get('currentPage')+1) * pfPageSize) < first.get('Total'));
    }.property('model'),
    prev: function() {
        var params = this.get('model.params');
        return params.page > 0;
    }.property('model'),
    currentPage: function(){
        var params = this.get('model.params');
        return ((typeof params.page == 'undefined') || params.page == 'undefined' || (parseInt(+params.page) === 'NaN')) ? 0 : parseInt(params.page);
    }.property('model'),
    actions: {
        transition: function (page) {
            var controller = this;
            var params = this.get('model.params');            
            var pp = (page == 'Next') ? this.get('currentPage') + 1 : this.get('currentPage') - 1;
            controller.transitionToRoute('search', pp , params.keywords)
        },
        selectToggle: function (item) {
            item.set('Selected', !item.get('Selected'));
            pickLocation(item.get('ReferenceID'), item.get('Title'));
        }
    }
})

App.ApplicationAdapter = DS.RESTAdapter.extend({
    namespace: 'share'
});


$.ajaxPrefilter(function (options, originalOptions, jqXHR) {
    if (options.type.toUpperCase() == "POST") {
        options.data = $.param($.extend(JSON.parse(originalOptions.data), { __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() }));
        options.contentType = "application/x-www-form-urlencoded; charset=UTF-8";
    }
});


App.MyLocation = DS.Model.extend({
    Title: DS.attr(),
    ReferenceID: DS.attr(),
    LocationID: DS.attr(),
    Sequence: DS.attr(),
    Total: DS.attr(),
    SpatialJSON: DS.attr(),
    Geography: DS.attr(),
    Selected: function () {
        return false;
    }.property()
});


App.NewLocationView = Ember.View.extend({
    click: function (evt) {
        //this.get('controller').send('turnItUp', 11);
        //alert('hi');
        if (smap.mapOverlays.length == 0) {
            alertify.error("You must add a point to create a new location.");
        }
        else {
            var m = App.MyLocation.store.createRecord('myLocation', {
                LocationID: $("#LocationID").val(),
                Title: $("#LocationName").val(),
                Geography: $("#Geography").val()
            });        
            // Save the new model
            m.save().then(function (response) {
                //if success
                alertify.log('Updated location.');
                if (typeof pickLocation !== 'undefined')
                    pickLocation($("#LocationID").val(), $("#LocationName").val());
            }, function (response) {
                //if failure
                alertify.error('Failed saving new location. Please try again with a new location or name.');
            });

        }
    }
});


