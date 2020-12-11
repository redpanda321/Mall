$(function () {
    loadCartInfo();

    $('#toSettlement').click(function () {
        bindToSettlement();
    });
});
var data = {};
function loadCartInfo() {
    $.post('/m-wap/Cart/GetCartProducts', {}, function (result) {
        data = [];
        var cart = result.data;
        $.each(cart.products, function (i, e) {
            var hasShop = false;
            $.each(data,function(i,s){
                if (s['shopId'] == e.shopId) {
                    if (s['name'] != e.shopName)
                        s['name'] = e.shopName;
                    s['product'].push(e);
                    hasShop = true;
                }
            });
            if (!hasShop) {
                var shop = {};
                shop['shopId'] = e.shopId;
                shop['name'] = e.shopName;
                shop['product'] = [];
                shop['product'].push(e);
                data.push(shop);
            }

            //if (data[e.shopId]) {
            //    if (!data[e.shopId]['name']) {
            //        data[e.shopId]['name'] = e.shopName;
            //    }
            //    data[e.shopId]['shop'].push(e);
            //} else {
            //    data[e.shopId] = {};
            //    data[e.shopId]['shop'] = [];
            //    data[e.shopId]['name'] = e.shopName;
            //    data[e.shopId]['shop'].push(e);
            //}
        });

        var str = '';
        //var memberId = $.cookie('Mall-User');
        //if (memberId) {
        //    $('.cart-inner .message').find('.unLogin').hide();
        //}
        if (cart.products.length == 0) {
            $(".top-nav,.footer-cart,.list-group.cart").hide();
            if (cart.shopBranchCart.length == 0) {
                $('.cart-inner').html('<div class="empty-show"><h4>购物车空空如也</h4><p>去挑几件中意的商品吧</p></div>').addClass('cart-empty');
            }
        } else {
            $.each(data, function (i, e) {
                //if (e.shop[0].vshopId != 0) {
                if (e.product[0].vshopId != 0) {
                    str += '<li class="list-group-item"><div class="cart-shop"><i class="check-custom"></i><span><a href="/' + areaName + '/vshop/detail/' + e.product[0].vshopId + '">'
                    if (e.product[0].shopLogo == null) {
                        str += '<img src="~/Areas/Web/Images/40x24.png" />';
                    }
                    else
                        str += e.name + '</a></span></div>';
                }
                else {
                    str += '<li class="list-group-item"><div class="cart-shop"><i class=" check-custom"></i><span>';
                    if (e.product[0].shopLogo == null) {
                        str += '<img src="~/Areas/Web/Images/40x24.png" />'
                    }
                    else
                        str += e.name + '<span></div>';
                }
                //在controller中修改了图片
                //$.each(e.shop, function (j, product) {
                $.each(e.product, function (j, product) {
                    if (product.status == 0) {
                        str += '<div class="cart-goods">'+
                            '<i class="check-custom"></i>'+
                            '<a><img src="'+ product.imgUrl + '" /></a>'+
                                '<p><a href="/'+ areaName + '/product/detail/' + product.id + '">' + product.name + '</a></p><h5>' + product.skuDetails + '</h5><span id="price">¥' + product.price.toFixed(2) + '</span>'+
                            '<div class="wrap-num">'+
                            '<div class="chenghao">x</div>'+
                            '<a class="glyphicon glyphicon-minus" sku="' + product.skuId + '" href="javascript:;"></a>'+
                                '<input name="count" type="text" onkeyup="(this.v=function(){this.value=this.value.replace(/[^0-9-]+/,\'\');}).call(this)" onblur="this.v()" data-cartid="' + product.cartItemId + '" value="' + product.count + '" name="count" sku="' + product.skuId + '" data-productid="'+ product.id +'" data-math="'+ product.minMath +'" />'+
                            '<a class="glyphicon glyphicon-plus" sku="' + product.skuId + '" href="javascript:;"></a>'+
                            '</div>'+
                            '<a class="cart-remove" href="javascript:removeFromCart(\'' + product.skuId + '\')"><i class="glyphicon glyphicon-trash"></i></a>'+
                            '</div>';
                    }
                    else {
                        str += '<div class="cart-goods cart-disable" data-skuid="'+ product.skuId + '">'+
                            '<a><img src="'+ product.imgUrl + '" /></a>'+
                                '<p><a href="/'+ areaName + '/product/detail/' + product.id + '">' + product.name + '</a></p><h5>' + product.skuDetails + '</h5><span id="price">¥' + product.price.toFixed(2) + '</span>'+
                            '<div class="wrap-num">'+
                            '<div class="chenghao">x' + product.count + '</div>'+
                            '<div class="chenghao" style="margin-left:30px">' + showProductStatus(product.status) + '</div>'+
                            '</div>'+
                            '<a class="cart-remove" href="javascript:removeFromCart(\''+ product.skuId + '\')"><i class="glyphicon glyphicon-trash"></i></a>'+
                            '</div>';
                    }
                });
            });
        }
        $('.list-group.cart').html(str);
        $('.list-group.cart .list-group-item').each(function(){
        	var _this=$(this);
        	if(_this.find('.cart-disable').length>0){
        		_this.find('.cart-shop').append('<em class="delDisabled fr mr10" style="display:none">清空失效商品</em>');
        	}
        });
        $('#totalSkuPrice').html('¥' + 0);
        $('#selectedCount').html('(' + 0 + ')');
        
        bindSelectAll();
        bindAddAndReduceBtn();

        bindBatchRemove();
        ShowCheckState();
        
        if(!isClickNum){
        	isClickNum=true;
        	$(".editor").click();
        }
        
        $('.list-group.cart').on('click','.delDisabled',function(){
        	var skuIds=[];
    		$(this).parent().siblings('.cart-disable').each(function(){
    			skuIds.push($(this).data('skuid'));
    		});
        	$.dialog.confirm("您确定清空失效商品吗?", function () {
                $.post('/m-wap/Cart/BatchRemoveFromCart', { skuIds: skuIds.join(',') }, function (result) {
			        if (result.success)
			            loadCartInfo();
			        else
			            $.dialog.errorTips(result.msg);
			    });
            });
        });

        //门店购物车
        var sHtml = "";
        $.each(cart.shopBranchCart, function (i, e) {//门店
            if (!e || !e[0]) {
                return;
            }
            sHtml += '<li class="list-group-item"><a href="/' + areaName + '/ShopBranch/Index/' + e[0].shopBranchId + '"><div class="cart-shop">' + e[0].shopBranchName + '<span class="glyphicon glyphicon-menu-right"></span></div></a><div class="cartstore_c"><ul>';
            var sumnum = 0;
            for (var _i = 0; _i < e.length; _i++) {
                var item = e[_i];
                if (item.status == 0 && item.count) {
                    sumnum += item.count;
                }
            }
            $.each(e, function (j, item) {//商品
                if (j < 4) {
                    sHtml += "<li><div class=\"pic\"><a href='/" + areaName + "/ShopBranch/Index/" + e[0].shopBranchId + "'><img style='height:56px;' src=\"" + item.imgUrl + "\"/></a>" + (item.status>0?"<span class='invalid'>"+showProductStatus(item.status)+"</span>":"")  + "</div><div class=\"price\">￥" + item.price + "</div></li>";
                } else if (j == 4) {
                    sHtml += "<li><div class=\"more\">●●●</div></li>";
                    sHtml += "<li><div class=\"fonts\">共" + sumnum + "件</div></li>";
                }
            });
            sHtml += "</ul><div class='clearCarts' bid='" + e[0].shopBranchId + " '><i class='glyphicon glyphicon-trash'></i></li>";
            sHtml += "</div></li>";

            $('.list-group.cartstore').html(sHtml);
        });
        $('.list-group.cartstore .clearCarts').each(function () {
            $(this).click(function () {
                var clearCarts = $(this);
                $.dialog.confirm("您确定删除门店购物车吗?", function () {
                    var bid = clearCarts.attr("bid");
                    if (!bid || bid <= 0) return;
                    $.post('/' + areaName + '/cart/ClearBranchCartProducts', {
                        shopBranchId: bid
                    }, function (result) {
                        if (result.success) {
                            clearCarts.parents(".list-group-item").remove();
                        }
                    })
                });
            });
        })
    });


}

