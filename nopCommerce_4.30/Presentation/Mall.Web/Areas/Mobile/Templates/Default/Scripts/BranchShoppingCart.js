//门店购物车
var sbottom = $(".sbottom-btn");//底部
var modulpopup = $(".modul-popup.store");//弹窗
$(function () {
    $(modulpopup).on('click', function () {
        $(modulpopup).removeClass("is-visible");
        return;
    })
    $(".modul-popup-container").click(function () {
        return false;
    })
    //默认隐藏购物车弹窗
    if ($("#opencart").length == 0 || $("#opencart").val() != "1")//获得到opencart参数为1时不隐藏
        modulpopup.removeClass("is-visible");
    else {
        if (!modulpopup.hasClass("is-visible")) {
            modulpopup.addClass("is-visible");
        }
    }
    //门店ID
    var bid = $("#bid").val();

    GetBranchCartProducts(bid);
    //打开商品列表
    $(sbottom).find(".s-cart").click(function () {
        //if ($(this).hasClass("disabled")) return;
        if (modulpopup.hasClass("is-visible"))
            modulpopup.removeClass("is-visible");
        else
            modulpopup.addClass("is-visible");
    });
    //全选
    $(modulpopup).find(".settle-popup-header .check-custom").addClass("active");
    $(modulpopup).find(".settle-popup-header .check-custom").click(function () {
        if ($(this).hasClass("active")) {
            $(this).removeClass("active");
            $(modulpopup).find(".settle-popup-body .check-custom").each(function () {
                if ($(this).hasClass("active")) {
                    $(this).removeClass("active");
                }
            })
        } else {
            $(this).addClass("active");
            $(modulpopup).find(".settle-popup-body .check-custom").each(function () {
                if (!$(this).hasClass("active")) {
                    $(this).addClass("active");
                }
            })
        }
        SetBottomData(true);
    })
    //结算
    $(sbottom).find("button").click(function () {
        var gid = $("#gid").val();
        var bid = $("#bid").val();

        var returnHref = "/" + areaName + "/BranchProduct/Detail/" + gid + "?shopBranchId=" + bid;
        checkLogin(returnHref, function () {
            if (!$(this).hasClass("disabled")) {
                var itemId = "";
                if ($(modulpopup).find(".settle-popup-body .active").length == 0) {
                    $.dialog.errorTips("请选择结算商品！");
                    return false
                }
                $(modulpopup).find(".settle-popup-body .active").each(function () {
                    itemId += $(this).attr("itemid") + ",";
                })
                var url = '/common/site/pay?area=mobile&platform=' + areaName.replace('m-', '') + '&controller=BranchOrder&action=SubmiteByCart&cartItemIds={0}'.format(itemId.substr(0, itemId.length - 1));
                //document.location.href = url;
                //document.location.href = "/" + areaName + "/BranchOrder/SubmiteByCart?cartItemIds=" + itemId.substr(0, itemId.length - 1);
                //检测是否开启了手机号强制绑定
                $.post("/" + areaName + "/Member/IsConBindSms", null, function (result) {
                    if (result.success) {
                        location.href = url;
                    } else {//说明开启了强制绑定手机号，而用户没有绑定，则需要先去绑定
                        location.href = "/" + areaName + "/Member/BindPhone?returnUrl=" + encodeURIComponent(url) + "&type=1";
                    }
                });
            }
        }, shopId);
    })
})

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
//获取购物车
function GetBranchCartProducts(bid) {
    if (!bid || bid <= 0) return;
    $.post('/' + areaName + '/cart/GetBranchCartProducts', {
        shopBranchId: bid
    }, function (result) {
        LoadBottomHtml(result.data);
    })
}
//初始化底部
function LoadBottomHtml(data) {
	var check = new Array()
	$(modulpopup).find(".settle-popup-body .check-custom").each(function(){
		if(!$(this).hasClass('active')){
			var itemid = $(this).attr('itemid')
			check.push(itemid)
		}
	})
    var products = data.products;//购物车列表
    var amount = parseFloat(data.amount);//总金额
    var totalCount = parseInt(data.totalCount);//总数量
    var DeliveFee = parseFloat(data.DeliveFee);//配送费
    var FreeMailFee = parseFloat(data.FreeMailFee);//配送费
    var DeliveTotalFee = parseFloat(data.DeliveTotalFee);//起送费
    var shopBranchStatus = data.shopBranchStatus;//门店状态
    if (shopBranchStatus != "0") {
        $(sbottom).find(".s-cart").addClass("disabled");//门店冻结，无法选择购物车

        $(sbottom).find("button").addClass("disabled");
    }
    //商品总数量
    if (totalCount > 0) {
        $(sbottom).find(".s-cart").removeClass("disabled");
        $(sbottom).find(".s-cart i").show();
        $(sbottom).find(".s-cart i").html(totalCount);
    } else {
        $(sbottom).find(".s-cart").addClass("disabled");
        $(sbottom).find(".s-cart i").hide();
    }
    //金额
    $(sbottom).find(".info span span").html(amount.toFixed(2));
    //配送费
    if (DeliveFee > 0) {
        if (data.IsFreeMail) {
            if (FreeMailFee > 0)
                $(sbottom).find(".info p").html("配送费" + DeliveFee + "元,满" + FreeMailFee + "元包邮");
            else {//满0元包邮，即为免配送费
                //$(sbottom).find(".info p").html("配送费" + DeliveFee + "元");
                $(sbottom).find(".info p").html("免配送费");
            }
        } else {
            $(sbottom).find(".info p").html("配送费" + DeliveFee + "元");
        }
    } else {
        $(sbottom).find(".info p").html("免配送费");
    }
    //结算
    $(sbottom).find("button").attr("delivetotalfee", DeliveTotalFee);
    if (DeliveTotalFee > amount) {
        $(sbottom).find("button").addClass("disabled");
        $(sbottom).find("button").html("差¥" + (DeliveTotalFee - amount).toFixed(2) + "起送");
        //将按钮置失效
        $(sbottom).find("button").attr("disabled", true);
    } else {
        $(sbottom).find("button").removeClass("disabled");
        $(sbottom).find("button").html("结算");
        $(sbottom).find("button").attr("disabled", false);
    }
    if (amount == 0) {
        $(sbottom).find("button").addClass("disabled");
        $(sbottom).find("button").html("¥" + (DeliveTotalFee - amount).toFixed(2) + "起送");
    }
    $(modulpopup).find("#product1").html("");
    $(modulpopup).find("#product2").html("");
    var hasPorduct2 = false;
    $('button.store-btnspan').find('i').text('');
    $.each(products, function () {
        var sHtml = "";
        var productCountBtn = $('button.store-btnspan[name=' + this.id + ']').find('i');
        productCountBtn.text(productCountBtn.text() == '' ? this.count : parseInt(productCountBtn.text()) + this.count);
        //普通商品
        if (this.status == 0) {
        		 sHtml = "<li class=\"clearfix\">" +
                " <i class=\"check-custom active\" bId=\"" + this.bId + "\" itemid=\"" + this.cartItemId + "\" skuId=\"" + this.skuId + "\"></i>" +
                " <span class=\"p-name\" pid=\"" + this.id + "\">" + this.name + "<p>" + this.skuDetails + "</p></span>" +
                " <div class=\"fr\">" +
                " <span class=\"money\"><sub>¥</sub> <em>" + this.price + "</em></span>" +
                " <div class=\"store-btn-buy clearfix\">" +
                " <span class=\"store-minus\"></span>" +
                " <input class=\"buynum\" value=\"" + this.count + "\" readonly stock=\"" + this.stock + "\" />" +
                " <span class=\"store-add\"></span>" +
                "</div></div></li>";
            $(modulpopup).find("#product1").append(sHtml);
        } else { //失效商品
            hasPorduct2 = true;
            sHtml = "<li class=\"clearfix\">" +
                "<span class=\"p-name\">" + this.name + " " + this.skuDetails + "</span>" +
                "<div class=\"fr\">" +
                "    <span class=\"money\"><sub>¥</sub>" + this.price + "</span>" +
                "    <span class=\"disable-reason\">" + showProductStatus(this.status) + "</span>" +
                "</div></li>";
            $(modulpopup).find("#product2").append(sHtml);
        }
    });
    if (!hasPorduct2) {
        $(modulpopup).find(".disabled-pros").hide();
    } else {
        $(modulpopup).find(".disabled-pros").show();
    }
    var allAction = true;
    
    //勾选
    $(modulpopup).find(".settle-popup-body .check-custom").each(function () {
        $(this).click(function () {
            if ($(this).hasClass("active")) {
                $(this).removeClass("active");
                $(modulpopup).find(".settle-popup-header .check-custom").removeClass("active");
            } else {
                $(this).addClass("active");
                if ($(modulpopup).find(".settle-popup-body .active").length == $(modulpopup).find(".settle-popup-body .check-custom").length) {
                    $(modulpopup).find(".settle-popup-header .check-custom").addClass("active");
                }
            }
            SetBottomData(true);
        })
    })
    //减少
    $(modulpopup).find(".settle-popup-body .store-minus").click(function () {
        var buynum = $(this).parents(".store-btn-buy").find(".buynum");
        if (parseInt(buynum.val()) > 0) {
            $(buynum).val(parseInt(buynum.val()) - 1);
            if (checkBuyNum_Cart(buynum)) {
                editcart_Cart(buynum);
            }
        }
    })
    //增加
    $(modulpopup).find(".settle-popup-body .store-add").click(function () {

        var buynum = $(this).parents(".store-btn-buy").find(".buynum");
        $(buynum).val(parseInt(buynum.val()) + 1);
        if (checkBuyNum_Cart(buynum)) {
            editcart_Cart(buynum);
        }
    })
    //清空购物车
    $(modulpopup).find(".settle-popup-header .c-dele").click(function () {
        $.dialog.confirm("您确定清空购物车吗?", function () {
            var bid = $("#bid").val();
            if (!bid || bid <= 0) return;
            $.post('/' + areaName + '/cart/ClearBranchCartProducts', {
                shopBranchId: bid
            }, function (result) {
                if (result.success) {
                    Refresh();
                    $(modulpopup).find(".settle-popup-body #product1").html("");
                    $(modulpopup).find(".settle-popup-header #product2").html("");
                    SetBottomData();
                    //调取页面购物车控制
                    if (editPageBuyNum) {
                        editPageBuyNum("buynum", 0);
                    }
                }
            })
        });
    })
    //清空失效商品
    $(modulpopup).find(".settle-popup-body .c-dele").click(function () {
        $.dialog.confirm("您确定清空失效商品吗?", function () {
            var bid = $("#bid").val();
            if (!bid || bid <= 0) return;
            $.post('/' + areaName + '/cart/ClearBranchCartInvalidProducts', {
                shopBranchId: bid
            }, function (result) {
                if (result.success) {
                    Refresh();
                    $(modulpopup).find(".disabled-pros").hide();
                    $(modulpopup).find(".settle-popup-header #product2").remove();
                    SetBottomData();
                }
            })
        });
    })
    
     if(check.length>0){
    	for(var i=0; i< check.length; i++){
    		$(modulpopup).find(".settle-popup-body .check-custom").each(function(){
    			var itemid = $(this).attr('itemid')	
    			if(itemid == check[i]){
    				$(this).trigger('click')
    			}
    		})
    		
    	}
    }
    
}

