regReturnUrl = encodeURIComponent(window.location.href);
$(function () {
    var myVideo = document.getElementById("video1");
    $(".fd_gif").click(function () {
        myVideo.play();
        $(this).hide();
        var obj = $(this);
        myVideo.addEventListener("ended", function () {
            obj.show();
            myVideo.style = "background:#000";
            myVideo.load();
        }, false)
    });

    $('#spec-list li').click(function () {
        if ($(this).hasClass('video_btn')) {
            $('.video-box').show();
            $('.jqzoom').hide();
        } else {
            $('.video-box').hide();
            $('.jqzoom').show();
        }
        $(this).addClass('active').siblings().removeClass('active');
    });
    var gid = $('#gid').val(),
        categoryId = $('#categoryId').val();
    $("#hidskuId").val(gid + '_0_0_0');
    var gidName = $('#gidName').val();
    var relativePath = $('#relativePath').val();
    // 获取商品需要实时刷新的数据
    loadProductNeedRefreshInfo();

    commentImgPrv();
    navChange();


    $('#sp-hot-sale .sub span').hover(function () {
        $(this).addClass('cur').siblings().removeClass();
        $(this).parent().next().children().eq($(this).index()).show().siblings().hide();
    });



    $("#buy-num").blur(function () {
        checkBuyNum();
        //if(parseInt($('#buy-num').val())>parseInt($("#stockProduct").html()))
        //{
        //    $.dialog.errorTips('不能大于库存数量');
        //    $('#buy-num').val(parseInt($("#stockProduct").html()));
        //    return false;
        //}
        calcFreight(gid);
    });
    //购买数量加减
    $('.wrap-input .btn-reduce').click(function () {
        if (parseInt($('#buy-num').val()) > 1) {
            $('#buy-num').val(parseInt($('#buy-num').val()) - 1);
        }
        calcFreight(gid);
    });
    $('.wrap-input .btn-add').click(function () {

        $('#buy-num').val(parseInt($('#buy-num').val()) + 1);

        checkBuyNum();
        calcFreight(gid);
        //alert(parseInt($('#buy-num').val())+1)
    });
    $("#easyBuyBtn").click(function () {
        var SkuId = $("#hidskuId").val();
        if (parseInt($('#buy-num').val()) > parseInt($("#stockProduct").html())) {
            //$.dialog.errorTips('不能大于库存数量');
            $('#buy-num').val(parseInt($("#stockProduct").html()));
            return false;
        }
        var has = $("#has").val();
        var dis = $(this).parent("#choose-btn-append").hasClass('disabled');
        if (has != 1 || dis) return;
        var len = $('#choose li .dd .selected').length;
        if (len === $('#choose').find(".choose-sku").length) {
            if (checkBuyNum()) {
                var num = $("#buy-num").val();
                location.href = "/Order/EasyBuyToOrder?skuId=" + SkuId + "&count=" + num;
                //   alert('SKUId：'+sku+'，购买数量：'+num);
            }
        } else {
            $.dialog.errorTips('请选择商品规格');
        }
    });

    $("#shopInSearch").click(function () {
        Search();
    });

    $('#sp-keyword,#sp-price,sp-price1').keydown(function (e) {
        if (e.keyCode == 13) {
            Search();
        }
    });

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

    //自营店产品详情跟普通店铺不一样显示
    changeShowView(gid);

});

//服务支持div整块是否显示或隐藏
function ShowHideDivFuWu() {
    if ($("#divFuWu").length > 0) {
        //是否支持货到付款
        if ($('#canSelfTake').val() == 'true' || $('#canSelfTake').val() == 'True')
            $("#dlCanSelfTake").show();
        else
            $("#dlCanSelfTake").hide();

        var isFuWu = false;
        $("#divFuWu dl.pop-ensure").each(function () {
            if ($(this).is(':visible')) {
                isFuWu = true;//服务支持里有一个显示就div显示
                return false;
            }
        });

        if (isFuWu)
            $("#divFuWu").show();
        else
            $("#divFuWu").hide();
    }
}

