var cate;
var couponlist;
var curpagesize = 10, curpageindex = 1, total = -1, lodeEnd = false, shopCategoryId = 0;
var currentProductSku = null;//选中规格属性
var selectskuId;
var price;//选中规格价格
var buyAmount;//选中规格购买数量
var selectsku;
var datalist;
var shopBranchId;
var returnHref;
var shopId;
var skuInfoList;
var typeId;
var skuClick;
var fromLatLng;
var firstLoad = true;
var isNotUseKey = false;
$(function () {
    //页面重置代码
    shopBranchId = $("#bid").val();
    shopId = $("#shopId").val();
    returnHref = "/" + areaName + "/ShopBranch/Index/" + shopBranchId;
    loadbranch();//加载活动    

    $("#stores .pic").height($("#stores .pic").width());
    var touxiangimg = $('#storelogo img').attr('src');
    var beijingBG = 'url(' + touxiangimg + ') no-repeat center center';
    $('.beijing').css({ 'background': beijingBG, 'background-size': '105% 105%' });

    //滚动
    $(".categoryLeft").niceScroll({ cursorwidth: 0, cursorborder: 0 });

    //图片延迟加载
    //   $(".lazyload").scrollLoading({ container: $(".category2") });

//  $('.index-category').height($(window).height() - ($('.index-topimg').height() + $('.index-address').height()));

    //点击切换2 3级分类
    var array = new Array();
    $('.categoryLeft li').each(function (i) {
        if (i == 1 && getUrlParam("keywords") == null) {
            shopCategoryId = Number($(this).attr("id"));
            $(this).addClass("cur").siblings().removeClass("cur");
        }
        array.push($(this).position().top - 56);
    });

    $('.categoryLeft li').click(function () {
        if ($(this).hasClass('cur')) {
            return;
        }
        page = 1;
        var index = $(this).index();
        $('.categoryLeft').delay(200).animate({ scrollTop: array[index] }, 300);
        $(this).addClass("cur").siblings().removeClass("cur");
        shopCategoryId = Number($(this).attr("id"));
        lodeEnd = false;
        curpageindex = 1;
        total = -1;
        //loadData();
        if (shopCategoryId > 0) {
            isNotUseKey = true;
        } else {
            isNotUseKey = false;
        }
        if (index == 1 && $("#couponlist").html().trim()!= '')
            $("#couponlist").removeClass("hidden");
        else
            $("#couponlist").addClass("hidden");
        storeObj.LoadView(shopCategoryId);
    });
    $('.bottom-btn').click(function () {
        $('.index-mask').removeClass('hide');
    });

    $('#cancel').click(function () {
        $('.index-mask').addClass('hide');
    });
    $('#call').click(function () {
        $('.index-mask').addClass('hide');
        location.href = "tel:\\" + $.trim($('.content').html());
    });
    $("#categoryRight").scroll(function () {
        var scrollTop = $(this).scrollTop();
        var scrollHeight = $(this)[0].scrollHeight;
        var windowHeight = $(this).height();
        if (scrollTop + windowHeight >= scrollHeight) {
            curpageindex = curpageindex + 1;
            setTimeout(loadData(2), 200);
        }
    });

    loadData(1);//加载商品 
});

$('#searchtxt').bind('click', function (e) {
    lodeEnd = false;
    curpageindex = 1;
    total = -1;
    $("#-1").addClass("cur").siblings().removeClass("cur");
    shopCategoryId = -1;
    isNotUseKey = false;
    loadData(0);
    e.preventDefault();
    return false;
});
document.addEventListener("keyup",function(event){
	if((event||window.event).keyCode==13){
		document.getElementById("searchtxt").blur();
		lodeEnd = false;
	    curpageindex = 1;
	    total = -1;
	    $("#-1").addClass("cur").siblings().removeClass("cur");
	    shopCategoryId = -1;
	    isNotUseKey = false;
	    loadData(0);
	    event.preventDefault();
	    return false;
	}
});

