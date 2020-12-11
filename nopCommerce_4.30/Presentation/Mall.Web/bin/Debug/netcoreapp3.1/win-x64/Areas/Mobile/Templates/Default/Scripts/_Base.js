var spreadidparaname = "spreadid", needclearspreadidckname = "Mall-needclearspreadid";
$(function () {
    if ($(document).width() <= 640) {
        $("html").niceScroll({ cursorwidth: 0, cursorborder: 0 });
    }

    //处理分销cookie
    if (sessionStorage) {
        spreadid = QueryString(spreadidparaname);
        if (spreadid.length > 0) {
            sessionStorage.setItem(spreadidparaname, spreadid);
        } else {
            spreadid = sessionStorage.getItem(spreadidparaname);
            if (!spreadid || spreadid.length < 1) {
                sessionStorage.setItem(spreadidparaname, "");
                document.cookie = needclearspreadidckname + "=1";
            }
        }
    }

    if (isWeiXin()) {
        var localhref = window.location.href.toLowerCase();
        if (localhref.indexOf("/order/") != -1) {
            //写入空白历史记录
            pushHistory();

            //延时监听
            setTimeout(function () {
                //监听物理返回按钮
                window.addEventListener("popstate", function (e) {
                    var ah = window.localStorage.getItem("arrHistory");
                    var arrHistory = [];
                    if (ah) {
                        arrHistory = JSON.parse(ah);
                    }
                    var curUrl = arrHistory.pop();
                    var returnUrl = arrHistory.pop();
                    if (curUrl.toLowerCase().indexOf("/order/ordershare?") > 0)
                        returnUrl = "/" + areaName + "/Member/Orders?orderStatus=0";
                    window.localStorage.setItem("arrHistory", JSON.stringify(arrHistory));
                    if (returnUrl && returnUrl.length > 0) {
                        location.href = returnUrl;
                    } else {
                        //调用微信浏览器私有API关闭浏览器  
                        WeixinJSBridge.call('closeWindow');
                    }
                    e.preventDefault();
                }, false);

            }, 300);
        }
    }
});

function checkLogin(returnHref, callBack, loginshopid) {

    var memberId = $.cookie('Mall-User');
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
    var ah = window.localStorage.getItem("arrHistory");
    var arrHistory = [];
    if (ah) {
        arrHistory = JSON.parse(ah);
    }
    arrHistory.push(location.href.toLowerCase());
    window.localStorage.setItem("arrHistory", JSON.stringify(arrHistory));
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
        addCookie(this.curPositionOption.positionKeyName, fromLatLng, this.curPositionOption.cacheTimeHour);
    },
    getCurPositionCookie: function () {
        return getCookie(this.curPositionOption.positionKeyName);
    }
});