//处理物流目的地
function calcFreight(gid) {
    //有些调用它没传gid直接调用的改方法，没值情况则要重新取一下值
    if (gid == null) {
        gid = $('#gid').val();
    }

    var productType = $("#productType").val();
    if (productType == 1) {
        return;
    }
    var isFree = $("#hdFreightType").val();//是否包邮
    var select = $("#addressChoose").data("select");
    var isEnableCashOnDelivery = $("#isEnableCashOnDelivery").val();
    if (select != "") {
        var cityid = select.split(',')[1];//收货城市Id
        var streetId = 0;//街道Id
        if (select.split(',').length == 3) {
            streetId = select.split(',')[2];
        } else if (select.split(',').length == 4) {
            streetId = select.split(',')[3];
        }

        //判断是否支持货到付款
        if (select.split(',').length > 1 && isEnableCashOnDelivery == 1) {
            var addressIndex;
            if (select.split(',').length == 2) {
                addressIndex = 1;
            }
            else {
                addressIndex = 2;
            }
            countyid = select.split(',')[addressIndex];
            $.post("../IsCashOnDelivery", { countyId: countyid }, function (result) {
                if (result.success) {
                    $("#isCashOnDelivery").attr("isCash", "1");//说明已支持货到付款,加属性是便于另异步区块后加载在那边已判断
                    $("#dlCashOnDelivery").show();
                }
                else {
                    $("#isCashOnDelivery").attr("isCash", "0");//说明不支持
                    $("#dlCashOnDelivery").hide();
                }

                ShowHideDivFuWu();//服务支持div整块是否显示或隐藏
            })
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
            var hidskuid = $("#hidskuId").val();
            $.ajax({
                type: 'post',
                url: '../CalceFreight',
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

function getProductActives(shopid, productid) {
    var productType = $("#productType").val();
    $.post("/product/GetProductActives", { shopid: shopid, productId: productid }, function (data) {
        if (productType==0&&parseFloat(data.freeFreight) > 0) {
            $("#summary-promotion").append("<div class='dt l l01'>促销</div>" +
                "<div class='promotion-l' style='float:left;width:440px;'>" +
                "<div style='margin-bottom:5px;'><em class='hl_red_bg'>满免</em><em class='hl_red'>单笔订单满<span>" + data.freeFreight + "</span>元免运费</em></div>" +
                "</div>");
        }
        if (data.ProductBonus != undefined && data.ProductBonus != null && typeof data.ProductBonus.GrantPrice != "undefined") {
            var html = '<div class="dd d02" style="padding:0;margin:0; margin-bottom:5px;"><em class="hl_red_bg">红包</em><em class="hl_red">满<span>' + data.ProductBonus.GrantPrice + '元</span>送红包（' + data.ProductBonus.Count + '个' + data.ProductBonus.RandomAmountStart + '—' + data.ProductBonus.RandomAmountEnd + '元代金券红包）</em></div>';
            if ($(".promotion-l").length > 0) {
                $(html).appendTo(".promotion-l");
            }
            else {
                $('#summary-promotion').append('<div class="dt l l01">促销</div>' + html);
            }
        }
        if (data.FullDiscount != undefined && data.FullDiscount != null) {
            var html = '<div class="dd d02" style="padding:0;margin:0;"><em class="hl_red_bg">满减</em>'
            html += '<em class="hl_red" style="color:#2d2d2d;">'
            $(data.FullDiscount.Rules).each(function (index, item) {
                html += '满<span>' + item.Quota.toFixed(2) + '元</span>减' + item.Discount.toFixed(2) + (index == data.FullDiscount.Rules.length - 1 ? '' : '，');
            });
            html += '</em>';
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

//加入购物车
function InitCartUrl() {
    $("#InitCartUrl").click(function (e) {
        var SkuId = $("#hidskuId").val();
        var has = $("#has").val();
        var dis = $(this).parent().hasClass('disabled');
        if (has != 1 || dis) return;

        if (parseInt($('#buy-num').val()) > parseInt($("#stockProduct").html())) {
            //$.dialog.errorTips('不能大于库存数量');
            $('#buy-num').val(parseInt($("#stockProduct").html()));
            return false;
        }

        var len = $('#choose li .dd .selected').length;
        if (len === $('#choose').find(".choose-sku").length) {
            if (checkBuyNum()) {
                var num = $("#buy-num").val();
                var loading = showLoading();
                $.ajax({
                    type: 'POST',
                    url: "/cart/AddProductToCart?skuId=" + SkuId + "&count=" + num,
                    dataType: 'json',
                    success: function (data) {
                        loading.close();
                        if (data.success == true) {
                            var cartOffset = $("#right_cart").offset(),
                                h = $(document).scrollTop();
                            flySrc = $('#spec-list li').first().find('img')[0].src,
                                flyer = $('<img class="cart-flyer" src="' + flySrc + '"/>');
                            flyer.fly({
                                start: {
                                    left: e.pageX,
                                    top: e.pageY - h - 30
                                },
                                end: {
                                    left: cartOffset.left,
                                    top: cartOffset.top - h + 30,
                                    width: 20,
                                    height: 20
                                },
                                onEnd: function () {
                                    this.destory(); //移除dom 
                                    refreshCartProducts();
                                }
                            });
                        } else {
                            loading.close();
                            $.dialog.errorTips(data.msg);
                        }
                    },
                    error: function (e) {
                        //loading.close();
                        $.dialog.errorTips('加入购物车失败');
                    }
                });
            }
        } else {
            $.dialog.errorTips("请选择商品规格！");
        }
    });
}

function canBuy(productId) {
    var count = parseInt($("#buy-num").val());
    if (isNaN(count) || count <= 0)
        return;

    var result = false;
    $.ajax({
        url: '../canbuy?productId={0}&count={1}'.format(productId, count),
        async: false,
        success: function (data) {
            if (data && data.message != "商品已售罄") {
                result = data.result;
                if ($.notNullOrEmpty(data.message))
                    $.dialog.errorTips(data.message);
            }
        }
    });

    return result;
}

function loadGroup() {
    if ($('.products-group.current .p-group-child li').length > 3) {
        $('.group-arrow').show();
    } else {
        $('.group-arrow').hide();
    }

    var groupPage = 1;
    var groupI = 3; //每版放3个图片 
    var groupContent = $(".products-group.current .p-group-child-box");
    var groupContentList = $(".products-group.current .p-group-child");
    groupContentList.animate({ top: '0px' }, 1);
    var groupHeight = groupContent.height();
    var groupLen = groupContent.find("li").length;
    var groupPageCount = Math.ceil(groupLen / groupI);
    //向后 按钮  
    $(".group-arrow-next").click(function () {
        if (!groupContentList.is(":animated")) {
            if (groupPage == groupPageCount) {
                groupContentList.animate({ top: '0px' }, 300);
                groupPage = 1;
            } else {
                groupContentList.animate({ top: '-=' + groupHeight }, 300);
                groupPage++;
            }
        }
    });
    //往前 按钮  
    $(".group-arrow-pre").click(function () {
        if (!groupContentList.is(":animated")) {
            if (groupPage == 1) {
                groupContentList.animate({ top: '-=' + groupHeight * (groupPageCount - 1) }, 300);
                groupPage = groupPageCount;
            } else {
                groupContentList.animate({ top: '+=' + groupHeight }, 300);
                groupPage--;
            }
        }

    });
}

function commentImgPrv() {
    //评论图片预览
    $(document).on("click", ".after-service-img img ", function () {
        $(this).addClass("active").siblings().removeClass("active");
        $(".preview-img img").attr("src", $(this).attr("src"));
        $(this).parent().siblings(".preview-img").show();
    });
    $("body").click(function () {
        $(".preview-img").hide();
        $('.after-service-img img.active').removeClass();
    });
}

function navChange() {
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
}

//处理轻松购按钮的显示
function showEasyBuyBt(isshow, isComple) {
    var isshowbt = false;
    var iscomple = false;
    try {
        iscomple = isComple || false;
    } catch (ex) {
        iscomple = false;
    }
    isshowbt = !!isshow;
    if (isshowbt) {
        var easybuybt = $("#choose-btn-easybuy");
        easybuybt.hide();   //不启用轻松购 by DZY[150713]
        if (false) {
            var curisshow = easybuybt.is(":visible");
            if (iscomple) curisshow = true;
            if (isshowbt && curisshow) {
                easybuybt.show();
            } else {
                easybuybt.hide();
            }
        }
    }
}

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
                strFuWu += '<dl class="pop-ensure new"><dt><a href="/Article/Category"><img src="/Images/SevenDay.jpg">七天退换货</a></dt></dl>';
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
            strFuWu += '<dl class="pop-ensure new" id="dlCashOnDelivery"' + codstrtyle + '><dt><img src="/Images/service3.png"> 货到付款</dt></dl>';

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
                 html += '<div id="evaluate-detail">' +
                    '<div class="mc">' +
                    '<dl >' +
                    '<dt>商品</dt>' +
                    '<dd title="（商家得分-行业平均得分）/（行业商家最高得分-行业平均得分）">' +
                    '<span class="' + productAndDescriptionColor + '">' + productAndDescription + '<i class="' + productAndDescriptionImage + '"></i></span>' +
                    '</dd>' +
                    '</dl>' +
                    '<dl>' +
                    '<dt>物流</dt>' +
                    '<dd title="（行业平均得分-商家得分）/（行业平均得分-行业商家最低得分）">' +
                    '<span class="' + sellerDeliverySpeedColor + '">' + sellerDeliverySpeed + '<i class="' + sellerDeliverySpeedImage + '"></i></span>' +
                    '</dd>' +
                    '</dl>' +
                    '<dl>' +
                    '<dt>服务</dt>' +
                    '<dd title="（行业平均得分-商家得分）/（行业平均得分-行业商家最低得分）">' +
                    '<span class="' + sellerServiceAttitudeColor + '">' + sellerServiceAttitude + '<i class="' + sellerServiceAttitudeImage + '"></i></span>' +
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
                html += strFuWu;//服务
            } else {
                var brandLogo = data.brandLogo;
                var shopinfo = '';
                if (brandLogo != "") {
                    shopinfo += '<dl id="seller"><a href="/Search/SearchAd/?b_id=' + data.brandId + '"> <img  width="65" height="35" src="' + data.brandLogo + '"  /> </a></dl><dl class="shopname text-overflow"><span><a href="/Shop/Home/' + data.id + '">' + data.name + '</a></span></dl>';
                }
                else {
                    shopinfo = '<dd><a target="_blank" style="color:#222;" href="/Shop/Home/' + data.id + '">' + data.name + '</a></dd>';
                }
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
                if (data.cashDeposits > 0 && data.cashDepositNeedPay <= 0) {
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

function loadShopCate(gid) {

    $.ajax({
        type: 'get',
        url: '../GetShopCate',
        data: { gid: gid },
        dataType: 'json',
        cache: true,// 开启ajax缓存
        success: function (data) {
            if (data) {
                //console.log(data);
                var html = '';
                for (var i = 0; i < data.length; i++) {
                    var text = '<dl><dt><a href="/Shop/SearchAd?cid=' + data[i].Id + '&sid=' + $('#shopid').val() + '&pageNo=1" target="_blank"><s></s>' + data[i].Name + '</a></dt><dd>';
                    if (data[i].SubCategory.length > 0) {
                        for (var j = 0; j < data[i].SubCategory.length; j++) {
                            text += '<a href="/Shop/SearchAd?cid=' + data[i].SubCategory[j].Id + '&sid=' + $('#shopid').val() + '&pageNo=1" target="_blank">' + data[i].SubCategory[j].Name + '</a>';
                        }
                    }
                    text += '</dd></dl>';
                    html += text;
                }
                $("#shopCateDiv").empty().append($(html));
            }
        },
        error: function (e) {
            //alert(e);
        }
    });
}


var isShow = false;

function ShowloadHotSaleProduct(gid) {
    $(window).scroll(function () {
        //到达可视区域时加载热门推荐
        var a = document.getElementById("showHotsaleDiv").offsetTop;
        if (a <= $(window).scrollTop() && a < ($(window).scrollTop() + $(window).height())) {
            if (!isShow) {
                loadHotSaleProduct(gid);
                loadHotConcernedProduct();
                isShow = true;
            }
        }
    });
}

var isShowProductCommentDiv = false;
function ShowloadProductComments() {
    $(window).scroll(function () {
        //到达可视区域时加载商品评论
        var produnctComment = document.getElementById("comment").offsetTop;
        if (produnctComment <= $(window).scrollTop() && produnctComment < ($(window).scrollTop() + $(window).height())) {
            if (!isShowProductCommentDiv) {
                GetProductComment();
                isShowProductCommentDiv = true;
            }
        }
    });
}


function loadHotSaleProduct() {
    $.ajax({
        type: 'get',
        url: '../GetHotSaleProduct',
        data: { sid: $("#shopid").val() },
        dataType: 'json',
        cache: true,// 开启ajax缓存
        success: function (data) {
            if (data) {
                //console.log(data);
                var html = '';
                for (var i = 0; i < data.length; i++) {
                    var text = '<li class="fore1">';
                    text+='<s>' + (i + 1).toString() + '</s>';
                    text += '<div class="p-img"><a href="/Product/Detail/' + data[i].Id.toString() + '" target="_blank"><img alt="' + data[i].Name + '" src="' + data[i].ImgPath + '" /></a></div>';
                    text += '<div class="p-name"><a href="/Product/Detail/' + data[i].Id.toString() + '" target="_blank" title="">' + data[i].Name + '</a></div>';
                    text += '<div class="p-info p-bfc">';
                    if (data[i].IsSaleCountOnOff)
                        text += '<div class="p-count fr"><b>热销' + data[i].SaleCount.toString() + '件</b></div>';

                    text += '<div class="p-price fl"><strong>￥' + data[i].Price.toString() + '</strong></div>';
                    text += '</div>';
                    text += '</li>';
                    html += text;
                }
                $("#hotsaleDiv").empty().append($(html));
            }
        },
        error: function (e) {
            //alert(e);
        }
    });
}


function LogProduct(gid) {
    $.ajax({
        type: 'post',
        url: '../LogProduct',
        data: { pid: gid },
        dataType: 'json',
        cache: false,// 开启ajax缓存
        success: function (data) {
            if (data) {
                //console.log(data);
            }
        },
        error: function (e) {
            //alert(e);
        }
    });
}

function loadHotConcernedProduct() {
    $.ajax({
        type: 'get',
        url: '../GetHotConcernedProduct',
        data: { sid: $("#shopid").val() },
        dataType: 'json',
        cache: true,// 开启ajax缓存
        success: function (data) {
            if (data) {
                //console.log(data);
                var html = '';
                for (var i = 0; i < data.length; i++) {
                    var text = '<li class="fore1">' +
                        '<div class="p-img"><a href="/Product/Detail/' + data[i].Id.toString() + '" target="_blank"><img alt="' + data[i].Name + '" src="' + data[i].ImgPath + '" /></a></div>' +
                        '<div class="p-name"><a href="/Product/Detail/' + data[i].Id.toString() + '" target="_blank" title="">' + data[i].Name + '</a></div>' +
                        '<div class="p-info p-bfc">' +
                        '<div class="p-count fl"><s>' + (i + 1).toString() + '</s><b>关注' + data[i].SaleCount.toString() + '次</b></div>' +
                        '<div class="p-price fr"><strong>￥' + data[i].Price.toString() + '</strong></div>' +
                        '</div>' +
                        '</li>';
                    html += text;
                }
                $("#hotConcerned").empty().append($(html));
            }
        },
        error: function (e) {
            //alert(e);
        }
    });
}


function loadGetProductDesc(gid) {
    $.ajax({
        type: 'get',
        url: '../GetProductDesc',
        data: { pid: gid },
        dataType: 'json',
        cache: false,// 开启ajax缓存
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

function loadProductNeedRefreshInfo() {
	var shopId = $("#shopid").val();
	var gid = $('#gid').val();
	$.ajax({
		type: 'post',
		url: '../GetProductDetails',
		data: {
			id: gid,
			shopId: shopId
		},
		dataType: 'json',
		cache: true, // 开启ajax缓存
		success: function(data) {
		    if (data) {
		        $("#isoverdue").val(data.isoverdue);
				$("#jd-price").text(data.price);
                $("#stockProduct").text(data.StockAll);//首加载总库存
				if(data.isSaleCountOnOff == false) {
					$("#saleCounts").parent("div").hide();
				} else {
					$("#saleCounts").text(data.saleCounts + data.measureUnit);
				}
				if(data.IsFavorite) {
					$("#choose-btn-coll a").html("已收藏商品");
				} else {
					$("#choose-btn-coll a").html("收藏商品");
				}
				if(data.hasFightGroup) {
					$("#flashSalePrice").html(data.fightFroupPrice);
					var temp = data.fightGroupUrl.split('/');
					var fightGroupId = temp[temp.length-1];
					//链接
					$("#liFlashSale span img").on("mouseenter", function() {
						var _t = $(this);
						var viewurl = data.fightGroupUrl;
						var imgurl = "/Product/ShowActiveQRCode/" + fightGroupId;
						$("#linkqrpic").attr("src", imgurl);

						$.dialog({
							title: '拼团链接',
							lock: true,
							id: 'linkshowdlg',
							content: $("#linkshowcon").html(),
							padding: '10px',
							width: '300px',
							button: []
						});
					});

				} else {
					$("#liFlashSale").hide();
				}
				//$("#addressChoose").text(data.shippingAddress);
				//$("#addressChoose").attr("data-select", data.shippingValue);
				//$("#spFreight").text(data.freight);
				//$("#productmark").append("<span class='star sa" + data.productMark + "'></span>");
				//if (data.isPreheat) {
				//    preheatHtml = "<a href=\"/LimitTimeBuy/Detail/" + data.flashSaleId + "\">" +
				//        "<div id=\"limited-tag\">" +
				//            "<span>限时购</span><span>此商品参加限时购活动，还有<i>" + data.flashSaleTime + "开始</span>" +
				//        "</div></a>";
				//    $("#preheat").append(preheatHtml);
				//}

				//var purchaseHtml = "";
				//if (data.isPreheat && !data.isNormalPurchase) {
				//    purchaseHtml += "<div id=\"choose-btn-active\" class=\"btn active-now\">" +
				//                "<a href=\"/LimitTimeBuy/Detail/" + data.flashSaleId + "\">去参加活动</a>" +
				//            "</div>";
				//} else {
				//    purchaseHtml += "<div id=\"choose-btn-buy\" class=\"btn\">" +
				//                "<a class=\"btn-append btn-order-now\" id=\"OrderNow\"><b class=\"iconfont icon-lijigoumai\"></b>立即购买</a>" +
				//    "</div>" +
				//    "<div id=\"choose-btn-append\" class=\"btn\">" +
				//        "<a class=\"btn-append\" id=\"InitCartUrl\"><b class=\"iconfont icon-jiarugouwuche\"></b>加入购物车</a>" +
				//    "</div>";
				//}
				//$("#choose-btns").append(purchaseHtml);
				loadPriceLadderPrices(data.LadderPrices, data.measureUnit); //加载阶梯价

				getProductActives($('#shopid').val(), gid);
				getHotProduct(gid, $('#categoryId').val());

				loadShipAndLimit(data.FreightTemplateId);
				loadSkus(data);

				//loadLoginUserInfo();
				//loadUserCoupons();
				if(!data.IsOpenLadderPrice) { //开启阶梯价不显示组合购
					loadProductColloCation();
				}
				loadShopCate(gid);
				loadShopInfo(gid);
				ShowloadHotSaleProduct(gid);
				ShowloadProductComments();
				loadProductAttr(gid);
				loadGetProductDesc(gid);
				loadGetCommentsNumber(gid);
				LogProduct(gid);
			}
		},
		error: function(e) {
			//alert(e);
		}
	});
}

function loadPriceLadderPrices(prices, unit) {
    var pricesHtml = '';
    var countsHtml = '';
    if (prices.length > 0) {
        $(prices).each(function (i) {
            pricesHtml += '<li><em>￥</em>' + this.Price + '</li>';//价格
            if (i == prices.length - 1)
                countsHtml += '<li><i>≥</i>' + this.MinBath + unit + '</li>';
            else {
                if (this.MinBath == this.MaxBath) {
                    countsHtml += '<li>' + this.MinBath + unit + '</li>';
                } else {
                    countsHtml += '<li>' + this.MinBath + '<i>-</i>' + this.MaxBath + unit + '</li>';
                }
            }
        });
    }
    $('#ladder-prices').html(pricesHtml);
    $('#ladder-counts').html(countsHtml);
}

function loadShipAndLimit(_templateId) {
    var shopId = $("#shopid").val();
    var gid = $('#gid').val();
    var productType = $("#productType").val();
    var isoverdue = $("#isoverdue").val();
    $.ajax({
        type: 'post',
        url: '../GetProductShipAndLimit',
        data: { id: gid, shopId: shopId, templateId: _templateId, skuId: $("#hidskuId").val() },
        dataType: 'json',
        cache: true,// 开启ajax缓存
        success: function (data) {
            if (data) {
                $("#canSelfTake").val(data.canSelfTake);//是否支持货到付款
                $("#addressChoose").text(data.shippingAddress);
                $("#addressChoose").attr("data-select", data.shippingValue);
                $("#spFreight").text(data.freight);
                $("#productmark").append("<span class='star'><i style='width:"+(data.productMark==0?5:data.productMark*15)+"px;'></i></span>");
                if (data.isPreheat) {
                    preheatHtml = "<a href=\"/LimitTimeBuy/Detail/" + data.flashSaleId + "\">" +
                        "<div id=\"limited-tag\">" +
                        "<span>限时购</span><span>此商品参加限时购活动，还有<i id='flashsecond'>" + data.flashSaleTime + "</i>开始</span>" +
                        "</div></a>";
                    $("#preheat").append(preheatHtml);
                    timer(data.flashSecond);
                }

                var purchaseHtml = "";
                if (data.isPreheat && !data.isNormalPurchase) {
                    purchaseHtml += "<div id=\"choose-btn-active\" class=\"btn active-now\">" +
                        "<a href=\"/LimitTimeBuy/Detail/" + data.flashSaleId + "\">去参加活动</a>" +
                        "</div>";
                } else {
                    if (productType == 1) {
                        if (isoverdue == 1) {
                            purchaseHtml += "<div id=\"choose-btn-buy\"  class=\"btn\">" +
                          "<a class=\"j_nobuy\" style=\"background-color:#999\">已过期</a>" +
                          "</div>";
                        } else {
                            purchaseHtml += "<div id=\"choose-btn-buy\" class=\"btn\">" +
                          "<a class=\"btn-append btn-order-now\" id=\"OrderNow\">立即购买</a>" +
                          "</div>";
                        }
                    } else {
                        purchaseHtml += "<div id=\"choose-btn-buy\" class=\"btn\">" +
                            "<a class=\"btn-append btn-order-now\" id=\"OrderNow\">立即购买</a>" +
                            "</div>" +
                            "<div id=\"choose-btn-append\" class=\"btn\"><span></span>" +
                            "<a class=\"btn-append\" id=\"InitCartUrl\"><b class=\"iconfont icon-jiarugouwuche\"></b>加入购物车</a>" +
                            "<lable id=\"MinBath\" minmath=" + data.minMath + "></lable>" +
                            "</div>";
                    }
                }

                $("#choose-btns").append(purchaseHtml);

                loadGetEnableBuyInfo(gid);

                $("#OrderNow").click(function () {
                    var has = $("#has").val();
                    var dis = $(this).parent().hasClass('disabled');
                    if (has != 1 || dis) {
                        return;
                    }
                    var skuId = $("#hidskuId").val();
                    bindToSettlement(skuId);
                });
                calcFreight(gid);
                InitCartUrl();

                InitStockView();
                ShowHideDivFuWu();
            }
        },
        error: function (e) {
            //alert(e);
        }
    });
}

//倒计时
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

        $('#flashsecond').html(day+'天'+hour + '时' + minute + '分' + second + '秒');

        intDiff--;
    }, 1000);
}

function loadProductColloCation() {
    var isPrehear = $("#isPrehear").val();
    if (isPrehear == 1) {
        return;//限时购预热则不加载组合购模块
    }
    var gid = $('#gid').val();
    $.ajax({
        type: 'post',
        url: '/product/ProductColloCation',
        data: { productId: gid },
        dataType: 'text',
        cache: true,// 开启ajax缓存
        success: function (data) {
            $("#ProductColloCation").html(data);
            //组合购
            var minCollTotal,
                maxCollTotal,
                minSaleTotal,
                maxSaleTotal;
            if ($('.p-group-list').length > 0) {
                GroupPriceChange();
            }

            $('.p-group-child input').change(function () {
                GroupPriceChange();
            });
            loadGroup();
        }
    });
}

function loadSkus(data) {
    var shopId = $("#shopid").val();
    var gid = $('#gid').val();
    var skuHtml = "";
    if (data.skuColors.length > 0) {
        skuHtml += "<li class='choose-sku'>" +
            "<div class='dt l'>" + data.colorAlias + "</div>" +
            "<div class='dd'>";
        $(data.skuColors).each(function () {
            skuHtml += " <div st='0' cid='" + this.SkuId + "' class='item itemSku " + this.EnabledClass + " " + this.SelectedClass + "'><b>◆</b>";
            if (this.Img) {
                skuHtml += "<a href='" + this.Img + "' class='cloud-zoom-gallery' rel=\"useZoom: \'zoom1\', smallImage: \'" + this.Img + "\'\" title='" + this.Value + "'>" +
                    "<img src='" + this.Img + "' />" +
                    "<i>" + this.Value + "</i>" +
                    "</a>";
            } else {
                skuHtml += "<a href='#none' title='" + this.Value + "'>" +
                    "<i>" + this.Value + "</i>" +
                    "</a>";
            }
            skuHtml += "</div>";
        });
        skuHtml += "</div></li>";
    }

    if (data.skuSizes.length > 0) {
        skuHtml += "<li class='choose-sku'>" +
            "<div class='dt l'>" + data.sizeAlias + "</div>" +
            "<div class='dd'>";
        $(data.skuSizes).each(function () {
            skuHtml += "<div st='1' cid='" + this.SkuId + "' class='item itemSku " + this.EnabledClass + " " + this.SelectedClass + "'>" +
                "<b>◆</b>" +
                "<a href='#none' title='" + this.Value + "'><i>" + this.Value + "</i></a>"
                + "</div>";
        });
        skuHtml += "</div></li>";
    }
    if (data.skuVersions.length > 0) {
        skuHtml += "<li class='choose-sku'>" +
            "<div class='dt l'>" + data.versionAlias + "</div>" +
            "<div class='dd'>";
        $(data.skuVersions).each(function () {
            skuHtml += "<div st=\"2\" cid=\"" + this.SkuId + "\" class=\"item itemSku " + this.EnabledClass + " " + this.SelectedClass + "\">" +
                "<b>◆</b>" +
                "<a href=\"#none\" title=\"" + this.Value + "\"><i>" + this.Value + "</i></a>" +
                "</div>";
        });

        skuHtml += "</div></li>";
    }

    //if (skuHtml) {
    $("#choose").prepend(skuHtml);
    $('#choose .cloud-zoom-gallery').CloudZoom();
    var productType = $("#productType").val();
    //sku操作初始化
    $('#choose').MallSku({
        data: { pId: gid },
        spec: '.choose-sku',
        resultClass: {
            price: '#jd-price',
            stock: '#stockProduct'
        },
        ajaxUrl: '/Product/GetSKUInfo',
        skuPosition: 'skuArray',
        callBack: function (select) {
            $("#hidskuId").val(select.SkuId);
            //每次sku改变则重新计算运费
            var isFree = $("#hdFreightType").val();//是否包邮
            var address = $("#addressChoose").attr("data-select");
            if (productType==0&&address != "") {
                var cityid = address.split(',')[1];//收货城市Id
                var streetId = 0;//街道Id
                if (address.split(',').length == 3) {
                    streetId = address.split(',')[2];
                } else if (address.split(',').length == 4) {
                    streetId = address.split(',')[3];
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
                    $.ajax({
                        type: 'post',
                        url: '../CalceFreight',
                        data: { cityId: cityid, streetId: streetId, pId: gid, count: totalnum, skuId: $("#hidskuId").val() },
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
            var result = '';
            $('#choose .selected a').each(function () {
                result += '<strong>“' + this.title + '“ </strong> ';
            })
            if (result.length > 0)
                $('#choose-result .dd').html('已选择：' + result);
        }
    });
    //}
}

function loadGetEnableBuyInfo(gid) {
    if ($('#IsExpiredShop').val()) {
        $('#summary-price').html('<div class="dd"><strong style="font-size:25px;color:red;">提示：该商品所在店铺已过期！</strong></div>');
        $('#choose-btn-buy').hide();
        $("#choose-btn-append").addClass("disabled");
    } else {
        $.ajax({
            type: 'get',
            url: '../GetEnableBuyInfo',
            data: { gid: gid },
            dataType: 'json',
            cache: true,// 开启ajax缓存
            async: false,//同步
            success: function (data) {
                if (data) {
                    if (data.IsOnSale === false) {
                        $('#summary-price').html('<div class="dd"><strong style="font-size:25px;color:red;">提示：该商品已下架！</strong></div>');
                        $("#stockProductImage").html('<div class="dd"><strong style="color:red;">提示：已售罄</strong></div>');
                        $("#choose-btns").hide();
                    } else {
                        //console.log(data);
                        if (!data.hasSKU) {
                            $("#stockProductImage").html("缺货");
                            $("#stockProduct").html("0");
                        }
                        else {
                            $("#choose-btn-append").removeClass("disabled");
                            $('#choose-btn-buy').show();
                            $("#choose-btns").show();
                        }
                    }
                    if (data.IsOnSale && data.Logined == 1 && data.hasQuick === 1 && data.hasSKU) {
                        showEasyBuyBt(true, true);
                    }

                }

            },
            error: function (e) {
                //alert(e);
            }
        });
    }


}

function loadGetCommentsNumber(gid) {
    $.ajax({
        type: 'get',
        url: '../GetCommentsNumber',
        data: { gid: gid },
        dataType: 'json',
        cache: true,// 开启ajax缓存
        success: function (data) {
            if (data) {
                $("#Comments").html(data.Comments.toString());
                $("#CommentsU").html("(" + data.Comments.toString() + ")");
                $("#ConsU").html("(" + data.Consultations.toString() + ")");
            }
        },
        error: function (e) {
            //alert(e);
        }
    });
}
function loadProductAttr(gid) {
    $.ajax({
        type: 'get',
        url: '../GetProductAttr',
        data: { pid: gid },
        dataType: 'json',
        cache: true,// 开启ajax缓存
        success: function (data) {
            if (data) {
                //console.log(data);
                var html = '';
                for (var i = 0; i < data.length; i++) {
                    var values = "";
                    for (var j = 0; j < data[i].AttrValues.length; j++) {
                        values += data[i].AttrValues[j].Name + ",";
                    }
                    values = values.substr(0, values.length - 1)
                    var text = '<li>' + data[i].Name + '：' + values + '</li>';

                    html += text;
                }
                //console.log(data);
                $("#detail-list").append($(html));
            }
        },
        error: function (e) {
            //alert(e);
        }
    });
}

function Search() {
    var start = isNaN(parseFloat($("#sp-price").val())) ? 0 : parseFloat($("#sp-price").val());
    var end = isNaN(parseFloat($("#sp-price1").val())) ? 0 : parseFloat($("#sp-price1").val());
    var shopid = $("#shopid").val();

    var keyword = $("#sp-keyword").val();
    if (keyword.length === 0 && start == end) {
        $.dialog.errorTips('请输入关键字或者价格区间');
        return;
    }
    if (start > end) {
        $.dialog.errorTips('起始价格不能大于结束价格');
        return;
    }
    location.href = "/Shop/SearchAd?pageNo=1&sid=" + shopid + "&keywords=" + keyword + "&startPrice=" + start + "&endPrice=" + end;
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

function changeShowView(gid) {
    var isSellerAdminProdcut = $('#IsSellerAdminProdcut').val();
    if (isSellerAdminProdcut.toLowerCase() == "true") {
        $("#sp-search,#sp-category").show();
        $("#shopname").attr("href", "#")
        bindOfficialView(gid);
    }
    else {
        $("#sp-brand-official,#sp-category-official").hide();
    }
}

function checkBuyNum() {
    var num = 0;
    var result = true;
    try {
        num = parseInt($("#buy-num").val());
    } catch (ex) {
        num = 0;
    }
    if (num < 1) {
        $.dialog.errorTips('购买数量有误');
        $('#buy-num').val(1);
        result = false;
    }
    var stockNum = parseInt($("#stockProduct").html());
    if (num > stockNum) {
        $('#buy-num').val(stockNum);
        $.dialog.errorTips('库存仅余 ' + stockNum + '件');
    }
    var minMath = parseInt($('#MinBath').attr('minmath'));
    var maxBuyCountlbl = $('#maxBuyCount');
    if (maxBuyCountlbl.length > 0 && minMath <= 0) {
        var maxBuyCount = maxBuyCountlbl.data('value');
        if (num > maxBuyCount) {
            $('#buy-num').val(maxBuyCount);
            $.dialog.errorTips('每个Id限购' + maxBuyCount + '件');
        }
    }
    return result;
}

function bindOfficialView(gid) {
    loadShopCateOfficial(gid);
    loadBrandOfficial(gid);
}

function loadBrandOfficial(gid) {

    $.ajax({
        type: 'get',
        url: '../GetBrandOfficial',
        data: { gid: gid },
        dataType: 'json',
        cache: true,// 开启ajax缓存
        success: function (data) {
            if (data) {
                $("#brandDivOfficial").empty();
                //console.log(data);
                var html = '';
                var dataLength = data.length;
                for (var i = 0; i < data.length; i++) {
                    html += '<dd style="width:33%;float:left;text-align:center;"><a href="/Search/SearchAd/?b_id=' + data[i].Id + '" target="_blank"><s></s>' + data[i].Name + '</a></dd>';
                }

                $("#brandDivOfficial").append($(html));
            }
        },
        error: function (e) {
            //alert(e);
        }
    });
}

function loadShopCateOfficial(gid) {

    $.ajax({
        type: 'get',
        url: '../GetShopCateOfficial',
        data: { gid: gid },
        dataType: 'json',
        cache: true,// 开启ajax缓存
        success: function (data) {
            if (data) {
                $("#shopCateDivOfficial").empty();
                //console.log(data);
                var html = '';
                var dataLength = data.length;
                for (var i = 0; i < data.length; i++) {
                    html += '<dd style="width:50%;float:left;text-align:center;"><a href="/Search/?cid=' + data[i].Id + '" target="_blank"><s></s>' + data[i].Name + '</a></dd>';
                }

                $("#shopCateDivOfficial").append($(html));
            }
        },
        error: function (e) {
            //alert(e);
        }
    });
}
function InitStockView() {
    if (parseInt($("#stockProduct").html()) == 0) {
        $("#stockProduct").html("已售罄");
        $('#unit').hide();
        $('#OrderNow,#InitCartUrl').parent().addClass('disabled');
    }
    else {
        $('#unit').show();
    }
}
function bindToSettlement(SkuId) {

    if (parseInt($('#buy-num').val()) > parseInt($("#stockProduct").html())) {
        //$.dialog.errorTips('不能大于库存数量');
        $('#buy-num').val(parseInt($("#stockProduct").html()));
        return false;
    }
    var minMath = parseInt($('#MinBath').attr('minmath'));
    if (minMath > 0 && parseInt($('#buy-num').val()) < minMath) {
        $.dialog.errorTips('必须满足最小批量才能购买');
        return false;
    }

    var productType = $("#productType").val();
    var productId = $('#gid').val();
    var len = $('#choose li .dd .selected').length;
    if (len === $('#choose').find(".choose-sku").length) {
        var memberId = $.cookie('Mall-User');
        var num = $("#buy-num").val();
        if (memberId) {
            $.post('/UserInfo/IsConBindSms', null, function (result) {
                if (result.success) {
                    if (checkBuyNum()) {
                        if (canBuy(productId) == false)
                            return false;

                        var url = "/Order/SubmitByProductId?skuIds=" + SkuId + "&counts=" + num + "&productType=" + productType + "&productId=" + productId;
                        location.href = url;
                    }
                }
                else {
                    $.fn.bindmobile({}, function () {
                        if (canBuy(productId) == false)
                            return false;
                        location.href = "/Order/SubmitByProductId?skuIds=" + SkuId + "&counts=" + num + "&productType=" + productType + "&productId=" + productId;
                    }, '', '', '/UserInfo/BindSms');
                }

            });
        }
        else {
            $.fn.login({}, function () {
                if (canBuy(productId) == false)
                    return false;
                location.href = "/Product/Detail/" + productId;//"/Order/SubmitByProductId?skuIds=" + SkuId + "&counts=" + num;
            }, '', '', '/Login/Login');
        }
    }
    else {
        $.dialog.errorTips("请选择商品规格！");
    }
}

function addFavorite(shopId) {
    checkLogin(function (callBack) {
        var loading = showLoading();
        $.post('/Product/AddFavorite', { shopId: shopId }, function (result) {
            loading.close();
            if (result.success)
                $.dialog.succeedTips('收藏店铺成功', function () { callBack && callBack(); });
            else
                $.dialog.tips(result.msg, function () { callBack && callBack(); });

        });
    });
}

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

function GroupPriceChange() {
    $(".productcollocation").each(function (index, item) {
        var mCheck = $(item).find('.p-group-main input:checked');
        minCollTotal = mCheck.data('mincollprice');
        maxCollTotal = mCheck.data('maxcollprice');
        minSaleTotal = mCheck.data('minsaleprice');
        maxSaleTotal = mCheck.data('maxsaleprice');

        $(item).find('.p-group-child input:checked').each(function () {
            minCollTotal += $(this).data('mincollprice');
            maxCollTotal += $(this).data('maxcollprice');
            minSaleTotal += $(this).data('minsaleprice');
            maxSaleTotal += $(this).data('maxsaleprice');
        });
        //if (minCollTotal != maxCollTotal)
        //    $(item).find('#collTotalPrice').text('¥' + minCollTotal.toFixed(2) + '-' + maxCollTotal.toFixed(2));
        //else
        $(item).find('#collTotalPrice').text('¥' + minCollTotal.toFixed(2));
        //if (minSaleTotal != maxSaleTotal)
        //    $(item).find('#saleTotalPrice').text(minSaleTotal.toFixed(2) + '-' + maxSaleTotal.toFixed(2));
        //else
        $(item).find('#saleTotalPrice').text(minSaleTotal.toFixed(2));
        if (minSaleTotal - minCollTotal != maxSaleTotal - maxCollTotal)
            $(item).find('#groupPriceMinus').text('¥' + (minSaleTotal - minCollTotal).toFixed(2) + '-' + (maxSaleTotal - maxCollTotal).toFixed(2));
        else
            $(item).find('#groupPriceMinus').text('¥' + (minSaleTotal - minCollTotal).toFixed(2));
    });
}

function CollocationBuy(i) {
    var pids = "";
    var colloPids = "";
    var chk = $(".productcollocation").eq(i).find(".collochk:checked");
    var pids = "";
    if (chk.length < 2) {
        $.dialog.errorTips("请至少选择一个商品组合购买！");
        return;
    }
    else {
        chk.each(function (index, item) {
            if (index < chk.length - 1) {
                pids += $(item).val() + ",";
                if ($(item).data("collpid") != "undefined" && $(item).data("collpid") != null)
                    colloPids += $(item).data("collpid") + ",";
            }
            else {
                pids += $(item).val();
                if ($(item).data("collpid") != "undefined" && $(item).data("collpid") != null)
                    colloPids += $(item).data("collpid");
            }
        });
    }
    $.ajax({
        url: "/Product/GetCollocationProducts",
        data: { productIds: pids, colloPids: colloPids },
        async: false,
        success: function (data) {
            $("#addCollocation").html(data);
        }
    });
    $.dialog({
        title: '组合购选择',
        width: 908,
        padding: 0,
        id: 'Collocation',
        content: document.getElementById("addCollocation"),
        lock: true,
        okVal: '确定购买套餐',
        init: function () {
            var groupPrice = 0;
            for (var i = 0; i < $('.group-item').length; i++) {
                groupPrice += parseFloat($('.product-price').eq(i).text());
            }
            $('.group-price span').data('groupprice', groupPrice).text(groupPrice.toFixed(2));

            var len = $('.group-item').length;
            for (var i = 0; i < len; i++) {
                $('.SKUgroup' + i).MallSku({
                    data: {
                        pId: $('.product-item').eq(i).data('productid'),
                        colloPid: $('.product-item').eq(i).data('colloproductid')
                    },
                    spec: '.choose-sku',
                    resultClass: {
                        price: '.product-price',
                        stock: '.group-stock'
                    },
                    ajaxUrl: '/Product/GetSKUInfo',
                    skuPosition: 'skuArray',
                    callBack: function (select, _this) {
                        $('.group-skuId', _this).val(select.SkuId);
                        var groupPrice = 0;
                        for (var i = 0; i < $('.group-item').length; i++) {
                            groupPrice += parseFloat($('.product-price').eq(i).text());
                        }
                        $('.group-price span').data('groupprice', groupPrice).text(($('#groupCounts').val() * groupPrice).toFixed(2));
                    }
                });
            }

            $('#groupCounts').keyup(function () {
                var groupPrice = $('.group-price span').data('groupprice');
                this.value = this.value.replace(/[^0-9]+/, '');
                $('.group-price span').text((this.value * groupPrice).toFixed(2));
            });

        },
        ok: function () {
            //创建订单页面
            var flag = 1,
                groupSkuids = '',
                collpids = '',
                groupcounts = '';
            var _tmp = $('#groupCounts').val();
            _tmp = parseInt(_tmp);
            if (isNaN(_tmp) || _tmp < 1) {
                $.dialog.errorTips("购买数量错误");
                return false;
            }
            $('.group-item').each(function () {
                $(this).removeClass('error');
                if ($(this).find('.selected').length != $(this).find(".choose-sku").length) {
                    $(this).addClass('error');
                    flag = 2;
                }
                if (parseInt($(this).find('.group-stock').text()) < parseInt($('#groupCounts').val())) {
                    $(this).addClass('error');
                    flag = 3;
                }
                groupSkuids += $(this).find('.group-skuId').val() + ',';
                collpids += $(this).find('.product-item').data('colloproductid') + ',';
                groupcounts += _tmp + ','
            });
            if (flag == 2) {
                $.dialog.errorTips("有未选择规格的商品");
                return false;
            }
            if (flag == 3) {
                $.dialog.errorTips("购买数大于商品库存");
                return false;
            }

            groupSkuids = groupSkuids.substring(0, groupSkuids.length - 1);
            collpids = collpids.substring(0, collpids.length - 1);
            groupcounts = groupcounts.substring(0, groupcounts.length - 1);


            $('#skuids').val(groupSkuids);
            $('#counts').val(groupcounts);
            $('#collpids').val(collpids);
            checkLogin(function () {
                //$('#CollProducts').submit();
                //检测是否开启了手机号强制绑定
                $.post('/UserInfo/IsConBindSms', null, function (result) {
                    if (result.success) {
                        $('#CollProducts').submit();
                    } else {
                        $.fn.bindmobile({}, function () { $('#CollProducts').submit(); }, '', '', '/UserInfo/BindSms');
                    }
                });
            })

        }
    });
}
function showQrCode() {
    $.dialog({
        title: '二维码',
        lock: true,
        id: 'weixinQrCode',
        content: $("#weixinQrCode").html(),
        padding: '10px',
        init: function () {
        }
    });
};