function InitLatLng() {
    var curPosition = $.getCurPositionCookie() || '';
    curPosition = decodeURIComponent(curPosition);

    if (!curPosition) {
        var mapkey = $("#hdQQMapKey").val();
        var geolocation = new qq.maps.Geolocation(mapkey, "myapp");
        if (geolocation) {
            geolocation.getLocation(function (p) {
                fromLatLng = p.lat + ',' + p.lng;
                $.addCurPositionCookie(fromLatLng);
            }, function (e) {
                $.dialog.tips('无法获取到位置信息,请手动设置位置');
            })
        }
        else {
            $.dialog.tips("请在系统设置中打开“定位服务“允许Mall商城获取您的位置");
        }
    }
    else {
        fromLatLng = curPosition;
    }
}

function loadbranch() {
    InitLatLng();
    $.ajax({
        type: 'post',
        url: '../GetBranchList',
        data: { shopBranchId: $("#shopBranchId").val(), fromLatLng: fromLatLng },
        dataType: 'json',
        cache: true,// 开启ajax缓存
        success: function (result) {
            if (result.data.success == false) {
                $.dialog.tips(data.msg);
            } else {
                var branch = $("#branchlist");
                var coupon = $("#couponlist");
                if (result.data.data && result.data.data.length > 0) {
                    var storedata = result.data.data[0];
                    var shopbranchdata = storedata.ShopBranch;
                    if(shopbranchdata.Status==1){
                    	$.dialog.alert("该门店已冻结",function(){
                    		window.history.go(-1);
                    	});
                    	return;
                    }
                    //超出配送范围提示
                    if (!shopbranchdata.IsAboveSelf && shopbranchdata.IsStoreDelive && shopbranchdata.ServeRadius && shopbranchdata.Distance > shopbranchdata.ServeRadius) {
                        $.dialog.alert("您的定位已经超过该店配送区域");
                    }
                    //门店活动
                    var userhtm = getBranchHtml(storedata);
                    branch.append(userhtm);
                    //});
                    //优惠劵
                    var count = 0;
                    var couponsArr = new Array();
                    //用户有可领取的优惠券，才显示领取界面
                    if (couponlist != undefined && result.data.isCouponsReceived == true) {
                        $.each(couponlist, function (i, model) {
                            if (model.OrderAmount > 0) {
                                couponsArr.push("满" + model.OrderAmount + "减" + model.Price);
                            } else {
                                couponsArr.push(model.Price + "元");
                            }
                            count++;
                        });
                        if (count > 0) {
                            var couponhtml = '<div class="fl"><span>' + count + '张优惠券</span><p>' + couponsArr.join(',') + '</p></div><button class="getCoupon" onclick="showCoupon()">领取</button>'
                            coupon.append(couponhtml);
                            coupon.removeClass("hidden");

                            LoadCoupon();//加载优惠劵
                        }
                    }
                    $('.sale-num').click(function () {
                        var obj = $(this).parent();
                        if (obj.hasClass('active')) {
                            obj.removeClass('active');
                        } else {
                            obj.addClass('active');
                        }
                    });
                }
            }
        }
    });
}

