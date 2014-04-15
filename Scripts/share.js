App = Ember.Application.create();

App.Router.map(function () {
    this.route('about', { path: '/about/:id' });
});

App.IndexRoute = Ember.Route.extend({
  model: function() {
      return this.store.find('myFile');
  }
});

//App.AboutRoute = Ember.Route.extend({
//    model: function(params){
//      return this.store.find('myfiles', params.id);
//    }
//})

App.ApplicationAdapter = DS.RESTAdapter.extend({
  namespace: 'share'
});


App.MyFile = DS.Model.extend({
    Title: DS.attr(),
    ReferenceID: DS.attr(),
    ImageUrl: function () {
        return '/share/user/preview/' + this.get('ReferenceID');
    }.property()
})


