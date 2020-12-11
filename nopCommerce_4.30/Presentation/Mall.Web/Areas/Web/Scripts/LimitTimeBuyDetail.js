
var gid = 0;
var skuId = new Array(3);
regReturnUrl = encodeURIComponent(window.location.href);

//chooseResult();
function chooseResult() {
    //已选择显示
    var str = '<em>已选择&nbsp;</em>';
    var len = $('#choose li .dd .selected').length;
    for (var i = 0; i < len; i++) {
        if (i < len - 1) {
            if ($('#choose li .dd .selected a').eq(i).text() != null)
                str += '<strong>“' + $('#choose li .dd .selected a').eq(i).text() + '”</strong>，';
        }
        else {
            if ($('#choose li .dd .selected a').eq(i).text() != null)
                str += '<strong>“' + $('#choose li .dd .selected a').eq(i).text() + '”</strong>';
        }
        var index = parseInt($('#choose li .dd .selected').eq(i).attr('st'));
        skuId[index] = $('#choose li .dd .selected').eq(i).attr('cid');
    }
    //console.log(skuId);
    //$( '#choose-result .dd' ).html( str )

    //请求Ajax获取价格
    if (len === $(".choose-sku").length) {
        var sku = '';
        for (var i = 0; i < skuId.length; i++) {
            sku += ((skuId[i] == undefined ? 0 : skuId[i]) + '_');
        }
        if (sku.length === 0) { sku = "0_0_0_"; }
    }
}

//转换0
function parseFloatOrZero(n) {
    result = 0;
    try {
        if (n.length < 1) n = 0;
        if (isNaN(n)) n = 0;
        result = parseFloat(n);
    } catch (ex) {
        result = 0;
    }
    return result;
}



function LoadActives()
{
    $.post("/product/GetProductActives", { shopid: $('#shopid').val(), productId: $("#gid").val() }, function (data) {
        if (parseFloat(data.freeFreight) > 0) {
            $("#summary-promotion").append("<div class='dt l l01'>促销</div>" +
                    "<div class='promotion-l' style='float:left;width:440px;'>" +
                            "<div style='margin-bottom:5px;'><em class='hl_red_bg'>满免</em><em class='hl_red'>单笔订单满<span>" + data.freeFreight + "</span>元免运费</em></div>" +
                    "</div>");
        }
        if (data.ProductBonus != undefined && data.ProductBonus != null && typeof data.ProductBonus.GrantPrice != "undefined") {
            var html = '<div class="dd d02" style="padding:0;margin:0;"><em class="hl_red_bg">红包</em><em class="hl_red">满<span>' + data.ProductBonus.GrantPrice + '</span>元送红包（' + data.ProductBonus.Count + '个' + data.ProductBonus.RandomAmountStart + '—' + data.ProductBonus.RandomAmountEnd + '元代金券红包）</em></div>';
            if ($(".promotion-l").length > 0) {
                $(html).appendTo(".promotion-l");
            }
            else {
                $('#summary-promotion').append('<div class="dt l l01">促销</div>' + html);
            }
        }
        if (data.FullDiscount != undefined && data.FullDiscount != null) {

            var html = '<div class="dd d02" style="padding:0;margin:0;"><em class="hl_red_bg">满减</em>'
            $(data.FullDiscount.Rules).each(function (index, item) {
                html += '<em>满<span>' + item.Quota + '</span>元减' + item.Discount + (index == data.FullDiscount.Rules.length - 1 ? '' : '，') + '</em>';
            });
            html += '</div>';
            if ($(".promotion-l").length > 0) {
                $(html).appendTo(".promotion-l");
            }
            else {
                $('#summary-promotion').append('<div class="dt l l01">促销</div>' + html);
            }
        }
    });
}

