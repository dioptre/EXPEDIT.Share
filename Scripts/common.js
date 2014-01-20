function getCookie(cname) {
    var name = cname + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i].trim();
        if (c.indexOf(name) == 0) return c.substring(name.length, c.length);
    }
    return "";
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
    popupWindow(url, title, w, h);
}
function popupWindow(url, title, w, h) {
    var left = (screen.width / 2) - (w / 2);
    var top = (screen.height / 2) - (h / 2);
    return window.open(url, title, 'toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=no, resizable=no, copyhistory=no, width=' + w + ', height=' + h + ', top=' + top + ', left=' + left);
}