//加载右边数据
function loadData(type) {
    var keywords = "";
    var productId = "";
    if (getUrlParam("keywords") == null) {
        keywords = $("#searchtxt").val();
    }
    else {
        keywords = getUrlParam("keywords");
        if (type == 1) {
            $("#searchtxt").val(keywords);
        }
    }
    if (getUrlParam("productId") != null) {
        productId = getUrlParam("productId");
    }
    if (isNotUseKey == true) {
        keywords='';
    }
    if (lodeEnd)
        return;
    var queryData = {
        pageNo: curpageindex, pageSize: curpagesize, shopCategoryId: shopCategoryId, shopId: $("#shopId").val(), shopBranchId: $("#shopBranchId").val(), keyWords: keywords, productId: productId,type:type, url: "../ProductList"
    }
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
                var data = result.data;
                var databox = $("#productlist");
                if (curpageindex == 1)
                    databox.html('');
                if (data) {
                    total = data.Total;
                    if ($("#searchtxt").val() != "") {
                        $("#-1").removeClass("hidden");
                        //$("#couponlist").addClass("hidden");
                        if (shopCategoryId != 0) {
                            //$("#-1").addClass("cur").siblings().removeClass("cur");
                        }
                    }
                    else {
                        $("#-1").addClass("hidden");
                    }
                    var isTopModel = false;
                    //置顶商品
                    if (data.TopModels && data.TopModels.length > 0) {
                        $.each(data.TopModels, function (i, model) {
                            var userhtml = '<li><ul class="pros">' + getProductHtml(model, data.ProductSaleCountOnOff) + '</ul></li>';
                            databox.append(userhtml);
                        });
                        isTopModel = true;
                    }
                    if (data.Models && data.Models.length > 0) {
                        var typelist = [];
                        $.each(data.Models, function (i, model) {
                            //如果上一页最后二级分类和当前第一个二级分类是相同的 则不加载新二级分类
                            var typehtml = "";
                            var result = $.inArray(model.ProductTypeId, typelist);
                            if (result < 0) {
                                if (model.ProductTypeId != typeId) {
                                    typehtml = '<h3 class="typename">' + model.ProductType + '</h3>';
                                }
                            }
                            var userhtml = typehtml + '<li><ul class="pros">' + getProductHtml(model, data.ProductSaleCountOnOff) + '</ul></li>';
                            databox.append(userhtml);

                            typelist.push(model.ProductTypeId);
                            if (i == 9) {
                                typeId = model.ProductTypeId
                            }
                        });
                        //curpageindex += 1;
                        if (total == data.Models.length)
                            lodeEnd = true;
                    } else {
                        if (!isTopModel) {
                            lodeEnd = true;
                            var noporudct = '<li style="text-align: center;line-height: 30px;color: #494e52;font-size: .11rem;" class="kong"><img src="/Areas/Mobile/Templates/Default/Images/null.png"/><p>此分类暂无商品</p></li>';
                            if (type == 2)
                                noporudct = '<li style="text-align: center;line-height: 30px;color: #494e52;font-size: .11rem;">没有更多商品了</li>';
                            databox.append(noporudct);
                        }
                    }
                    $("img").attr("onerror","this.onerror='';src='/Areas/Mobile/Templates/Default/Images/default-img.png'");
                    loadEndProcess();
                }
            }
        },
        error: function () {
            $.dialog.tips("系统繁忙，请刷新重试");
        }
    });
}
function loadEndProcess() {
    if (lodeEnd) {
        $("#autoLoad").show();
        $("#autoLoad").html("没有更多商品了");
    } else {
        $("#autoLoad").hide();
    }
}
//组合商品数据
function getProductHtml(obj, productSaleCountOnOff) {
    var link = obj.isvirtual ? "vdetails" : "detail";
    var arr = new Array();
    var showUnit = obj.MeasureUnit || "";
    arr.push('<li>');
    arr.push('<a href="/m-wap/branchproduct/' + link + '/' + obj.Id + '?shopBranchId=' + shopBranchId + '"><img src="' + obj.RelativePath + '"/></a>');
    arr.push('<div class="content">');
    arr.push('<a href="/m-wap/branchproduct/' + link + '/' + obj.Id + '?shopBranchId=' + shopBranchId + '"><h3>' + obj.ProductName + '</h3>');
    arr.push('<p>')
    if (productSaleCountOnOff) 
        arr.push('月销 ' + obj.SaleCounts + '' + showUnit + '  ');
    arr.push('好评 ' + obj.HighCommentCount + '</p></a>');
    arr.push('<div class="c-bottom">');
    if (obj.MarketPrice > 0) {
        arr.push('<span class="money"><span>¥</span> ' + obj.MinSalePrice + '</span><span class="money original"><span>¥</span>' + obj.MarketPrice + '</span>');
    }
    else {
        arr.push('<span class="money"><span>¥</span> ' + obj.MinSalePrice + '</span>');
    }
    arr.push('<div class="fr">');
    if (obj.Stock > 0) {
        if (obj.isvirtual) {
            arr.push('<a class="store-btnspan" href="/m-wap/branchproduct/vdetails/' + obj.Id + '?shopBranchId=' + shopBranchId + '">立即购买</a>');
        } else {
            if (obj.HasSku) {
                //多规格
                if (obj.Quantity > 0) {
                    arr.push('<button class="store-btnspan" name=' + obj.Id + ' onclick="showSku(this,' + obj.Id + ')">选择规格<i class="cart-num">' + obj.Quantity + '</i></button>');
                }
                else {
                    arr.push('<button class="store-btnspan" name=' + obj.Id + ' onclick="showSku(this,' + obj.Id + ')">选择规格<i class="cart-num" style="display:none;"></button>');
                }
            }
            else {
                if (obj.Quantity > 0) {
                    //有加入购物车
                    arr.push('<div class="store-btn-buy clearfix"><span class="store-minus" onclick="stockminus(this)"></span><input class="buynum" value="' + obj.Quantity + '" id="' + obj.Id + '_0_0_0" readonly="readonly" /><span class="store-add" onclick="stockadd(this)"></span></div>');
                }
                else {
                    //无加入购物车
                    arr.push('<div class="store-btn-buy clearfix"><span class="store-minus not-visible" onclick="stockminus(this)"></span><input class="buynum not-visible" value="' + obj.Quantity + '" id="' + obj.Id + '_0_0_0" readonly="readonly" /><span class="store-add" onclick="stockadd(this)"></span></div>');
                }
            }
        }
    } else {
        arr.push('<div class="store-btn-buy clearfix">己售罄</div>');
    }
    arr.push('</div></div>');
    arr.push('</li>');
    return arr.join("");
}
function showSku(obj, id) {
    skuClick = obj;//保存点击控件
    event.preventDefault();  
    $.ajax({
        type: 'post',
        url: '../ProductSkuInfoById',
        data: { id: id, shopBranchId: $("#shopBranchId").val() },
        dataType: 'json',
        cache: true,// 开启ajax缓存
        success: function (data) {
            if (data.success == false) {
                $.dialog.tips(data.msg);
            } else {
                //先清空在加载
                $("#pop-sku").html('');
                var databox = $("#pop-sku");
                var sku = GetSkuInfoHtml(data.data);
                databox.append(sku);
                //弹窗
                $("#pop-sku").removeClass("hidden");

                $('.comm-stand .comm-icon').click(function () {
                    $("#pop-sku").addClass("hidden");
                });

                $(".comm-attr span").click(function () {
                    onSkuClick($(this));
                });
            }
        }
    });
}
function GetSkuInfoHtml(data) {
    datalist = data;
    selectsku = [];
    var arr = new Array();
    arr.push('<div class="comm-stand">');
    arr.push('<div class="comm-stand-con">');
    arr.push('<div class="comm-name"><span class="comm-txt">' + data.ProductName + '</span><span class="comm-icon"></span></div>');
    arr.push('<ul class="comm-bd">');
    var count = 0;
    $.each(data.SkuItems, function (i, model) {
        arr.push('<li class="comm-item">');
        arr.push('<div class="comm-title">' + model.AttributeName + '</div>');
        arr.push('<div class="comm-attr">');

        $.each(model.AttributeValue, function (i, attr) {
            if (i == 0) {
                var defaultsku = new Object();
                defaultsku.ValueId = attr.ValueId;
                defaultsku.Value = attr.Value;
                arr.push('<span class="active" data-indexcount=' + count + ' id="' + attr.ValueId + '" value="' + attr.Value + '">' + attr.Value + '</span>');
                selectsku.push(defaultsku);
            }
            else {
                arr.push('<span data-indexcount=' + count + ' id="' + attr.ValueId + '" value="' + attr.Value + '">' + attr.Value + '</span>');
            }
        });
        arr.push('</div>');
        arr.push('</li>');
        count++;
    });

    arr.push('</ul>');
    arr.push('</div>');

    arr.push('<div class="comm-stand-shop">');
    arr.push('<div class="product-into">');

    arr.push('<span class="product-price">¥' + data.DefaultSku.SalePrice + '</span>');
    arr.push('<div class="fr">');
    arr.push('<div class="store-btn-buy clearfix">');
    arr.push('<span class="store-minus" onclick="skustockminus(this)"></span>');
    arr.push('<input class="buynum" readonly="readonly" value="' + data.DefaultSku.CartQuantity + '">');
    arr.push('<span class="store-add"  onclick="skustockadd(this)"></span>');
    arr.push('</div></div>');
    arr.push('</div></div></div></div>');
    //给默认值赋值
    selectskuId = data.DefaultSku.SkuId;
    skuInfoList = [];
    $.each(datalist.Skus, function (i, item) {
        var skuInfo = new Object();
        skuInfo.ValueId = item.SkuId;
        skuInfo.Value = item.CartQuantity;
        skuInfoList.push(skuInfo);
    });
    return arr.join("");
}
//活动
function getBranchHtml(actives) {
    var arr = new Array();
    if (actives.ShopAllActives) {
        var count = 0;
        if (actives.ShopAllActives.ShopActives && actives.ShopAllActives.ShopActives.length > 0) {
            $.each(actives.ShopAllActives.ShopActives, function (i, model) {
                arr.push('<li><i class="sale-icon type1"></i><span>' + model.ActiveName + '</span></li>');
                count += 1;
            });
        }
        if (actives.ShopBranch && actives.ShopBranch.IsFreeMail) {
            arr.push('<li><i class="sale-icon type3"></i><span>满' + actives.ShopAllActives.FreeFreightAmount + '元免运费</span></li>');
            count += 1;
        }
        if (actives.ShopAllActives.ShopCoupons && actives.ShopAllActives.ShopCoupons.length > 0) {
            var couponsArr = new Array();
            $.each(actives.ShopAllActives.ShopCoupons, function (i, model) {
                if (model.OrderAmount > 0) {
                    couponsArr.push("满" + model.OrderAmount + "减" + model.Price);
                } else {
                    couponsArr.push(model.Price + "元");
                }
            });
            arr.push('<li><i class="sale-icon type2"></i> <span>' + couponsArr.join(',') + '</span></li>');
            couponlist = actives.ShopAllActives.ShopCoupons;
            count += 1;
        }
        if (count > 0) {
            arr.push('<span class="sale-num">' + count + '个活动<i></i></span>');
        }
    }
    return arr.join("");
}

