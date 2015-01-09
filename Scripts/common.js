function setCookie(cname, cvalue, seconds) {
    var d = new Date();
    d.setTime(d.getTime() + (seconds * 1000));
    var expires = "expires=" + d.toGMTString();
    document.cookie = cname + "=" + cvalue + "; " + expires;
}
function getCookie(cname) {
    var name = cname + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i].trim();
        if (c.indexOf(name) == 0) return c.substring(name.length, c.length);
    }
    return "";
}

function validateEmail(email) {
    var re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(email);
}

function isLoggedIn() {
    return getCookie('.ASPXAUTH').length > 0;
}

function deleteAllCookies() {
    var cookies = document.cookie.split(";");

    for (var i = 0; i < cookies.length; i++) {
        var cookie = cookies[i];
        var eqPos = cookie.indexOf("=");
        var name = eqPos > -1 ? cookie.substr(0, eqPos) : cookie;
        document.cookie = name + "=;expires=Thu, 01 Jan 1970 00:00:00 GMT";
    }
}
function checkAffiliate() {
    var affiliateID = getCookie("AffiliateID");
    if (affiliateID.length == 0) {
        $.ajax({
            url: "/share/referral/id",
            context: document.body
        }).done(function (data) {
            document.cookie = "AffiliateID=" + data;
            refer(data);
        });
    }
    else {
        refer(affiliateID);
    }
}
function refer(affiliateID) {
    var url = "";
    if (affiliateID == null || affiliateID.length == 0) {
        url = document.URL;
    }
    else {
        url = window.location.origin + "/share/refer/" + affiliateID + "?name=" + document.URL.replace(window.location.origin, "");
    }
    var title = ""; var summary = "";
    $('h1').each(function (index) { title += ((index>0) ? " - ": "") + $(this).text(); });
    $('h2').each(function () { summary += $(this).text() + " "; });
    summary = "Check out the " + ((summary.length>0) ? summary + " on the " : "") + title.toLowerCase() + ". I think it could be really useful for everyone in our industry.";
    url = 'http://www.linkedin.com/shareArticle?mini=true&url=' + encodeURIComponent(url) + '&title=' + encodeURIComponent(title) + '&summary=' + encodeURIComponent(summary);
    var w = 600;
    var h = 450;
    var popup = popupWindow(url, title, w, h);
    checkPopup(popup);

}
function popupWindow(url, title, w, h) {
    var left = (screen.width / 2) - (w / 2);
    var top = (screen.height / 2) - (h / 2);
    return window.open(url, title, 'toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=no, resizable=no, copyhistory=no, width=' + w + ', height=' + h + ', top=' + top + ', left=' + left);
}
function checkPopup(popup, url) {
    if (!popup || popup.closed || (typeof popup.closed == 'undefined') || popup.outerHeight == 0 || popup.outerWidth == 0) {
        if (typeof url != 'undefined' && url) {
            alert('Please enable popups. Attempting to redirect instead...');
            document.location.href = url;
        }
        else {
            alert('Please enable popups.');
        }
    }
}
var setupPager = false;
var pagers = new Array();
function addPager(offset, pageSize, urlPrefix, action, once) { //MyTicketsPartial & /tickets/user/
    if (!setupPager) {
        $.ajaxSetup({ cache: false });
        setupPager = true;
    }
    if (!pagers[action] || (typeof pagers[action] == 'undefined')) {
        var s = //'<script language=\"javascript\" type=\"text/javascript\">' +
            'var pageSize' + action + ' = ' + pageSize + ';' +
            'var offset' + action + ' = ' + offset + ';' +
            '$(\"#' + action + 'PagerNext\").click(function () {' +
            'offset' + action + '+=pageSize' + action + ';' +
            '$(\"#' + action + 'PagerPage\").load(\'' + urlPrefix + action + '/\' + pageSize' + action + ' + \'/\' + offset' + action + ');' +
            '});' +
            '$(\"#' + action + 'PagerPrevious\").click(function () {' +
            'if (offset' + action + ' < 2)' +
            '   return;' +
            'offset' + action + '-=pageSize' + action + ';' +
            '$(\"#' + action + 'PagerPage").load(\'' + urlPrefix + action + '/\' + pageSize' + action + ' + \'/\' + offset' + action + ');' +
            '});'
        // + '</script>'
        ;
        eval(s);
        eval('$(\"#' + action + 'PagerPage\").load(\'' + urlPrefix + action + '/\' + pageSize' + action + ' + \'/\' + offset' + action + ');');
        if (once)
            pagers[action] = true;
    }
}

// For phonegap
var expHost = window.location.origin;
if (expHost === "file://")
    expHost = "http://app.flowpro.io";

if (typeof Offline !== 'undefined' && Offline)
    Offline.options = { checks: { xhr: { url: expHost + '/share/IsOnline' } } };

if (typeof Messenger !== 'undefined' && Messenger)
    Messenger.options = {
        extraClasses: 'messenger-expedit messenger-fixed messenger-on-bottom messenger-on-right',
        theme: 'flat',
        messageDefaults: { hideAfter: 3 }
    };

function TraverseDom(node, func) {
    func(node);
    node = node.firstChild;
    while (node) {
        TraverseDom(node, func);
        node = node.nextSibling;
    }
}


function queryParamsLookup(name) {
    name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    var regex = new RegExp("[\\?&]" + name + "=([^&#]*)");
    var results = regex.exec(window.location.href);
    if (results == null)
        return null;
    else
        return results[1];
}

function JSON2CSV(objArray) {
    var array = typeof objArray != 'object' ? JSON.parse(objArray) : objArray;

    var str = '';
    var line = '';

    var head = array[0];
    for (var index in array[0]) {
        line += index + '\t';
    }

    line = line.slice(0, -1);
    str += line + '\r\n';
    

    for (var i = 0; i < array.length; i++) {
        var line = '';

        for (var index in array[i]) {
            line += array[i][index] + '\t';
        }

        line = line.slice(0, -1);
        str += line + '\r\n';
    }
    return str;

}

function Download(str) {
    window.open("data:text/csv;charset=utf-8," + escape(str))
}

function DownloadCSV(objArray) {
    var csv = JSON2CSV(objArray);
    Download(csv);
}

String.prototype.hashCode = function () {
    var hash = 0, i, chr, len;
    if (this.length == 0) return hash;
    for (i = 0, len = this.length; i < len; i++) {
        chr = this.charCodeAt(i);
        hash = ((hash << 5) - hash) + chr;
        hash |= 0; // Convert to 32bit integer
    }
    return hash;
};

function ToColor(str) {
    if (!str)
        return '#111111';
    var color = parseInt(Math.abs(str.hashCode()), 10).toString(16).replace(/[defDEF]/ig, '3');
    color += '222222';
    return '#' + color.substr(0, 6);
}