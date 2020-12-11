//页面级参数
var positionKeyName = 'curPosition';
var fromLatLng = '';
var curAddress = '';
var cityId = '';
var cityName = '';
var isRefresh = true;
var proId = 0, cityId = 0, districtId = 0, streetId = 0;
var searchService;
var pageIndex = 0;
var pageCapacity = 100;
var geocoder;
var hasData = true;
var isBlur = true;

$(function () {

    var spanCityName = document.getElementById("spanCityName");
    $.ajax({
        url: '/common/RegionAPI/GetAllRegion',
        type: 'get', //GET
        async: true,    //或false,是否异步
        data: {
        },
        timeout: 10000,    //超时时间
        dataType: 'json',    //返回的数据格式：json/xml/html/script/jsonp/text
        success: function (data, textStatus, jqXHR) {
            cityPicker3 = new mui.PopPicker({
                layer: 2,
                getData: function (parentId) {
                    var ret = [];
                    if (!parentId) return ret;
                    $.ajax({
                        url: '/common/RegionAPI/GetSubRegion',
                        type: 'get', //GET
                        async: false,    //或false,是否异步
                        data: { parent: parentId, bAddAll: true },
                        timeout: 10000,    //超时时间
                        dataType: 'json',    //返回的数据格式：json/xml/html/script/jsonp/text
                        success: function (data, textStatus, jqXHR) {
                            ret = data;
                        }
                    });
                    return ret;
                }
            });
            cityPicker3.setData(data);
            spanCityName.addEventListener('click', function () {
                document.activeElement.blur();
                setTimeout(function () {
                    cityPicker3.show(function (items) {
                        spanCityName.innerHTML = (items[1].Name || '');
                        cityName = items[1].Name || '';
                    });
                }, 500);
            }, false);
        }
    });
    //加载收货地址
    bindUserAddress();

});

function setUserAddressHeight() {
    var listLength = $("#locates li").length;
    if (listLength > 2) {
        $("#locates li").eq(1).nextAll().hide();
        $("#shousuo").html('还有<i>' + (listLength - 2) + '</i>个收货地址');
        $("#shousuo").click(function () {
            if ($("#locates li").eq(2).is(":hidden")) {
                $("#locates li").eq(1).nextAll().show();
                $("#shousuo").addClass('active');
                $("#shousuo").html('收起');
            }
            else {
                $("#locates li").eq(1).nextAll().hide();
                $("#shousuo").removeClass('active');
                $("#shousuo").html('还有<i>' + (listLength - 2) + '</i>个收货地址');
            }
        });
    }
    else {
        $("#shousuo").hide();
        //if (listLength == 0) {
        //    $("#divDeliveryAddress").hide();
        //}
    }
}

$(document).ready(function () {
    //设置Poi检索服务，用于本地检索、周边检索
    searchService = new qq.maps.SearchService({
        //检索成功的回调函数
        complete: function (results) {
            //设置回调函数参数
            var pois = results.detail.pois;
            if (pois == undefined) {
                $("#divMore").html("查询不到数据");
            }
            else {
                $("#nearAddress").empty();
                for (var i = 0, l = pois.length; i < l; i++) {
                    var poi = pois[i];
                    if (typeof (poi.address) != "undefined") {
                        var parText = "\'" + poi.latLng.lat + ',' + poi.latLng.lng + "\'" + "," + "\'" + poi.address + "\'" + "," + "\'" + poi.name + "\'";
                        $("#nearAddress").append('<li data-addr=\"' + poi.name + '\" onclick="choosePosition(' + parText + ')"> <h3>' + poi.name + '</h3><p>' + poi.address + '</p></li>');
                    }
                }
                $("#divAdr").show();
                if (pois.length < 10) {
                    $("#divMore").html("附近没有更多地址了");
                    hasData = false;
                }
                else {
                    //$("#divMore").html("加载更多...");
                }
                pageIndex++;
            }
        },
        //若服务请求失败，则运行以下函数
        error: function () {
            $("#divMore").html("查询不到数据");
        }
    });
    $("#divAdr").hide();
    $("#consigneeAddress").bind('input propertychange', function (e) {
        var keyword = $("#txtKeyWord").val();
        if (keyword != "" && keyword != null) {
            searchKeyword(1);
        }
        e.preventDefault();
    });

    $('#consigneeAddress input').on('focus', function () {
        setTimeout(function () {
            $('#divCurrentPosition').hide();
            $('#divDeliveryAddress').hide();
            $('#divAdr_h').hide();
            $('.cleartext').show();
        }, 300);
    })
    $('#consigneeAddress input').on('blur', function (e) {
        setTimeout(function () {
            if (isBlur) {
                $('.cleartext').hide();
                $('#divCurrentPosition').show();
                $('#divDeliveryAddress').show();
                $('#divAdr_h').show();
            }
        }, 300);
    });
    $('#consigneeAddress .cleartext').on('click', function (e) {
        $("#consigneeAddress input").val('');
        $('.cleartext').hide();
        $('#divCurrentPosition').show();
        $('#divDeliveryAddress').show();
        $('#divAdr_h').show();
        isBlur = false;
        setTimeout(function () {
            isBlur = true;
        }, 400);
    });

    fromLatLng = GetQueryString(positionKeyName) || '';
    curAddress = decodeURIComponent(escape(GetQueryString('curAddress') || ''));
    cityName = decodeURIComponent(escape(GetQueryString('CityInfoName') || ''));
    cityId = decodeURIComponent(escape(GetQueryString('CityInfoId') || ''));
    $('#curAddress').text(curAddress == '' ? '无法定位' : curAddress);
    $('#spanCityName').text(cityName == '' ? '请选择' : cityName);
    $('#curAddress').click(function () {
        choosePosition(fromLatLng);
    });
    //设置位置搜索服务参数
    setSearchServiceOption(curAddress);
});
function setSearchServiceOption(keyword) {
    //根据输入的城市设置搜索范围
    searchService.setLocation(cityName);
    //设置搜索页码
    searchService.setPageIndex(pageIndex);
    //设置每页的结果数
    searchService.setPageCapacity(pageCapacity);
    //根据输入的关键字在搜索范围内检索
    searchService.search(keyword);
}