//拼凑skuId
function onSkuClick(obj) {
    skuId = "";
    var valueId = obj.attr('id');
    var value = obj.attr('value');
    var index = obj.attr('data-indexcount');
    var selInfo = new Object();
    selInfo.ValueId = valueId;
    selInfo.Value = value;
    var selSku = selectsku;
    selSku[index] = selInfo;

    var selContent = "";
    var isAlSelected = false;
    var product = datalist.ProductId;
    var itemList = datalist.SkuItems;
    if (datalist.SkuItems.length == selSku.length) isAlSelected = true;

    for (var i = 0; i < selSku.length; i++) {
        var info = selSku[i];
        if (info != NaN) {
            selContent += selContent == "" ? info.Value : "," + info.Value;
            skuId += "_" + info.ValueId;
        }
    }

    $.each(datalist.Skus, function (i, item) {
        var found = true;
        for (var i = 0; i < selSku.length; i++) {
            if (selSku[i] == undefined || item.SkuId.indexOf('_' + selSku[i].ValueId) == -1)
                found = false;
        }

        if (found && itemList.length == selSku.length) {
            currentProductSku = item;
            selectskuId = item.SkuId;
            price = item.SalePrice;
            buyAmount = item.CartQuantity > 0 ? item.CartQuantity : 0;//已购买
            //变更价格和购买数量
            $(".product-price").html('¥' + price);
            $(".comm-stand-shop input").val("" + buyAmount + "");

            $.each(skuInfoList, function (i, info) {
                if (info.ValueId == selectskuId && info.Value != buyAmount) {
                    $(".comm-stand-shop input").val("" + info.Value + "");//如果是已购买且窗口未关闭
                }

            });
            return;
        }
    });
    obj.siblings('.active').removeClass("active");
    obj.addClass("active");
}