function getHotProduct(productid, categoryid) {
    categoryid = categoryid || '';
    if (categoryId == '') {
        console.log('getHotProduct缺少 categoryid 参数');
        return;
    }
    $.get('/product/GetHotProduct?productId={0}&categoryId={1}'.format(productid, categoryid), function (data) {
        if (data && data.length > 0) {
            var relationProducts = $('#relationProducts');

            var template = relationProducts.children('template').html();
            var html = '';
            for (var i = 0; i < data.length; i++) {
                var product = data[i];
                if (product.id != gid) {//不显示当前商品
                    html += template.formatProperty(product);
                }
            }

            relationProducts.children(':not(template)').remove();
            relationProducts.html(html);
            if (html.length > 0) {//有推荐商品，则显示
                relationProducts.parent().show();
            }
        }
    });
}


$(function() {
    var gid = $('#gid').val();
    var gidName = $('#gidName').val();
    var relativePath = $('#relativePath').val();

    //关注商品
    $("#choose-btn-coll").click(function () {
        checkLogin(function (func) {
            addFavoriteFun(func, gid, gidName, relativePath);
        });
    });
    if ($("#addressChoose").html() != "请选择" && $("#addressChoose").html() != "") {
        calcFreight(gid);
    }

    $('#addressChoose').click(function () {
        $(this).MallDistrict({
            ajaxUrl: '/common/RegionAPI/GetSubRegion',
            closeFn: function () {
                calcFreight(gid);
            }
        });
    });

    loadShipAndLimit();

	var myVideo = document.getElementById("video1");
	$(".fd_gif").click(function () {
	    myVideo.play();
	    $(this).hide();
	    var obj = $(this);
	    myVideo.addEventListener("ended", function () {
	        obj.show();
	        myVideo.style = "background:#000";
	        myVideo.load();
	    },false)
	});
	
	$('#spec-list li').click(function(){
		if($(this).hasClass('video_btn')){
			$('.video-box').show();
			$('.jqzoom').hide();
		}else{
			$('.video-box').hide();
			$('.jqzoom').show();
		}
		$(this).addClass('active').siblings().removeClass('active');
	});
	
    var marketPrice = $('#marketPrice').text();
    $('#choose').MallSku({
        data: { id: $('#mainId').val() },
        productId: $('#gid').val(),
        spec: '.choose-sku',
        itemClass: '.item',
        resultClass: {
            price: '#jd-price',
            stock: '#stockNum',
            chose: '#choose-result .dd'
        },
        ajaxType: 'POST',
        ajaxUrl: '/LimitTimeBuy/GetSkus',      
        skuPosition: 'Details',
        callBack: function (select) {
            $("#rebate em").html((select.Price / marketPrice * 10).toFixed(2));
            $('#stockNum').html(select.TotalCount);
           
        }
    });
	$('#sp-hot-sale .sub span').hover(function () {
        $(this).addClass('cur').siblings().removeClass();
        $(this).parent().next().children().eq($(this).index()).show().siblings().hide();
    });
    LoadActives();

    var gid = $('#gid').val();
    getHotProduct(gid, $('#categoryId').val());
    //倒计时
    var intDiff = parseInt($('#intDiff').val());//倒计时总秒数量
    function timer(intDiff) {
        window.setInterval(function () {
            var day = 0,
                hour = 0,
                minute = 0,
                second = 0;//时间默认值        
            if (intDiff > 0) {
                day = Math.floor(intDiff / (60 * 60 * 24));
                hour = Math.floor(intDiff / (60 * 60)) - (day * 24);
                minute = Math.floor(intDiff / 60) - (day * 24 * 60) - (hour * 60);
                second = Math.floor(intDiff) - (day * 24 * 60 * 60) - (hour * 60 * 60) - (minute * 60);
            }
            if (minute <= 9) minute = '0' + minute;
            if (second <= 9) second = '0' + second;

            if ($("#isStart").val() == "1") {
                $('.countime').html('<div class="dt">距离结束：</div><span class="hour">' + day + '</span><em>天</em> <span class="hour">' + hour + '</span><em>时</em> <span class="hour">' + minute + '</span><em>分</em> <span class="hour">' + second + '</span><em>秒</em>');
            }
            else {
                $('.countime').html('<div class="dt"></div><span class="hour">' + day + '</span><em>天</em> <span class="hour">' + hour + '</span><em>时</em> <span class="hour">' + minute + '</span><em>分</em> <span class="hour">' + second + '</span><em>秒</em>后开始');
            }

            intDiff--;
        }, 1000);
    }
    timer(intDiff);


    if ($(".btn-goshop_in").hasClass("disabled")) {
        $(".btn-goshop_in").attr('disabled', "true");
    }

    //购买数量加减
    $("#buy-num").blur(function () {
        var max = parseInt($("#maxSaleCount").val());
        var stockNum = parseInt($("#stockNum").html());
        var buynum = parseInt($('#buy-num').val())
        if (buynum < 0) {
            $.dialog.errorTips('购买数量必须大于零');
            $('#buy-num').val(1);
        }
        if (buynum > stockNum) {
            $.dialog.errorTips('库存仅余 ' + stockNum + '件');
        }
        if (buynum > max) {
            $.dialog.errorTips('每个ID限购 ' + max + '件');
            // $('#buy-num').val(max);
        }
    });

    $('.wrap-input .btn-reduce').click(function () {
        if (parseInt($('#buy-num').val()) > 1) {
            $('#buy-num').val(parseInt($('#buy-num').val()) - 1);
        }
    });

    $('.wrap-input .btn-add').click(function () {
        var max = parseInt($("#maxSaleCount").val());
        var stockNum = parseInt($("#stockNum").html());
        var buynum = parseInt($('#buy-num').val())
        if (max < buynum + 1) {
            $.dialog.errorTips('每个ID限购 ' + max + '件');
        } else {
            if (buynum +1> stockNum) {
                $.dialog.errorTips('库存仅余 ' + stockNum + '件');
            } else {
                $('#buy-num').val(buynum + 1);
            }
        }
    });

    $("#easyBuyBtn").click(function () {
        var has = $("#has").val();
        var dis = $(this).parent("#choose-btn-append").hasClass('disabled');
        if (has != 1 || dis) return;
        var len = $('#choose li .dd .selected').length;
        if (len === $(".choose-sku").length) {
            var sku = getskuid();
            var num = $("#buy-num").val();
            location.href = "/Order/EasyBuyToOrder?skuId=" + sku + "&count=" + num;
            //   alert('SKUId：'+sku+'，购买数量：'+num);
        } else {
           
            $.dialog.errorTips('请选择商品规格');

        }
    });

    function checkLogin(callBack) {
        var memberId = $.cookie('Mall-User');
        if (memberId) {
            callBack();
        }
        else {
            $.fn.login({}, function () {
                callBack(function () { location.reload(); });
            }, './', '', '/Login/Login');
        }
    }



    //导航切换
    $('.tab .comment-li').click(function () {
        $('#product-detail .mc').hide();
        $(this).addClass('curr').siblings().removeClass('curr');
        $(document).scrollTop($('#comment').offset().top - 52);
    });
    $('.tab .consult-li').click(function () {
        $('#product-detail .mc').hide();
        $(this).addClass('curr').siblings().removeClass('curr');
        $(document).scrollTop($('#consult').offset().top - 52);
    });
    $('.tab .goods-li').click(function () {
        $('#product-detail .mc').show();
        $(this).addClass('curr').siblings().removeClass('curr');
        $(document).scrollTop($('#product-detail').offset().top);
    });

    //导航浮动
    $(window).scroll(function () {
        if ($(document).scrollTop() >= $('#product-detail').offset().top)
            $('#product-detail .mt').addClass('nav-fixed');
        else
            $('#product-detail .mt').removeClass('nav-fixed');
    });


    $("#shopInSearch").click(function () {
        var start = isNaN(parseFloat($("#sp-price").val())) ? 0 : parseFloat($("#sp-price").val());
        var end = isNaN(parseFloat($("#sp-price1").val())) ? 0 : parseFloat($("#sp-price1").val());
        var shopid = $("#shopid").val();

        var keyword = $("#sp-keyword").val();
        if (keyword.length === 0 && start == end) {
            $.dialog.errorTips('请输入关键字或者价格区间');
            return;
        }
        location.href = "/Shop/searchAd?pageNo=1&sid=" + shopid + "&keywords=" + keyword + "&startPrice=" + start + "&endPrice=" + end;
    });

    //立即购买
    $("#justBuy").click(function () {
        var flag = true;
        var max = parseInt($("#maxSaleCount").val());
        if (parseInt($('#buy-num').val()) < 0) {
            $.dialog.errorTips('购买数量必须大于零');
            $('#buy-num').val(1);
            flag = false;
        }
        if (parseInt($('#buy-num').val()) > max) {
            $.dialog.errorTips('每个ID限购 ' + max + '件');
            $('#buy-num').val(max);
            flag = false;
        }
        if (flag) {
            checkLogin(function (func) {
                //检测是否开启了手机号强制绑定
                $.post('/UserInfo/IsConBindSms', null, function (result) {
                    if (result.success) {
                        justBuy(func);
                    } else {
                        $.fn.bindmobile({}, function () {justBuy(func);}, '', '', '/UserInfo/BindSms');
                    }
                });
            });
        }
    });



    function justBuy(callBack) {
        var loading = showLoading();
        chooseResult();
        var has = $("#has").val();
        var dis = $("#justBuy").hasClass('disabled');
        if (has != 1 || dis) {
            loading.close();
            return
        };
        if (dis) {
            loading.close();
            return false;
        }
        var len = $('#choose li .dd .selected').length;
        if (len === $(".choose-sku").length) {
            var sku = getskuid();
            var num = $("#buy-num").val();
            $.post('/LimitTimeBuy/CheckLimitTimeBuy', { skuIds: sku, counts: num }, function (result) {
                if (result.success) {
                    location.href = "/Order/SubmitByProductId?skuIds=" + sku + "&counts=" + num;
                }
                else if (result.maxSaleCount <= 0 && result.remain <= 0) {
                    loading.close();
                    $.dialog.errorTips("亲，活动已结束，不能再买了哦");
                }
                else if (result.remain <= 0) {
                    loading.close();
                    $.dialog.errorTips("亲，限购" + result.maxSaleCount + "件，不能再买了哦");
                }
                else {
                    loading.close();
                    $.dialog.errorTips("亲，限购" + result.maxSaleCount + "件，您最多还能买" + result.remain + "件");
                }
            });
        }
        else {
            loading.close();
            $.dialog.errorTips("请选择商品规格！");

        }
    }

    function checkLogin(callBack) {
        var memberId = $.cookie('Mall-User');
        if (memberId) {
            callBack();
        }
        else {
            $.fn.login({}, function () {
                callBack(function () { location.reload(); });
            }, '', '', '/Login/Login');
        }
    }

    function getskuid() {
        var gid = $("#gid").val();
        var sku = '';
        for (var i = 0; i < skuId.length; i++) {
            sku += ((skuId[i] == undefined ? 0 : skuId[i]) + '_');
        }
        if (sku.length === 0) { sku = "0_0_0_"; }
        sku = gid + '_' + sku.substring(0, sku.length - 1);
        return sku;
    }

    //处理物流目的地
    function calcFreight(gid) {
        //有些调用它没传gid直接调用的改方法，没值情况则要重新取一下值
        if (gid == null) {
            gid = $('#gid').val();
        }
        var isFree = $("#hdFreightType").val();//是否包邮
        var select = $("#addressChoose").data("select");
        if (select != "") {
            var cityid = select.split(',')[1];//收货城市Id
            var streetId = 0;//街道Id
            if (select.split(',').length == 3) {
                streetId = select.split(',')[2];
            } else if (select.split(',').length == 4) {
                streetId = select.split(',')[3];
            }
            //重新计算运费
            if (parseInt(cityid) > 0 && isFree == "0") {
                var totalnum = 0;//商品总数量
                $('.wrap-input .text').each(function (i, e) {
                    if (parseInt($(e).val()) > 0) {
                        totalnum += parseInt($(e).val());
                    }
                });
                if (totalnum == 0) {
                    totalnum = 1;
                }
                var hidskuid = getskuid();
                $.ajax({
                    type: 'post',
                    url: '/product/CalceFreight',
                    data: { cityId: cityid, streetId: streetId, pId: gid, count: totalnum, skuId: hidskuid },
                    dataType: "json",
                    async: false,
                    success: function (data) {
                        if (data.success == true) {
                            $("#spFreight").html("" + data.msg + "");
                        }
                    }
                });
            }
        }
    }

    function addFavoriteFun(callBack, gid, gidName, relativePath) {
        console.log(gid)
        $.post('/Product/AddFavoriteProduct', { pid: gid }, function (result) {
            if (result.success == true) {
                if (result.favorited == true) {
                    $.dialog.tips(result.mess);
                } else {
                    var obj = $('.side-goods-list');
                    console.log(obj)
                    obj.append('<li> <a href="/Product/Detail/' + gid + '" target="_blank"><img src="' + relativePath + '"></a> <p><a href="/Product/Detail/' + gid + '" target="_blank">' + gidName + '</a></p></li>');
                    $("#choose-btn-coll a").html("已收藏商品");
                    $.dialog.succeedTips(result.mess);
                }
                (function () { callBack && callBack(); })();
            }

        });
    }

    function loadShipAndLimit() {
        var _templateId = $("#FreightTemplateId").val();
        var shopId = $("#shopid").val();
        var gid = $('#gid').val();
        var hidskuid = getskuid();
        $.ajax({
            type: 'post',
            url: '/product/GetProductShipAndLimit',
            data: { id: gid, shopId: shopId, templateId: _templateId, skuId: hidskuid },
            dataType: 'json',
            cache: true,// 开启ajax缓存
            success: function (data) {
                if (data) {
                    $("#addressChoose").text(data.shippingAddress);
                    $("#addressChoose").attr("data-select", data.shippingValue);
                    $("#spFreight").text(data.freight);
                    calcFreight(gid);
                }
            },
            error: function (e) {
                //alert(e);
            }
        });
    }
});