function showProductStatus(status) {
    var result = "";
    switch (status) {
        case 1:
            result = '已失效';
            break;
        case 2:
            result = '已售罄';
            break;
        case 3:
            result = '已下架';
            break;
    }
    return result;
}

function removeFromCart(skuId) {
	$.dialog.confirm("您确定删除该商品吗?", function () {
        $.post('/m-wap/Cart/RemoveFromCart', { skuId: skuId }, function (result) {
	        if (result.success)
	            loadCartInfo();
	        else
	            $.dialog.errorTips(result.msg);
	    });
    });
    
}
function bindSelectAll() {
    $('#checkAll').unbind('click').click(function () {
        var checked = $(this).hasClass('active');
        if (checked) {
            $('.check-custom').each(function (i, e) {
                if ($(this).hasClass('active')) {
                    $(this).removeClass('active');
                }
            });

        } else {
            $('.check-custom').each(function (i, e) {
                if (!$(this).hasClass('active')) {
                    $(this).addClass('active');
                }
            });
        }
        ComputeShowCartPrice();
    });

    $('.cart-shop .check-custom').unbind('click').click(function () {
        var checked = $(this).hasClass('active');
        if (checked) {
            $(this).parent().siblings().find('.check-custom').each(function () {
                if ($(this).hasClass('active')) {
                    $(this).removeClass('active');
                }
            });
            $(this).removeClass('active');
        } else {
            $(this).parent().siblings().find('.check-custom').each(function () {
                if (!$(this).hasClass('active')) {
                    $(this).addClass('active');
                }
            });
            $(this).addClass('active');
        }
        ShowCheckState();
        ComputeShowCartPrice();
    });

    $('.cart-goods .check-custom').unbind('click').click(function () {
        var checked = $(this).hasClass('active');
        if (checked) {
            $(this).removeClass('active');
            if ($(this).parent().siblings().not(".cart-goods").find('.check-custom').hasClass('active')) {
                $(this).parent().siblings().not(".cart-goods").find('.check-custom').removeClass('active');
            }
        } else {
            $(this).addClass('active');
            var allGoodsCheck = true;
            $(this).parent().parent().find(".cart-goods .check-custom").each(function () {
                if (!$(this).hasClass('active')) {
                    allGoodsCheck = false;
                }
            });
            if (allGoodsCheck) {
                $(this).parent().siblings().not(".cart-goods").find('.check-custom').addClass('active');

            }
        }
        ShowCheckState();
        ComputeShowCartPrice();
    });
}