//查看地图
function onMapClick(latitude, longitude, shopbranchAddress) {
    window.location.href = 'http://apis.map.qq.com/tools/routeplan/eword=' + shopbranchAddress + '&epointx=' + longitude + '&epointy=' + latitude + '?referer=myapp&key=OB4BZ-D4W3U-B7VVO-4PJWW-6TKDJ-WPB77';
}
var storeObj = {
    curView: 0,
    LoadView: function (suff) {
        if (storeObj.curView == suff) {
            return;
        }
        loadData(0);
        storeObj.curView = shopCategoryId;
    }
}

function skustockminus(obj) {
    if (parseInt($(obj).parent().find('input').val()) > 0) {
        var stock = parseInt($(obj).parent().find('input').val()) - 1;
        editcart(stock, 0, "skustockminus", obj);
    }
}
function skustockadd(obj) {
    var stock = 0;
    if ($(obj).parent().find('input').val() != undefined) {
        stock = parseInt($(obj).parent().find('input').val()) + 1;
    }
    editcart(stock, 0, "skustockadd", obj);
}

function stockminus(obj) {
    if (parseInt($(obj).parent().find('input').val()) > 0) {
        var stock = parseInt($(obj).parent().find('input').val()) - 1;
        selectskuId = $(obj).parent().find('input').attr('id');
        editcart(stock, 0, "stockminus", obj);
    }
}
function stockadd(obj) {
    selectskuId = $(obj).parent().find('input').attr('id');;
    var stock = 0;
    if ($(obj).parent().find('input').val() != undefined) {
        stock = parseInt($(obj).parent().find('input').val()) + 1;
    }
    editcart(stock, 0, "stockadd", obj);
}

