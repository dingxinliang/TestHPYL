$(function () {
    if ($(document).width() <= 640) {
        $("html").niceScroll({ cursorwidth: 0, cursorborder: 0 });
    }

    if (isWeiXin()) {
        //写入空白历史记录
        pushHistory();

        //延时监听
        setTimeout(function () {
            //监听物理返回按钮
            window.addEventListener("popstate", function (e) {
                //获取URL中值
                var returnUrl = getQueryString("returnUrl");
                if (returnUrl && returnUrl.length > 0) {
                    window.location.href = returnUrl;
                } else {
                    window.history.go(-1);
                }
                e.preventDefault();
            }, false);

        }, 300);
    }
});

function checkLogin(returnHref, callBack, loginshopid) {

    var memberId = $.cookie('Himall-User');
    if (memberId) {
        callBack();
    }
    else {
        $.ajax({
            type: 'get',
            url: '/' + areaName + '/login/CheckLogin',
            data: {},
            dataType: 'json',
            success: function (result) {
                if (result.success) {
                    callBack();
                }
                else {
                    $.dialog.tips("您尚未登录，请先登录", function () {
                        if (loginshopid && MAppType != '') {
                            location.href = "/" + areaName + "/Redirect?redirectUrl=" + returnHref + '&shop=' + MAppType;
                        }
                        else {
                            location.href = "/" + areaName + "/Redirect?redirectUrl=" + returnHref;
                        }
                    });
                }
            }
        });

    }
}

function pushHistory() {
    var state = {
        title: "title",
        url: "#"
    };
    window.history.pushState(state, "title", "#");
}

function getQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
    var r = window.location.search.substr(1).match(reg);
    if (r != null) return decodeURIComponent(r[2]); return null;
}

function isWeiXin() {
    var ua = window.navigator.userAgent.toLowerCase();
    if (ua.match(/MicroMessenger/i) == 'micromessenger') {
        return true;
    } else {
        return false;
    }
}

//门店定位的经纬度
$.extend({
    curPositionOption: {
        positionKeyName: 'curPosition',
        cacheTimeHour: 0.5
    },
    addCurPositionCookie: function (fromLatLng) {

        addCookie(this.curPositionOption.positionKeyName, fromLatLng, this.curPositionOptioncacheTimeHour);
    },
    getCurPositionCookie: function () {
        return getCookie(this.curPositionOption.positionKeyName);
    }
});