function bindAddAndReduceBtn() {
    $('a.glyphicon-minus').click(function () {
        var skuId = $(this).attr('sku');
        var textBox = $(this).parent().find('input[name="count"]');
        var count = parseInt(textBox.val());
        if (count > 1) {
            count -= 1;
            updateCartItem(skuId, count, textBox);
        }
    });

    $('a.glyphicon-plus').click(function () {
        var skuId = $(this).attr('sku');
        var textBox = $(this).parent().find('input[name="count"]');
        var count = parseInt(textBox.val());

        if (count > 0) {
            count += 1;
            updateCartItem(skuId, count, textBox);
        }
    });

    $('input[name="count"]').change(function () {
        var skuId = $(this).attr('sku');
        var count = parseInt($(this).val());
        var textBox = $(this).parent().find('input[name="count"]');
        if (count > 0) {
            updateCartItem(skuId, count, textBox);
        } else
            $(this).val('1');

    });
}

function updateCartItem(skuId, count,obj) {
    $.post('/' + areaName + '/cart/UpdateCartItem', { skuId: skuId, count: count }, function (result) {
        if (result.success) {
            $(obj).val(count);
            $.each($('input[name="count"]'), function (i, inputText) {
                if ($(this).attr('sku').indexOf(result.data.productid) > -1) {
                    if (result.data.isOpenLadder)
                        $(this).parent().parent().find('#price').html("¥" + result.data.saleprice);
                    if ($(this).parent().parent().find('.check-custom').hasClass('active')) {
                        var totalPrice = getCheckProductPrice().toFixed(2);
                        var count = getCheckProductCount();
                        $('#totalSkuPrice').html('¥' + totalPrice);
                        $('#selectedCount').html('(' + count + ')');
                    }
                }
            });
        }
        else {
            $.dialog.errorTips(result.msg);
            var textBox = $('input[name="count"][sku="' + skuId + '"]');
            updateCartItem(skuId, result.stock, textBox);
        }
    });
}