function loadGetProductDesc(gid) {
    $.ajax({
        type: 'get',
        url: '/Product/GetProductDesc',
        data: { pid: gid },
        dataType: 'json',
        cache: true,// 开启ajax缓存
        success: function (data) {
            if (data) {
                //console.log(data);
                if (data.DescriptionPrefix.length > 0) {
                    $("#product-html").append($(data.DescriptionPrefix));
                }
                if (data.ProductDescription.length > 0) {
                    var prodes = $(data.ProductDescription);
                    var imgs = $("img", prodes);
                    imgs.each(function () {
                        var _t = $(this);
                        _t.attr("data-url", _t.attr("src"));  //图片延时加载
                        _t.addClass("lazyload");
                        _t.attr("src", "/Areas/Web/images/blank.gif");  //图片延时加载
                    });
                    $("#product-html").append(prodes);

                    //图片延迟加载
                    $(".lazyload").scrollLoading();
                }
                if (data.DescriptiondSuffix.length > 0) {
                    $("#product-html").append($(data.DescriptiondSuffix));
                }
            }
        },
        error: function (e) {
            //alert(e);
        }
    });
}

$(function () {
    gid = $("#gid").val();
    $(document).on("click", ".after-service-img img ", function () {
        $(this).addClass("active").siblings().removeClass("active");
        $(".preview-img img").attr("src", $(this).attr("src"));
        $(this).parent().siblings(".preview-img").show();
    });
    $(".preview-img").click(function () {
        $(this).hide()
    });
    $("body").click(function () {
        $(".preview-img").hide()
    });

    $(".startNtc").hover
    (
        function () {
            $(".scan-code").show();
        },
        function () {
            $(".scan-code").hide();
        }
    );

    loadGetProductDesc(gid);
    GetProductComment();
    loadShopInfo(gid);
});




