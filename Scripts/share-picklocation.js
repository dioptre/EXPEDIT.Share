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
        }
        else {
            DeleteExceptedShapeTypes(map);
        }
    }
    if (event.eventType == "BOUNDS_CHANGED") {
        //console.log(event);
    }
}

var drawing = false;
var smap;
var cmap;
if (!drawing)
    LoadScript('https://maps.googleapis.com/maps/api/js?v=3.exp&sensor=false&callback=MapInitialize');
else
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
    }.observes('q')
});

var pfPageSize = 20;
App.SearchRoute = Ember.Route.extend({
    model: function (params) {
        return Ember.RSVP.hash({
            results: this.store.find('myLocation', { page: params.page, keywords: params.keywords, pagesize: pfPageSize }),
            params: params
        })
    },
    afterModel: function (model, transition, params){
        //console.log(model);
        //alert('hi');
        $.each(model.results.content, function (i, a) {
            //alert(i + a);
            //console.log(a.get('SpatialJSON'));
            //var t = eval(a.get('SpatialJSON'));
            //console.log(t);
            AddGeographyUnique(smap, JSON.parse(a.get('SpatialJSON')).data, false, a.get('Title'), true, NewGUID());
        });
        //AddGeographyUnique(smap, JSON.parse(ui.item.id).spatial, false, ui.item.label, true, NewGUID());
        //RefocusMap(smap);
        //smap.setZoom(9);
        //Add new:
        //GetAddressLocation($("#searchLocation").val(), function (latlng) {
        //    DeleteShapes(smap);
        //    AddMarkerSingle(smap, latlng, false, $("#searchLocation").val(), NewGUID());
        //    RefocusMap(smap);
        //    smap.setZoom(15);
        //});
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
    namespace: 'share',
    headers: {
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
    }
});


App.MyLocation = DS.Model.extend({
    Title: DS.attr(),
    ReferenceID: DS.attr(),
    Sequence: DS.attr(),
    Total: DS.attr(),
    SpatialJSON: DS.attr(),
    Selected: function() {
        return false;
    }.property()
})