function SetBottomData(isChecked) {
    var amount = 0;//总金额
    var totalCount = 0;//总数量
    var DeliveTotalFee = $(sbottom).find("button").attr("delivetotalfee");//起送价

    $(modulpopup).find(".settle-popup-body .active").each(function () {
        var price = parseFloat($(this).parents("li").find(".money em").html());
        var num = parseFloat($(this).parents("li").find(".store-btn-buy .buynum").val());
        amount += price * num;
        totalCount += num;
    })

    if (!isChecked) {
        //商品总数量
        if (totalCount > 0) {
            $(sbottom).find(".s-cart i").show();
            $(sbottom).find(".s-cart i").html(totalCount);
        } else {
            $(sbottom).find(".s-cart i").hide();
        }
    }
    
    //金额
    $(sbottom).find(".info span span").html(amount.toFixed(2));
    //结算
    if (DeliveTotalFee > amount) {
        $(sbottom).find("button").addClass("disabled");
        $(sbottom).find("button").html("差¥" + (DeliveTotalFee - amount).toFixed(2) + "起送");
    } else {
        $(sbottom).find("button").removeClass("disabled");
        $(sbottom).find("button").html("结算");
    }
    if (amount == 0) {
        $(sbottom).find("button").addClass("disabled");
        $(sbottom).find("button").html("¥" + (DeliveTotalFee - amount).toFixed(2) + "起送");
    }
    //空购物车不可展开
    if ($(modulpopup).find(".settle-popup-body #product1 li").length == 0 && $(modulpopup).find(".settle-popup-body #product2 li").length == 0) {
        $(sbottom).find(".s-cart").addClass("disabled");
        $(modulpopup).removeClass("is-visible");
        return;
    }

}


function checkBuyNum_Cart(num) {
    var maxnum = isNaN(num.attr("stock")) ? 0 : parseInt(num.attr("stock"));

    return checkBranchSkuBuyNum(num, maxnum);
}
function editcart_Cart(numObj) {
    var sku = numObj.parents("li").find(".check-custom").attr("skuid");
    var bid = numObj.parents("li").find(".check-custom").attr("bid");
    var num = numObj.val();
    if (num.length == 0) {
        $.dialog.errorTips("请输入购买数量！");
        return false;
    }
    editToCartByBranch(sku, num, bid, function () {
        if (num == 0) {
            numObj.parents("li").remove();
        }
        SetBottomData();
        //调取页面购物车控制
        if (editPageBuyNum) {
            editPageBuyNum(sku, num);
        }
    });
}

function editPageBuyNum(id, num) {
    if (id == "buynum")
        $('.buynum').val(0);
    else {
        var obj = $('#' + id);
        obj.val(num);
        if (num <= 0) {
            obj.addClass("not-visible");
            obj.parent().find('.store-minus').addClass("not-visible");
        }
    }
}