function loadShopInfo(gid) {
    /* 计算规则：
高 （店铺得分-同行业平均分）/（同行业商家最高得分-同行业平均分）
低 （同行业平均分-店铺得分）/（同行业平均分-同行业商家最高低分）
*/

    var upImage = "up";
    var dowmImage = "down";
    var red = "red";
    var green = "green";

    var productAndDescription = parseFloat($('#ProductAndDescription').val()).toFixed(2);
    var productAndDescriptionPeer = parseFloat($('#ProductAndDescriptionPeer').val()).toFixed(2);
    var productAndDescriptionMax = parseFloat($('#ProductAndDescriptionMax').val()).toFixed(2);
    var productAndDescriptionMin = parseFloat($('#ProductAndDescriptionMin').val()).toFixed(2);
    var productAndDescriptionContrast = 0;
    var productAndDescriptionColor = "";
    if (productAndDescription > productAndDescriptionPeer) {
        if (productAndDescriptionMax - productAndDescriptionPeer == 0) {
            productAndDescriptionContrast = "持平";
        } else {
            productAndDescriptionContrast = (((productAndDescription - productAndDescriptionPeer) / (productAndDescriptionMax - productAndDescriptionPeer)) * 100).toFixed(2) + '%';
        }
        productAndDescriptionColor = red;
    }
    else {
        if (productAndDescriptionPeer - productAndDescriptionMin == 0) {
            productAndDescriptionContrast = "持平";
            productAndDescriptionColor = red;
        }
        else {
            productAndDescriptionContrast = ((productAndDescriptionPeer - productAndDescription) / (productAndDescriptionPeer - productAndDescriptionMin) * 100).toFixed(2) + '%';
            productAndDescriptionColor = green;
        }


    }


    var sellerServiceAttitude = parseFloat($('#SellerServiceAttitude').val()).toFixed(2);
    var sellerServiceAttitudePeer = parseFloat($('#SellerServiceAttitudePeer').val()).toFixed(2);
    var sellerServiceAttitudeMax = parseFloat($('#SellerServiceAttitudeMax').val()).toFixed(2);
    var sellerServiceAttitudeMin = parseFloat($('#SellerServiceAttitudeMin').val()).toFixed(2);
    var sellerServiceAttitudeContrast = 0;
    var sellerServiceAttitudeColor = "";

    if (sellerServiceAttitude > sellerServiceAttitudePeer) {
        if (sellerServiceAttitudeMax - sellerServiceAttitudePeer == 0) {
            sellerServiceAttitudeContrast = "持平";
        } else {
            sellerServiceAttitudeContrast = (((sellerServiceAttitude - sellerServiceAttitudePeer) / (sellerServiceAttitudeMax - sellerServiceAttitudePeer)) * 100).toFixed(2) + '%';
        }

        sellerServiceAttitudeColor = red;
    }
    else {
        if (sellerServiceAttitudePeer - sellerServiceAttitudeMin == 0) {
            sellerServiceAttitudeContrast = "持平";
            sellerServiceAttitudeColor = red;
        } else {
            sellerServiceAttitudeContrast = ((sellerServiceAttitudePeer - sellerServiceAttitude) / (sellerServiceAttitudePeer - sellerServiceAttitudeMin) * 100).toFixed(2) + '%';
            sellerServiceAttitudeColor = green;
        }

    }
    var sellerDeliverySpeed = parseFloat($('#SellerDeliverySpeed').val()).toFixed(2);
    var sellerDeliverySpeedPeer = parseFloat($('#SellerDeliverySpeedPeer').val()).toFixed(2);
    var sellerDeliverySpeedMax = parseFloat($('#SellerDeliverySpeedMax').val()).toFixed(2);
    var sellerDeliverySpeedMin = parseFloat($('#SellerDeliverySpeedMin').val()).toFixed(2);

    var sellerDeliverySpeedContrast = 0;
    var sellerDeliverySpeedColor = "";
    if (sellerDeliverySpeed > sellerDeliverySpeedPeer) {
        if (sellerDeliverySpeedMax - sellerDeliverySpeedPeer == 0) {
            sellerDeliverySpeedContrast = "持平";
        }
        else {
            sellerDeliverySpeedContrast = (((sellerDeliverySpeed - sellerDeliverySpeedPeer) / (sellerDeliverySpeedMax - sellerDeliverySpeedPeer)) * 100).toFixed(2) + '%';
        }
        sellerDeliverySpeedColor = red;
    }
    else {
        if (sellerDeliverySpeedPeer - sellerDeliverySpeedMin == 0) {
            sellerDeliverySpeedContrast = "持平";
            sellerDeliverySpeedColor = red;
        } else {
            sellerDeliverySpeedContrast = ((sellerDeliverySpeedPeer - sellerDeliverySpeed) / (sellerDeliverySpeedPeer - sellerDeliverySpeedMin) * 100).toFixed(2) + '%';
            sellerDeliverySpeedColor = green;
        }

    }


    var productAndDescriptionImage = productAndDescription >= productAndDescriptionPeer ? upImage : dowmImage;
    var sellerServiceAttitudeImage = sellerServiceAttitude >= sellerServiceAttitudePeer ? upImage : dowmImage;
    var sellerDeliverySpeedImage = sellerDeliverySpeed >= sellerDeliverySpeedPeer ? upImage : dowmImage;
    var showShop = $('#showShop').val();
    showShop = !!showShop;
    //先同步客服信息
    $.ajax({
        type: 'get',
        url: '../CustmerServices',
        data: { shopid: $("#shopid").val() },
        async: false,
        success: function (data) {
            data = "<dd>" + data + "</dd>"
            $("#online-service").html(data);
        },
        error: function (e) {
            //alert(e);
        }
    });

    $.ajax({
        type: 'get',
        url: '../GetShopInfo',
        data: { sid: $("#shopid").val(), productId: gid },
        dataType: 'json',
        cache: false,// 开启ajax缓存
        success: function (data) {
            var html = "";
            //------服务支持模块---------
            var strFuWu = "";//服务支持
            strFuWu += '<h3>服务支持：</h3>';

            //strFuWu += '<dl class="pop-ensure new"><dt><a href="/Article/Category"><img src="/Images/service5.png">  随时退</a></dt></dl>';
            //strFuWu += '<dl class="pop-ensure new"><dt><a href="/Article/Category"><img src="/Images/service2.png">  购买后不可退</a></dt></dl>';
            if (data.isSevenDayNoReasonReturn) {
                strFuWu += '<dl class="pop-ensure new"><dt><a href="/Article/Category"><img src="/Images/service1.png">  七天退换货</a></dt></dl>';
            }
            if (data.isCustomerSecurity) {
                strFuWu += '<dl class="pop-ensure new"><dt><a href="/Article/Category"><img src="/Images/service4.png">  消费者保障服务</a></dt></dl>';
            }
            //if (data.timelyDelivery) {
            //    strFuWu += '<dl class="pop-ensure"><dt><a href="/Article/Category"><img src="/Images/TimelyDelivery.jpg"> 及时发货</a></dt></dl>';
            //}
            var canSelfTake = $('#canSelfTake').val() == 'true';
            var selfstrtyle = canSelfTake ? "" : " style=\"display:none;\"";
            strFuWu += '<dl class="pop-ensure" id="dlCanSelfTake"' + selfstrtyle + '><dt><img src="/Images/selftake2.png"> 到店自提</dt></dl>';

            var isCash = ($("#isCashOnDelivery").attr("isCash") == "1") ? true : false;//另一个异步加载是否支持货到付款是否已加载已支持
            var codstrtyle = isCash ? "" : " style=\"display:none;\"";
            strFuWu += '<dl class="pop-ensure" id="dlCashOnDelivery"' + codstrtyle + '><dt><img src="/Images/cashOnDelivery.jpg"> 货到付款</dt></dl>';

            strFuWu += $(".j_RefundService").html();
            var fuwustyle = " style=\"display:none;\"";
            if (data.isSevenDayNoReasonReturn || data.isCustomerSecurity || data.timelyDelivery || canSelfTake || strFuWu) {
                fuwustyle = "";
            }
            strFuWu = "<div id=\"divFuWu\"" + fuwustyle + ">" + strFuWu + "</div>";
            //-------------

            if (data && data.isSelf == true) {
                var brandLogo = data.brandLogo;
                if (brandLogo == "") {
                    html += '<dl id="seller" class="shopname text-overflow"><span><a href="/Shop/Home/' + data.id + '">平台直营</a></span><dt>自营</dt></dl>';
                }
                else {
                    html += '<dl id="seller"><a href="/Search/SearchAd/?b_id=' + data.brandId + '"> <img  width="65" height="35" src="' + data.brandLogo + '"  /> </a></dl><dl class="shopname text-overflow"><span><a href="/Shop/Home/' + data.id + '">平台直营</a></span><dt>自营</dt></dl>';
                }
                html += '<dl id="hotline">' + $("#online-service").html() + '</dl>';
                html += strFuWu;//服务
            } else {
                var shopinfo = '<dd><a target="_blank" style="color:#222;" href="/Shop/Home/' + data.id + '">' + data.name + '</a></dd>';
                if (showShop) {
                    shopinfo = '<dd>' + data.name + '</dd>';
                }
                html = '<dl id="seller" class="shopname text-overflow">' +
                    shopinfo +
                    '</dl>';
                html += '<div id="evaluate-detail">' +
                    '<div class="mc">' +
                    '<dl >' +
                    '<dt>商品</dt>' +
                    '<dd title="（商家得分-行业平均得分）/（行业商家最高得分-行业平均得分）">' +
                    '<span class="' + productAndDescriptionColor + '">' + productAndDescription + '<i class="' + productAndDescriptionImage + '"></i></span>' +
                    //'<em class="' + productAndDescriptionColor + '">' + productAndDescriptionContrast + '</em>' +
                    '</dd>' +
                    '</dl>' +
                    '<dl>' +
                    '<dt>物流</dt>' +
                    '<dd title="（行业平均得分-商家得分）/（行业平均得分-行业商家最低得分）">' +
                    '<span class="' + sellerDeliverySpeedColor + '">' + sellerDeliverySpeed + '<i class="' + sellerDeliverySpeedImage + '"></i></span>' +
                    //'<em class="' + sellerDeliverySpeedColor + '">' + sellerDeliverySpeedContrast + '</em>' +
                    '</dd>' +
                    '</dl>' +
                    '<dl>' +
                    '<dt>服务</dt>' +
                    '<dd title="（行业平均得分-商家得分）/（行业平均得分-行业商家最低得分）">' +
                    '<span class="' + sellerServiceAttitudeColor + '">' + sellerServiceAttitude + '<i class="' + sellerServiceAttitudeImage + '"></i></span>' +
                    //'<em class="' + sellerServiceAttitudeColor + '">' + sellerServiceAttitudeContrast + '</em>' +
                    '</dd>' +
                    '</dl>' +
                    '</div>';
                if (showShop)
                    html += '<div id="enter-shop" style="display:none">';
                else
                    html += '<div id="enter-shop">';
                html += '<a class="mr10" target="_blank" href="/Shop/Home/' + data.id + '">进入店铺</a>' +
                    '<a href="javascript:addFavorite(' + data.id + ')">收藏店铺</a>' +
                    '</div>'+
                    '</div>';
                
                html += '<dl id="hotline">' + $("#online-service").html() + '</dl>';
				if (data.cashDeposits > 0) {
                    html += '<dl class="pop-money"><dt>资质：</dt><dd><span title="该卖家已缴纳保证金' + data.cashDeposits + '元">' + data.cashDeposits + '元</span></dd></dl>'
                }
                html += strFuWu;//服务

            }
            $("#brand-bar-pop").show().append(html);
        },
        error: function (e) {
            //alert(e);
        }
    });
    return;

}