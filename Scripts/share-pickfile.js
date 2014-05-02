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
            images: this.store.find('myFile', { page: params.page, keywords: params.keywords, pagesize: pfPageSize }),
            params: params
        })
    },
    afterModel: function (model, transition, params) {
        if ($("#searchFile").val() === "")
            $("#searchFile").val(model.params.keywords);
    }
});

App.SearchController = Ember.Controller.extend({
    next: function () {
        var first = this.get('model.images').objectAt(0);
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
            item.set('Selected', !item.get('Selected'));
            pickFile(item.get('ReferenceID'), item.get('Title'));
        }
    }
})

//App.AboutRoute = Ember.Route.extend({
//    model: function(params){
//      return this.store.find('myfiles', params.id);
//    }
//})

App.ApplicationAdapter = DS.RESTAdapter.extend({
    namespace: 'share',
    headers: {
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
    }
});


App.MyFile = DS.Model.extend({
    Title: DS.attr(),
    ReferenceID: DS.attr(),
    Row: DS.attr(),
    TotalRows: DS.attr(),
    ImageUrl: function () {
        return '/share/user/preview/' + this.get('ReferenceID');
    }.property(),
    Selected: function() {
        return false;
    }.property()
})