function canBuy(productId, num) {
    var count = parseInt(num);
    if (isNaN(count) || count < 0)
        return;

    var result = false;
    $.ajax({
        url: '../../branchProduct/canbuy?productId={0}&count={1}'.format(productId, count),
        async: false,
        success: function (data) {
            if (data) {
                result = data.data.result;
                if (data.data.message!="" && $.notNullOrEmpty(data.data.message)) {
                    $.dialog.errorTips(data.data.message);
                }
            }
        }
    });

    return result;
}

function editcart(amount, type, name, obj) {
//  selectskuId = $(obj).parent().find('input').attr('id');
    var productId = selectskuId.split('_')[0];
    //if (!canBuy(productId, amount)) return false;

    var flag = false;
    //登录才能购买
    checkLogin(returnHref, function () {
        if (type > 0) {
            if (!selectskuId || selectskuId.lenght < 1) {
                $.dialog.tips("请选择规格");
                return false;
            }
        }
        $.post('../UpdateCartItem', { skuId: selectskuId, shopBranchId: $("#shopBranchId").val(), count: amount }, function (result) {
            if (!result.success) {
                $.dialog.errorTips(result.msg);
                flag = false;
            }
            else {
                //记录增加的SkuId和数量 再次点击时显示最新数量
                if (skuInfoList != undefined) {
                    $.each(skuInfoList, function (i, item) {
                        if (item.ValueId == selectskuId) {
                            item.Value = amount;
                        }
                    });
                }
                GetBranchCartProducts(shopBranchId);
                flag = true;
            }
            if (flag) {
                if (name == "stockadd") {
                    $(obj).parent().find('input').val(amount);
                    $(obj).parent().find('input').removeClass("not-visible");
                    $(obj).parent().find('.store-minus').removeClass("not-visible");
                }
                if (name == "stockminus") {
                    //保存对应值，再次选中时显示最新
                    $(obj).parent().find('input').val(parseInt($(obj).parent().find('input').val()) - 1);
                    if (amount == 0) {
                        $(obj).parent().find('input').addClass("not-visible");
                        $(obj).parent().find('.store-minus').addClass("not-visible");
                    }
                }
                if (name == "skustockadd") {
                    $(obj).parent().find('input').val(amount);
                    $(skuClick).parent().find('.cart-num').show().text(amount);
                    $(obj).parent().find('input').removeClass("not-visible");
                    $(obj).parent().find('.store-minus').removeClass("not-visible");
                }
                if (name == "skustockminus") {
                    //保存对应值，再次选中时显示最新
                    $(obj).parent().find('input').val(parseInt($(obj).parent().find('input').val()) - 1);
                    $(skuClick).parent().find('.cart-num').text(amount);
                    if (amount == 0) {
                        $(skuClick).parent().find('.cart-num').text('').hide();
                        $(obj).parent().find('input').addClass("not-visible");
                        $(obj).parent().find('.store-minus').addClass("not-visible");
                    }
                }
            }
        });
    }, shopId);
    return flag;
}
//添加/修改购物车
//function updateCartItem() {
//    $.post('/' + areaName + '/cart/UpdateCartItem', { skuId: skuId, count: count }, function (result) {
//        if (result.success) {

