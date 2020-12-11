$(function () {
    loadCartInfo();

    $('#toSettlement').click(function () {
        bindToSettlement();
    });

});
var data = {};
var nobuypros = [];
var hasnobuypro = false;

function loadCartInfo() {
    hasnobuypro = false;
    nobuypros = [];
    $.post('/cart/GetCartProducts', {}, function (cart) {
        //data = {};
        data = [];
        $.each(cart.products, function (i, e) {
            var hasShop = false;
            $.each(data, function (i, s) {
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
                shop['status'] = e.productstatus;
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
            //    data[e.shopId]['status'] = e.productstatus;
            //    data[e.shopId]['shop'].push(e);
            //}
        });
        var strproductstatus = $("#hidSaleStatus").val();
        var strproductauditstatus = $("#hidAuditStatus").val();
        var str = '';
        var memberId = $.cookie('Mall-User');
        if (memberId) {
            $('.cart-inner .message').find('.unLogin').hide();
        }
        if (cart.products.length == 0) {
            $('.cart-inner').addClass('cart-empty');
            $("#cartrecommend").show();
        } else {
            $.each(data, function (i, e) {
                str += '\
                  <div class="cart-toolbar cl">\
                    <span class="column t-checkbox form">\
                      <input type="checkbox" style="margin: 15px 5px 0 9px;" class="shopSelect" value="' + e.shopId + '" name="checkShop" checked="">\
                      <label for=""><a href="/shop/home/' + e.shopId + '">' + e.name + '</a></label>\
                    </span>\
                  </div>';
                //$.each(e.shop, function (j, product) {
                $.each(e.product, function (j, product) {
                    if (product.status != 0) {
                        hasnobuypro = true;
                        nobuypros.push(product.skuId);
                        str += '\
                        <div class="item item_disabled ">\
                          <div class="item_form cl">\
                            <div class="cell p-checkbox">\
                              <span status=' + product.productstatus + ' name="checkItem" class="checkbox">' + showProductStatus(product.productstatus, product.status) + '</span>\
                            </div>'
                    } else {
                        str += '\
                        <div class="item item_selected ">\
                            <div class="item_form cl">\
                            <div class="cell p-checkbox">\
                                <input class="checkbox" type="checkbox" data-cartid="'+ product.cartItemId + '" name="checkItem" checked="" value="' + product.shopId + '" sku="' + product.skuId + '" />\
                            </div>'
                    }
                    var skuStr = product.Color == "" || product.Color == null ? "" : '[' + product.ColorAlias + ':' + product.Color + ']';
                    skuStr += product.Size == "" || product.Size == null ? "" : '&nbsp;&nbsp;[' + product.SizeAlias + ':' + product.Size + ']';
                    skuStr += product.Version == "" || product.Version == null ? "" : '&nbsp;&nbsp;[' + product.VersionAlias + ':' + product.Version + ']';

                    str += '<div class="cell p-goods">\
                  <div class="p-img"><a href="/product/detail/' + product.id + '" target="_blank"><img src="' + product.imgUrl + '" alt="" /></a></div>\
                  <div class="p-name"><a href="/product/detail/' + product.id + '" target="_blank">' + product.name + '<br/>' + skuStr + '</a><br>' + (product.productstatus != 1 || product.productauditstatus == 4 ? "[已下架]" : "") + '</div>'
                    if (product.productcode) {
                        if (product.productcode.length > 0) {
                            str += '<div class="p-code">商品货号：' + product.productcode + '</div>'
                        }
                    }
                    str += '</div>\
                <div class="cell p-price"><span class="price">¥'+ product.price.toFixed(2) + '</span></div>\
                <div class="cell p-quantity">\
                  <div class="quantity-form">\
                    <a href="javascript:void(0);" class="decrement" sku="'+ product.skuId + '" >-</a>\
                    <input type="text" class="quantity-text" value="' + product.count + '" onkeyup="(this.v=function(){this.value=this.value.replace(/[^0-9-]+/,\'\');}).call(this)" onblur="this.v()" name="count" sku="' + product.skuId + '" minMath="' + product.minMath + '" />\
                    <a href="javascript:void(0);" class="increment" sku="' + product.skuId + '"  >+</a>\
                  </div>\
                </div>\
                <div class="cell p-remove"><a class="cart-remove" href="javascript:removeFromCart(\''+ product.skuId + '\')">删除</a></div>\
              </div>\
            </div>';
                });
            });
            $('#product-list').html(str);
            $(".item_disabled .quantity-text").prop("disabled", true);
            $('#totalSkuPrice').html('¥' + cart.amount.toFixed(2));
            $('#selectedCount').html(cart.totalCount);
            $('#finalPrice').html('¥' + cart.amount.toFixed(2));
            if (hasnobuypro) {
                $("#remove-nobuy-batch").show();
            } else {
                $("#remove-nobuy-batch").hide();
            }
            bindAddAndReduceBtn();
            bindBatchRemove();
            bindSelectAll();
        }
    });


}

function showProductStatus(productStatus, status) {
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

function bindToSettlement() {
    var memberId = $.cookie('Mall-User');
    var arr = [], str = '';
    $('input[name="checkItem"]').each(function (i, e) {
        if ($(e).attr('checked')) {
            arr.push($(e).attr('data-cartid'));
        }
    });

    str = (arr && arr.join(','));
    if (memberId) {
        if (str != "") {
            $.post('/UserInfo/IsConBindSms', null, function (result) {
                if (result.success) {
                    $.post('/m-wap/Cart/IsLadderCount', { cartItemIds: str }, function (result) {
                        if (result.success) {
                            location.href = '/order/submit?' + 'cartItemIds=' + str;
                        }
                        else
                            $.dialog.errorTips(result.msg);
                    });
                }
                else {
                    $.fn.bindmobile({}, function () {
                        location.href = '/order/submit?' + 'cartItemIds=' + str;
                    }, '', '', '/UserInfo/BindSms');
                }

            });
        }
        else
            $.dialog.errorTips("没有可结算的商品！");
    } else {
        $.fn.login({}, function () {
            location.href = '/cart/cart';
        }, '', '', '/Login/Login');
    }
}

function bindSelectAll() {
    $('input[name="checkAll"]').change(function () {
        var checked = $(this).attr('checked');
        checked = checked ? true : false;
        if (checked) {
            $('#product-list input[type="checkbox"]').attr('checked', checked);
            $('input[name="checkAll"]').attr('checked', checked);
            $('#product-list .item').addClass('item_selected ');
            var total = getCheckProductPrice();
            $('#finalPrice').html('¥' + total);
        }
        else {
            $('#product-list input[type="checkbox"]').removeAttr('checked');
            $('#product-list .item').removeClass('item_selected ');
            $('input[name="checkAll"]').removeAttr('checked');
            $('#finalPrice').html('¥' + "0.00");

        }
        $('#selectedCount').html(getCheckProductCount());
    });

    $('input[name="checkShop"]').change(function () {
        var checked = $(this).attr('checked'),
            v = $(this).val();
        checked = checked ? true : false;
        if (checked) {
            var total = priceAll(this, false, checked);
            var t = $('#finalPrice').html();
            var s = t.replace('¥', '');
            $('#finalPrice').html('¥' + (+parseFloat(s) + parseFloat(total)).toFixed(2));
        } else {
            var total = priceAll(this, false, checked);
            var t = $('#finalPrice').html();
            var s = t.replace('¥', '');
            $('#finalPrice').html('¥' + (+parseFloat(s) - parseFloat(total)).toFixed(2));
        }

        $('#product-list input[type="checkbox"]').each(function (i, e) {
            var a = $(e).val();
            if (a == v) {
                $(e).attr('checked', checked);
                $(e).parents('.item')[checked ? 'addClass' : 'removeClass']('item_selected');
            }
        });
        var allShopChecked = true;
        $('#product-list input[type="checkbox"]').each(function (i, e) {
            if (!$(this).attr('checked')) {
                allShopChecked = false;
            }
        });
        if (allShopChecked) {
            $('input[name="checkAll"]').attr('checked', checked);
        } else {
            $('input[name="checkAll"]').removeAttr('checked');
        }
        $('#selectedCount').html(getCheckProductCount());
        //$('input[name="checkAll"]').attr('checked', checked);
    });

    $('input[name="checkItem"]').change(function () {
        var checked = $(this).attr('checked'),
            v = $(this).val();
        checked = checked ? true : false;
        if (checked) {
            $(this).attr('checked', checked);
            $(this).parents('.item').addClass('item_selected')
        } else {
            $(this).removeAttr('checked');
            $(this).parents('.item').removeClass('item_selected')
        }

        //判断店铺下的所有商品是否全选中
        var allProductChecked = true;
        $('input[name="checkItem"]').each(function (i, e) {
            if ($(e).val() == v) {
                if (!$(e).attr('checked'))
                    allProductChecked = false;
            }
        });
        if (allProductChecked)
            $('input[name="checkShop"]').each(function () {
                if ($(this).val() == v)
                    $(this).attr('checked', checked);
            });
        else {
            $('input[name="checkShop"]').each(function () {
                if ($(this).val() == v)
                    $(this).removeAttr('checked');
            });
        }

        //判断所有店铺是否都选中了
        var allShopChecked = true;
        $('#product-list input[type="checkbox"]').each(function (i, e) {
            if (!$(this).attr('checked')) {
                allShopChecked = false;
            }
        });
        if (allShopChecked)
            $('input[name="checkAll"]').attr('checked', checked);
        else
            $('input[name="checkAll"]').removeAttr('checked');


        $('#finalPrice').html('¥' + getCheckProductPrice());
        $('#selectedCount').html(getCheckProductCount());
    });

}

function removeFromCart(skuId) {
    $.dialog.confirm('确定要从购物车移除该商品吗?', function () {
        var loading = showLoading();
        $.post('/cart/RemoveFromCart', { skuId: skuId }, function (result) {
            loading.close();
            if (result.success)
                loadCartInfo();
            else
                $.dialog.errorTips(result.msg);
        });
    })
}

function bindAddAndReduceBtn() {
    $('a.decrement').click(function () {
        var skuId = $(this).attr('sku');
        var textBox = $('input[name="count"][sku="' + skuId + '"]');
        var count = parseInt(textBox.val());
        if (count > 1) {
            count -= 1;
            updateCartItem(skuId, count, textBox);
        }
    });

    $('a.increment').click(function () {
        var skuId = $(this).attr('sku');
        var textBox = $('input[name="count"][sku="' + skuId + '"]');
        var count = parseInt(textBox.val());

        if (count > 0) {
            count += 1;
            updateCartItem(skuId, count, textBox);
        }
    });

    $('input[name="count"]').keyup(function () {
        var skuId = $(this).attr('sku');
        var count = parseInt($(this).val());
        if (count > 0) {
            updateCartItem(skuId, count);
        }
        else {
            $(this).val('1');
            updateCartItem(skuId, 1, $(this));
            $('#finalPrice').html(getCheckProductPrice());
        }
    });
}

function updateCartItem(skuId, count, obj) {
    var loading = showLoading();
    $.post('/cart/UpdateCartItem', { skuId: skuId, count: count }, function (result) {
        loading.close();
        if (result.success) {
            $(obj).val(count);
            $.each($('input[name="count"]'), function (i, inputText) {
                if ($(this).attr('sku').indexOf(result.productid) > -1) {
                    if (result.isOpenLadder)
                        $(this).parent().parent().parent().find('.price').html("¥" + result.saleprice);

                    if ($(this).parent().parent().parent().find('input[name="checkItem"]').is(":checked")) {
                        $('#finalPrice').html(getCheckProductPrice());
                        $('#selectedCount').html(getCheckProductCount());
                    }
                }
            });
        }
        else {
            var textBox = $('input[name="count"][sku="' + skuId + '"]');
            updateCartItem(skuId, result.stock, textBox);
            $.dialog.errorTips(result.msg);
        }
    });
}

function bindBatchRemove() {
    $('#remove-batch').click(function () {
        var skuIds = [];
        $.each($('#product-list input[type="checkbox"]:checked'), function (i, checkBox) {
            skuIds.push($(checkBox).attr('sku'));
        });
        if (skuIds.length < 1) {
            $.dialog.errorTips("请选择要删除的商品！");
            return;
        }

        $.dialog.confirm('确定要从购物车移除选中的商品吗?', function () {
            var loading = showLoading();
            $.post('/cart/BatchRemoveFromCart', { skuIds: skuIds.toString() }, function (result) {
                loading.close();
                if (result.success)
                    loadCartInfo();
                else
                    $.dialog.errorTips(result.msg);
            });
        })
    });

    $('#remove-nobuy-batch').click(function () {
        var _t = $(this);
        if (nobuypros.length < 1) {
            _t.hide();
            return;
        }

        $.dialog.confirm('确认要清空该店铺下的失效商品吗？', function () {
            var loading = showLoading();
            $.post('/cart/BatchRemoveFromCart', { skuIds: nobuypros.toString() }, function (result) {
                loading.close();
                if (result.success)
                    loadCartInfo();
                else
                    $.dialog.errorTips(result.msg);
            });
        })
    });
}
function priceAll(tag, bool, checked) {

    var t = 0;
    if (bool) {
        $(tag).parent().parent().parent().find('.item_form').each(function (i, e) {
            var a = $(this).find('.price').html(),
                b = a.replace('¥', ''),
                c = $(this).find('input[name="count"]').val(),
                d = (+b) * (+c);
            t += d;
        });
        return t.toFixed(2);
    }
    if (typeof tag == 'string') {
        $(tag).each(function (i, e) {
            if ($(this).find(".checkbox").attr("status") == $("#hidSaleStatus").val() && $(this).find(".checkbox").attr("status") != $("#hidAuditStatus").val()) {
                var a = $(this).find('.price').html(),
                b = a.replace('¥', ''),
                c = $(this).find('input[name="count"]').val(),
                d = (+b) * (+c);
                t += d;
            }
        });
    } else {
        if (checked) {
            $(tag).parent().parent().parent().find('input[name="checkItem"]').not("input:checked").each(function (i, e) {
                if ($(tag).val() == $(e).val()) {
                    var a = $(this).parent().parent().find('.price').eq(0).html(),
                        b = a.replace('¥', ''),
                        c = $(this).parent().parent().find('input[name="count"]').val(),
                        d = (+b) * (+c);
                    t += d;
                }
            });
        }
        else {
            $(tag).parent().parent().parent().find('input[name="checkItem"]:checked').each(function (i, e) {
                if ($(tag).val() == $(e).val()) {
                    var a = $(this).parent().parent().find('.price').eq(0).html(),
                        b = a.replace('¥', ''),
                        c = $(this).parent().parent().find('input[name="count"]').val(),
                        d = (+b) * (+c);
                    t += d;
                }
            });
        }
        return t.toFixed(2);
    }
    return t.toFixed(2);
}

function getCheckProductPrice() {
    var t = 0;
    $.each($('input[name="checkItem"]:checked'), function () {
        var a = $(this).parent().parent().find('.price').html(),
            b = a.replace('¥', ''),
            c = $(this).parent().parent().find('input[name="count"]').val(),
            d = (+b) * (+c);
        t += d;
    })
    return t.toFixed(2);
}

function getCheckProductCount() {
    var t = 0;
    $.each($('input[name="checkItem"]:checked'), function () {
        var c = $(this).parent().parent().find('input[name="count"]').val();
        d = parseInt(c);
        t += d;
    })
    return t;
}

