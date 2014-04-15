App = Ember.Application.create();

App.Router.map(function () {
    this.route('search', { path: '/' });
    this.route('search', { path: '/:page' });
    this.route('search', { path: '/:page/:keywords' });
});

App.IndexRoute = Ember.Route.extend({
    redirect: function () {
        this.transitionToRoute("search", 0, null);
    }
})

var timer;
App.ApplicationController = Ember.Controller.extend({
    q: '',
    qm: function () {
        var controller = this;
        var temp = this.get('q');

        if (temp.length > 2) {
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
        console.log(params.page, params.keywords);
//        return this.store.find('myFile', { page: params.page, keywords: params.keywords, pagesize: pfPageSize });
        return Ember.RSVP.hash({
            images: this.store.find('myFile', { page: params.page, keywords: params.keywords, pagesize: pfPageSize }),
            params: params
        })
  }
});

App.SearchController = Ember.Controller.extend({
    next: function () {
        var total = this.store.all('myFile').objectAt(0).get('Total');
        return (((this.get('currentPage')+1) * pfPageSize) < total);
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
    Sequence: DS.attr(),
    Total: DS.attr(),
    ImageUrl: function () {
        return '/share/user/preview/' + this.get('ReferenceID');
    }.property()
})