//        }
//        else
//            alert(result.msg);
//    });
//}
function showCoupon() {
    $("#pop-coupon").show();
    $('.cover').fadeIn();
    $('.cover,.dialog-close').click(function () {
        $("#pop-coupon").hide();
        $('.cover').fadeOut();
    });
}
function LoadCoupon() {
    var shopId = $("#shopId").val();
    $.ajax({
        type: 'post',
        url: '../GetLoadCoupon',
        data: { shopid: shopId },
        dataType: 'json',
        cache: true,// 开启ajax缓存
        success: function (result) {
            if (result.success == false) {
                $.dialog.tips(result.msg);
            } else {
                //先清空在加载
                $("#coupons").html('');
                var databox = $("#coupons");
                var coupon = GetCouponHtml(result.data);
                databox.append(coupon);
            }
        }
    });
}
//加载优惠劵列表
function GetCouponHtml(data) {
    var arr = new Array();
    if (data && data.length > 0) {
    	var str='';
        $.each(data, function (i, model) {
        	
        	str+='<li class="coupon-item">'+
                '<div class="detail">'+
                    '<p class="couponprice">￥ '+model.Price+'</p>'+
                    '<p class="rule">'+(model.OrderAmount?'满'+model.OrderAmount+'元可用（不含运费）':'金额无限制（不含运费）')+'</p>'+
                    '<span class="btn getCoupon ' + (model.IsUse == 0 ? "" : "disabled") + '" ' + (model.IsUse == 0 ? '' : 'disabled="disabled"') + ' id="' + model.CouponId + '_Coupon" onclick="GetUserCoupon(' + model.CouponId + ')">' + (model.IsUse == 0 ? "领取" : model.IsUse == 1 ? "已领取" : "已领完") + '</span>'+
                '</div>'+
                '<div class="desc">'+
                    '<p>'+(model.UseArea == 0 ? "全场通用" : model.Remark)+'</p>'+
                    '<p>' + model.StartTime + '-' + model.ClosingTime + '</p>'+
                '</div>'+
            '</li>';
        });
    }
    return str;
}
//领取优惠劵
function GetUserCoupon(couponId) {
    var returnurl = "/" + areaName + "/shopbranch/Index/" + shopBranchId;
    checkLogin(returnurl, function () {
        $.ajax({
            type: 'post',
            url: '../GetUserCoupon',
            data: { couponId: couponId },
            dataType: 'json',
            cache: false,
            success: function (data) {
                if (data.success) {
                    $("#" + couponId + "_Coupon").addClass("disabled");
                    $("#" + couponId + "_Coupon").attr("disabled", "disabled");
                    $("#" + couponId + "_Coupon").html("已领取");
                    $.dialog.succeedTips(data.msg);
                } else {
                    //领取成功（能否继续领取？）
                    $.dialog.errorTips(data.msg);
                }
            }
        });
    });
}
function getUrlParam(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)"); //构造一个含有目标参数的正则表达式对象
    var r = window.location.search.substr(1).match(reg);  //匹配目标参数
    if (r != null) return decodeURIComponent(r[2]); return null; //返回参数值
}