function geoPosition() {
    var mapkey = $("#hdQQMapKey").val();
    var geolocation = new qq.maps.Geolocation(mapkey, "myapp");
    if (geolocation) {
        geolocation.getLocation(getPositionSuccess, ShowError)
    }
    else {
        $.dialog.tips("请在系统设置中打开“定位服务“允许Mall商城获取您的位置");
    }
}
//获取定位
function getPositionSuccess(position) {
    var lat = position.lat;
    var lng = position.lng;
    fromLatLng = lat + ',' + lng;
    //本地存储当前位置经纬度
    $.addCurPositionCookie(fromLatLng);
    if (position.city) {
        cityName = position.city;
    }
    if (position.addr) {
        curAddress = position.addr;
    } else if (position.district) {
        curAddress = position.district;
    }

    $('#curAddress').text(curAddress == '' ? cityName : curAddress);
    $('#spanCityName').text(cityName == '' ? '请选择' : cityName);
    console.log(fromLatLng);

}
//定位错误
function ShowError(error) {
    if (error) {
        switch (error.code) {
            case error.PERMISSION_DENIED:
                break;
            case error.POSITION_UNAVAILABLE:
                break;
            case error.TIMEOUT:
                break;
            case error.UNKNOWN_ERROR:
                break;
        }
    }
}
//设置搜索的范围和关键字等属性
function searchKeyword(index) {
    if (!isRefresh && index == 2) {
        isRefresh = true;
        return;
    }
    //$("#container").hide();
    //$("#divAdr").show();
    var keyword = $("#txtKeyWord").val();
    if (index == 1) {
        pageIndex = 0;
        hasData = true;
        //$("#nearAddress").empty();
    }
    else {
        if (!hasData) {
            return;
        }
    }
    //设置位置搜索服务参数
    setSearchServiceOption(keyword);
}
//--------滚动时，往下加载数据 start--------------

function scrollLoadData() {
    var scrollTop = $(this).scrollTop();
    var scrollHeight = $(this)[0].scrollHeight;
    var windowHeight = $(this).height();
    if (scrollTop + windowHeight >= scrollHeight) {
        setTimeout(searchKeyword(2), 200);
    }
}
$(window).scroll(function () {
    if (hasData)
        scrollLoadData();
});
//选择地址操作，返回周边门店
function choosePosition(latLng, address, name) {
    $("#Longitude").val(latLng.split(',')[1].trim());
    $("#Latitude").val(latLng.split(',')[0].trim());
    isRefresh = false;
    $("#divAdr").hide();
    $.addCurPositionCookie(latLng);
    location.href = './StoreList';
}
function getNewAddress(address, showCity, street) {
    if (showCity != '') {
        var storeAreaArr = showCity.split(' ');
        if (street != '') {
            storeAreaArr.push(street);
        }
        for (var i = 0; i < storeAreaArr.length; i++) {
            address = address.replace(storeAreaArr[i], '');
        }
    }
    return address;
}

function bindUserAddress() {
    var dataUrl = './GetUserShippingAddressesList';
    $.ajax({
        url: dataUrl,
        type: 'get',
        data: {},
        dataType: 'json',
        success: function (result) {
            if (result.success) {
                var data = result.data;
                var htmlArray = new Array();
                if (data && data.length > 0) {
                    $.each(data, function (idx, item) {
                        if (item.latitude && item.longitude) {
                            var parText = "\'" + item.latitude + ',' + item.longitude + "\'" + "," + "\'" + item.fullRegionName + "\'" + "," + "\'" + item.address + "\'";
                            var addressDetail = item.addressDetail || '';
                            htmlArray.push('<li onclick="choosePosition(' + parText + ')">');
                            htmlArray.push('<h3>' + item.fullRegionName + ' ' + item.address + ' ' + addressDetail + '</h3>');
                            htmlArray.push('<p>' + item.shipTo + '，' + item.phone + '</p>');
                            htmlArray.push('</li>');
                        }
                    });
                }
                else {
                    window.localStorage.setItem('refer', location.href);
                    var addAddressUrl = '/' + areaName + '/Order/EditShippingAddress?isOrder=1';
                    htmlArray.push('<div class="zanwu"><h3>暂无收货地址</h3><a href="' + addAddressUrl + '">+添加收货地址</a></div>');
                }
                $('#locates').html(htmlArray.join(''));

                //设置收货地址高度
                setUserAddressHeight();
            }
            else {
                if (result.msg == 'nologin') {
                    var href = encodeURIComponent(window.location.href);
                    $("#shousuo").hide();
                    $('#locates').html("<li style='padding: 8px 0;'>未登录，无法获取地址信息<a href='/m-Wap/Member/GotoChooseAddress' style='float:right;color:#007AFF;'>去登录</a></li>");
                }
            }
        }
    });
}