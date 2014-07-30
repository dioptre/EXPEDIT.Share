// This is a libary function needed to run myformeditor, myimgplugin.
// It handles the general function to render html templates, setup click handlers in different places (important for integration with ember)
// Main idea here is that all information is stored in one div and then template are rendered inside of that div (this means that templates can be updated at any point)

// list of conventions:
// 1. Main object needs :
//   data-json: 'this is the place to store the settings for the object (custom sturcture for each plugin'
//   class: need to contain .tiny and a class prefixed with tiny like .tiny-form 
//   type: the same name as the prefixed class. this is important to set correctly as it will be used to identiy what render function will be used
//   Example: <div data-json="{}" class="tiny tiny-form mceNonEditable disable-user-select" type="tiny-form"></div>
// 2. Different type of functions necessary for rendering
//   a) 'tiny-form' - main one used for the template (no js, just template)
//   b) 'tiny-form-trigger' - used to setup additional click helpers, this is rendered by default
//   c) 'tiny-form-editor' - additonal template code and click handlers to 

(function () {
    
    // Removing the content of all .tiny object & any helper divs identified by .tiny-popover
    window.cleanFunctions = function (selected) {
        $selected = $(selected);

        TraverseDom($selected, function (e) {
            if (e.hasClass('tiny'))
                e.empty()
            if (e.hasClass('tiny-popover'))
                e.remove();
        });

        $selected = $selected.filter(':not(.tiny-popover)');

        return $selected;
    }

    // use this for dom nodes, cleanFunctions() is for virtual dom
    window.cleanAlternative = function () {
        $('.tiny').empty()
        $('.tiny-popover').remove();
    }



    // Rendering all templates 
    //if (typeof window.renderFunctions === 'undefined') {
        window.renderFunctions = function (selector) {
            if (typeof selector === 'undefined')
                selector = $('.tiny'); //in the case undefined is passed through select any tinys possible HACK
            selector.find('*').andSelf().filter('.tiny').each(function () {
                var $this = $(this)
                var type = $this.attr('type');
                window.renderFunction[type]($this);
                var trigFn = window.renderFunction[type + '-trigger'];
                if (typeof trigFn !== 'undefined') {
                    trigFn($this);
                }
            });
        }
    //}


    // Setup global renderFunction object
    //if (typeof window.renderFunction === 'undefined')
        window.renderFunction = {};

})()