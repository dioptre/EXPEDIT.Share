

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
        //debugger;
        if (temp.length > 2 || temp == '') {
            if (timer) {
                clearTimeout(timer);
            }
            timer = setTimeout(function () {
                controller.transitionToRoute('search', 0, temp)
            }, 300);
        }
    }.observes('q'),
    actions: {
        blur: function (e) {

        }
    }
});

var pfPageSize = 20;
var tempx;
App.SearchRoute = Ember.Route.extend({
    model: function (params) {
        return Ember.RSVP.hash({
            results: (params.keywords.length > 0) ? this.store.find('search', { page: params.page, keywords: params.keywords, pagesize: pfPageSize, type: 'workflow' }) : [],
            params: params
        });
    },
    afterModel: function (model, transition, params){
        //console.log(model);
    }
});

App.SearchController = Ember.Controller.extend({
    next: function () {
        var first = this.get('model.results').objectAt(0);
        if (typeof first === 'undefined')
            return false;        
        return (((this.get('currentPage')+1) * pfPageSize) < first.get('TotalRows'));
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
            pickWorkflow(item);
        }
    }
})

App.ApplicationAdapter = DS.RESTAdapter.extend({
    namespace: 'flow'
});


$.ajaxPrefilter(function (options, originalOptions, jqXHR) {
    if (options.type.toUpperCase() == "POST") {
        options.data = $.param($.extend(JSON.parse(originalOptions.data), { __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() }));
        options.contentType = "application/x-www-form-urlencoded; charset=UTF-8";
    }
});


App.Search = DS.Model.extend({
    Title: DS.attr(),
    ReferenceID: DS.attr(),
    Row: DS.attr(),
    TotalRows: DS.attr(),
    Author: DS.attr(),
    TotalRows: DS.attr()
});