function bindBatchRemove() {
    $('#deleteProducts').click(function () {
        if ($('.cart-goods .check-custom.active').length == 0) {
            $.dialog.errorTips('请选择删除的商品！');
            return;
        }
        $.dialog.confirm('确定要删除这些商品吗？', function () {
            var skuIds = [];
            $('.cart-goods .check-custom').each(function () {
                if ($(this).hasClass('active')) {
                    skuIds.push($(this).parent().find('input[name="count"]').attr('sku'));
                }
            });

            $.post('/m-wap/cart/BatchRemoveFromCart', { skuIds: skuIds.toString() }, function (result) {
                if (result.success)
                    $.dialog.succeedTips("删除成功", function () {
                        window.location.reload();
                    })
                else
                    alert(result.msg);
            });
        });
    });
}

//计算并显示购物车价格
//By DZY[150707]
function ComputeShowCartPrice() {
    var PriceTotal = 0;
    $(".list-group").find('.cart-goods .check-custom').each(function () {
        var _t = $(this);
        var _p = $(this).parent();
        if (_t.hasClass('active')) {
            var a = _p.find('#price').html(),
                b = a.replace('¥', ''),
                c = _p.find('input[name="count"]').val(),
                d = (+b) * (+c);
            PriceTotal += d;
        }
    });
    PriceTotal = parseInt(PriceTotal * 100) / 100;
    $('#totalSkuPrice').html('¥' + PriceTotal);
    $('#selectedCount').html('(' + getCheckProductCount() + ')');
}

//显示全选状态
function ShowCheckState() {
    var allShopCheck = true;
    $('.cart-shop .check-custom').each(function () {
        if (!$(this).hasClass('active')) {
            allShopCheck = false;
        }
    });
    if (allShopCheck) {
        $('#checkAll').addClass('active');
    } else {
        $('#checkAll').removeClass('active');
    }
}

function getCheckProductPrice() {
    var t = 0;
    $('.cart-goods .check-custom').each(function () {
        if ($(this).hasClass('active')) {
            var a = $(this).parent().find('#price').html(),
                b = a.replace('¥', ''),
                c = $(this).parent().find('input[name="count"]').val(),
                d = (+b) * (+c);
            t += d;
        }
    });
    return t;
}

function getCheckProductCount() {
    var t = 0;
    $('.cart-goods .check-custom').each(function () {
        if ($(this).hasClass('active')) {
            var c = $(this).parent().find('input[name="count"]').val();
            d = parseInt(c);
            t += d;
        }
    });
    return t;
}

function bindToSettlement() {
    var memberId = $.cookie('Mall-User');
    var arr = [], str = '';
    $('.cart-goods .check-custom').each(function (i, e) {
        if ($(e).hasClass('active')) {
            arr.push($(e).parent().find('input[name="count"]').attr('data-cartid'));
        }
    });
    str = (arr && arr.join(','));
    if (str != '') {
        if (memberId) {
            $.post('/m-wap/Cart/IsLadderCount', { cartItemIds: str }, function (result) {
                if (result.success) {
                    var returnUrl = "/" + areaName + "/cart/cart";
                    returnUrl = encodeURIComponent(returnUrl);
                    //检测是否开启了手机号强制绑定
                    $.post("/" + areaName + "/Member/IsConBindSms", null, function (result) {
                        if (result.success) {
                            var url = '/common/site/pay?area=mobile&platform=' + areaName.replace('m-', '') + '&controller=order&action=SubmiteByCart&cartItemIds={0}&returnUrl={1}'.format(str, returnUrl);
                            //location.href = '/' + areaName + '/Order/SubmiteByCart?' + 'cartItemIds=' + str + '&returnUrl=' + returnUrl;
                            location.href = url;
                        } else {//说明开启了强制绑定手机号，而用户没有绑定，则需要先去绑定
                            location.href = "/" + areaName + "/Member/BindPhone?returnUrl=" + returnUrl + "&type=1";
                        }
                    });
                }
                else
                    $.dialog.errorTips(result.msg);
            });
            
        }
        else {
            $.fn.login({}, function () {
                var url = '/common/site/pay?area=mobile&platform=' + areaName.replace('m-', '') + '&controller=order&action=SubmiteByCart';
                //location.href = '/' + areaName + '/Order/SubmiteByCart';
                location.href = url;
            }, '', '', '/' + areaName + '/Login/Login');
        }
    }
    else {
        $.dialog.errorTips('请选择结算的商品！');
    }
}
