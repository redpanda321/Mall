//页面级参数
var positionKeyName = 'curPosition';
var firstLoad = true;
var fromLatLng = '';
var curpagesize = 10, curpageindex = 1, total = -1, lodeEnd = false;

$(function () {
    $("html").removeAttr("style");
    freshStoresData();
    resetDom();

    $("#store_list").on("click", ".products a", function () {
        location = shopBranchIndexUrl + "/Index/" + $(this).attr("branchid") + "?productId=" + $(this).attr("productid");
    }).on("click", ".sale-num", function () {
        var obj = $(this).parent();
        if (obj.hasClass('active')) {
            obj.removeClass('active');
        } else {
            obj.addClass('active');
        }
    });
});
function resetDom() {
    $(".products img").css('height', $(".products li:first-child img").width());
}

function freshStoresData() {
    //如果有参数
    var curPositionPara = GetQueryString(positionKeyName) || '';
    if (curPositionPara != '' && firstLoad) {
        firstLoad = false;
        //本地存储当前位置经纬度
        fromLatLng = curPositionPara;
        $.addCurPositionCookie(fromLatLng);
    }
    var curPosition = $.getCurPositionCookie() || '';
    curPosition = decodeURIComponent(curPosition);

    if (!curPosition) {
        var mapkey = $("#hdQQMapKey").val();
        var geolocation = new qq.maps.Geolocation(mapkey, "myapp");
        if (geolocation) {
            geolocation.getLocation(getPositionSuccess, ShowError)
        }
        else {
            $.dialog.tips("请在系统设置中打开“定位服务“允许Mall商城获取您的位置");
        }
    }
    else {
        fromLatLng = curPosition;
        loadStoresData();
    }
}
//获取定位
function getPositionSuccess(position) {
    var lat = position.lat;
    var lng = position.lng;
    fromLatLng = lat + ',' + lng;
    //本地存储当前位置经纬度
    $.addCurPositionCookie(fromLatLng);

    console.log(fromLatLng);
    loadStoresData();
}
//定位错误
function ShowError(error) {
    //switch (error.code) {
    //    case error.PERMISSION_DENIED:
    //        break;
    //    case error.POSITION_UNAVAILABLE:
    //        break;
    //    case error.TIMEOUT:
    //        break;
    //    case error.UNKNOWN_ERROR:
    //        break;
    //}
    $.dialog.tips('无法获取您的当前位置，请确认是否开启定位服务');
}
//加载周边门店数据
function loadStoresData() {
    if (lodeEnd)
        return;
    var queryData = {
        pageNo: curpageindex, pageSize: curpagesize, shopId: shopId, fromLatLng: fromLatLng, productId: productId, url: storeByProductUrl
    }
    if (fromLatLng == "" || fromLatLng == undefined) {
        $.dialog.tips('无法获取您的当前位置，请确认是否开启定位服务');
        return;
    }
    $.ajax({
        type: "GET",
        url: queryData.url,
        data: queryData,
        async: false,
        dataType: "json",
        success: function (result) {
            if (result.success == false) {
                $(".no_sotre").hide();
                $("#sansearchstroe").hide();
                $("#imgnonstore").hide();
                var addUrl = './StoreListAddress';
                $('#CurrentAddress').text('无法定位');
                $('#CurrentAddress').click(function () {
                    location.href = addUrl;
                });
                $.dialog.tips(result.msg);
            } else {
                var data = result.data;
                var databox = $("#store_list");
                var branchIds = new Array();
                if (curpageindex == 1)
                    databox.empty();
                if (data) {
                    total = data.Total;
                    if (data.Models && data.Models.length > 0) {
                        $.each(data.Models, function (i, model) {
                            //门店、活动
                            var userhtm = getStoreHtml(model);
                            databox.append(userhtm);
                            branchIds.push(model.ShopBranch.Id);
                        });
                        curpageindex += 1;
                        $(".no_sotre").hide();
                        $("#sansearchstroe").hide();
                        $("#imgnonstore").hide();
                        //当前位置
                        var addUrl = './StoreListAddress?' + positionKeyName + '=' + fromLatLng + '&CityInfoId=' + data.CityInfo.Id + '&CityInfoName=' + encodeURIComponent(data.CityInfo.Name) + '&curAddress=' + encodeURI(data.CurrentAddress);
                        $('#CurrentAddress').text(data.CurrentAddress);
                        $('#CurrentAddress').click(function () {
                            location.href = addUrl;
                        });
                        //商品、销量
                        LoadProductAndSaleCount(branchIds);
                        //绑定门店跳转事件
                        bindbranchNameClick();
                        //设置商品标签高度，活动点击事件
                        resetDom();
                        if (total == data.Models.length)
                            lodeEnd = true;
                    } else {
                        $("#sansearchstroe").hide();
                        if (curpageindex == 1) {
                            lodeEnd = true;
                            $.dialog.tips("未匹配到任何门店");
                            $(".no_sotre").show();
                            $("#imgnonstore").show();
                        } else {
                            lodeEnd = true;
                        }
                    }
                    loadEndProcess();
                }
            }
        },
        error: function () {
            $.dialog.tips("系统繁忙，请刷新重试");
        }
    });
}
//获取地址参数，当从详细页过来的时候，需要商家IDshopid
function getQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
    var r = window.location.search.substr(1).match(reg);
    if (r != null) return unescape(r[2]); return null;
}
//组合门店数据
function getStoreHtml(item) {
    var obj = item.ShopBranch;//门店数据
    var arr = new Array();
    arr.push('<li id="branch' + obj.Id + '" class="list-li clearfix">');
    if (obj.IsRecommend && obj.ServeRadius >= obj.Distance) {
        arr.push('<span class="remm-bg"></span>');
    }
    arr.push('<i class="cart-num">1</i>');
    arr.push('<img class="store-img" src="' + obj.ShopImages + '"/>');
    arr.push('<div class="li-right">');
    arr.push('<h3 data=' + obj.Id + ' class="branchName">' + obj.ShopBranchName + '</h3>');
    arr.push('<div class="rowline clearfix">');
    arr.push('<span class="fl">¥ ' + obj.DeliveTotalFee + '起送<i class="space-line"></i>配送费 ¥ ' + obj.DeliveFee + '</span>');
    arr.push('<span class="fr">' + obj.DistanceUnit + '</span>');
    arr.push('</div>');
    arr.push('<div class="rowline clearfix">');
    arr.push('<span id="branchCommentScore' + obj.Id + '" class="fl" style="color:#ff9800;"><span class="hdstars"><i class="hdstars-active"></i></span> 5</span><span class="fl"><i class="space-line"></i></span>');
    arr.push('<span id="branchSaleCount' + obj.Id + '" class="fl">月售 0</span>');
    arr.push('<span class="fr">');
    if (obj.IsAboveSelf) {
        arr.push('<span class="tag">自提</span>');
    }
    if (obj.IsStoreDelive) {
        arr.push('<span class="tag">门店配送</span>');
    }
    arr.push('</span>');
    arr.push('</div>');
    if (item.ShopAllActives) {
        //活动
        var activeHtml = getActiveHtml(item);
        arr.push(activeHtml);
    }
    arr.push('<ul  class="products">');
    arr.push('</ul >');
    arr.push('</div></li >');
    return arr.join("");
}
//加载门店活动数据
function getActiveHtml(actives) {
    var arr = new Array();
    var count = 0;
    arr.push('<ul class="sales">');
    if (actives.ShopAllActives.ShopActives && actives.ShopAllActives.ShopActives.length > 0) {
        $.each(actives.ShopAllActives.ShopActives, function (i, model) {
            arr.push('<li><i class="sale-icon type1"></i><span>' + model.ActiveName + '</span></li>');
            count += 1;
        });
    }
    if (actives.ShopBranch && actives.ShopBranch.FreeMailFee > 0) {
        arr.push('<li><i class="sale-icon type3"></i><span>满' + actives.ShopBranch.FreeMailFee + '元免运费</span></li>');
        count += 1;
    }
    if (actives.ShopAllActives && actives.ShopAllActives.ShopCoupons && actives.ShopAllActives.ShopCoupons.length > 0) {
        var couponsArr = new Array();
        $.each(actives.ShopAllActives.ShopCoupons, function (i, model) {
            //couponsArr.push(model.CouponName);
            if (model.OrderAmount > 0) {
                couponsArr.push("满" + model.OrderAmount + "减" + model.Price);
            }
            else {
                couponsArr.push(model.Price + "元");
            }
        });
        arr.push('<li> <i class="sale-icon type2"></i> <span>' + couponsArr.join(',') + '</span></li>');
        count += 1;
    }
    if (count > 0) {
        arr.push('<span class="sale-num">' + count + '个活动<i></i></span>');
    }
    arr.push('</ul>');
    return arr.join("");
}
//加载门店商品及销量
function LoadProductAndSaleCount(branchIds) {
    var queryData = {
        ids: branchIds.join(','), productId: productId, url: storeByProductSaleCountUrl
    }
    var productSaleCountOnOff = $("#productSaleCountOnOff").val();
    $.ajax({
        type: "GET",
        url: queryData.url,
        data: queryData,
        async: false,
        dataType: "json",
        success: function (result) {
            if (result.success == false) {
                $.dialog.tips(result.msg);
            } else {
                if (result.data) {
                    var productUrl = '/' + areaName + '/ShopBranch/Index/';//门店首页
                    productSaleCountOnOff = result.productSaleCountOnOff;
                    $("#productSaleCountOnOff").val(productSaleCountOnOff);
                    $.each(result.data, function (i, model) {
                        var $branchProduct = $('#branch' + model.branchId + ' .products');
                        var arr = new Array();
                        var index = 0;
                        $.each(model.products, function (i, p) {
                            if (i < 4) {
                                arr.push('<li> <a href="' + productUrl + model.branchId + '?productId=' + p.Id + '"><img src="' + p.ImagePath + '/1_100.png" /><span>¥ ' + p.MinSalePrice + '</span></a></li>');
                            } else {
                                index++;
                            }
                        });
                        if (index > 0) {
                            arr.push('<li><span>更多' + index + '件</span></li>');
                        }
                        $branchProduct.html(arr.join(''));
                        if (model.cartQuantity > 0) {
                            $('#branch' + model.branchId + ' .cart-num').text(model.cartQuantity);
                        }
                        else {
                            $('#branch' + model.branchId + ' .cart-num').hide();
                        }
                        if (productSaleCountOnOff == true) {
                            $('#branchSaleCount' + model.branchId).text('月售 ' + model.saleCountByMonth);
                        } else {
                            $('#branchSaleCount' + model.branchId).hide();
                        }
                        $('#branchCommentScore' + model.branchId).html('<span class="hdstars"><i class="hdstars-active" style=width:'+(model.CommentScore/5*100)+'%></i></span> '+model.CommentScore);
                    });
                }
            }
        },
        error: function () {
            $.dialog.tips("系统繁忙，请刷新重试");
        }
    })
}
//
function bindbranchNameClick() {
    $('.branchName').click(function () {
        location.href = shopBranchIndexUrl + "/Index/" + $(this).attr('data');
    });
}

$(window).scroll(function () {
    totalheight = parseFloat($(window).height()) + parseFloat($(window).scrollTop());     //浏览器的高度加上滚动条的高度
    if ($(document).height() - 10 <= totalheight)     //当文档的高度小于或者等于总的高度的时候，开始动态加载数据
    {
        setTimeout(loadStoresData(), 200);
    }
});
//查看地图
function onMapClick(latitude, longitude, shopbranchAddress) {
    window.location.href = 'http://apis.map.qq.com/tools/routeplan/eword=' + shopbranchAddress + '&epointx=' + longitude + '&epointy=' + latitude + '?referer=myapp&key=OB4BZ-D4W3U-B7VVO-4PJWW-6TKDJ-WPB77';
}
function loadEndProcess() {
    if (lodeEnd) {
        $("#divMore").show();
        $("#divMore").html("没有更多数据了");
    } else {
        $("#divMore").hide();
    }
}