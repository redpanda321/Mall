$(function () {
    InvoiceOperationInit();
});

var paymentShown = false;
var loading;
var orderIds = '';
function integralSubmit(ids, isgroup) {
    ids = ids.replace(/(.+?)\,+$/, "$1");
    ajaxRequest({
        type: 'POST',
        url: '/' + areaName + '/Order/PayOrderByIntegral',
        param: { orderIds: ids },
        dataType: 'json',
        success: function (data) {
            if (data.success == true) {
                $.dialog.succeedTips('支付成功！', function () {
                    //location.href = '/' + areaName + '/Member/Orders';
                    if (!isgroup || orderIds.indexOf(",") > 0) {
                        location.href = '/' + areaName + '/Order/OrderShare?orderids=' + orderIds + '&returnUrl=/m-wap/Member/Orders?orderStatus=0';
                    } else {
                        //拼团跳转
                        window.location.href = '/' + areaName + "/FightGroup/GroupOrderOk?orderid=" + orderIds;
                    }
                }, 0.5);

            }
        },
        error: function (data) { $.dialog.tips('支付失败,请稍候尝试.', null, 0.5); }
    });
}

$('#submit-order').click(function () {
    var collPIds = $("#collPIds").val();
    var cartItemIds = QueryString('cartItemIds');
    //发票相关
    //var invoiceType = 0
    //var invoiceTitle = "";
    //var invoiceCode = "";
    //var invoiceContext = "";

    //if (!($(".bill a").text() == "不需要发票")) {
    //    invoiceType = 2;
    //    invoiceTitle = $(".bill a").text();
    //    invoiceCode = $(".bill #invoicecode").text();;
    //    invoiceContext = $(".bill-Cart .content-bill .active ").parent().text();
    //}

    //是否货到付款
    var isCashOnDelivery = false;
    if ($("#icod").val() == "True") {
        isCashOnDelivery = $(".way-01 .offline").hasClass("active");
    }

    var integral = 0;
    if (isintegral) {
        integral = $("#useintegral").val();
        integral = isNaN(integral) ? 0 : integral;
    }
    var capital = 0;
    if (iscapital) {
        capital = $("#userCapitals").val();
        capital = isNaN(capital) ? 0 : capital;
    }
    var couponIds = "";
    $('input[name="couponIds"]').each(function (i, e) {
        var type = $(this).attr("data-type");
        couponIds = couponIds + $(e).val() + '_' + type + ',';
    })
    if (couponIds != '') {
        couponIds = couponIds.substring(0, couponIds.length - 1);
    }
    var latAndLng = $("#latAndLng").val();
    var recieveAddressId = $('#shippingAddressId').val();
    recieveAddressId = parseInt(recieveAddressId);
    recieveAddressId = isNaN(recieveAddressId) ? null : recieveAddressId;
    var productType = $("#productType").val();
    if (!recieveAddressId && productType==0)
        $.dialog.alert('请选择或新建收货地址');
    else {
        var model = {};
        model.cartItemIds = cartItemIds;
        model.latAndLng = latAndLng;
        model.recieveAddressId = recieveAddressId;
        model.couponIds = couponIds;
        model.integral = integral;
        model.capital = capital;
        model.isCashOnDelivery = isCashOnDelivery;
        //model.invoiceType = invoiceType;
        //model.invoiceTitle = invoiceTitle;
        //model.invoiceCode = invoiceCode;
        //model.invoiceContext = invoiceContext;
        model.collPIds = collPIds;
        model.groupActionId = $('#groupActionId').val();
        model.groupId = $('#groupId').val();
        model.payPwd = $("#PayCapitalPwd").val();

        var isTrue = false;
        var orderShops = [];
        $('.goods-info[shopid]').each(function () {
            var shopId = $(this).attr('shopid');
            var orderShop = {};
            orderShop.shopId = shopId;
            orderShop.orderSkus = [];
            $('.item[skuid][count]', this).each(function () {
                orderShop.orderSkus.push({ skuId: $(this).attr('skuid'), count: $(this).attr('count') });
            });
            var deliveryType = $('input:radio[name="shop{0}.DeliveryType"]:checked'.format(shopId));
            orderShop.deliveryType = deliveryType.val();
            orderShop.shopBrandId = deliveryType.attr('sbid');

            if (orderShop.deliveryType == "1" && !orderShop.shopBrandId) {
                $.dialog.tips('到店自提必须选择门店！');
                isTrue = true;
                return false;
            }
            orderShop.remark = $('.orderRemarks#remark_' + shopId).val();
            if (orderShop.remark.length > 200) {
                $.dialog.tips('留言信息至多200个汉字！');
                isTrue = true;
                return false;
            }
            //发票
            var para = {};            
            var $form = $('div[name="formInvoice' + shopId + '"]');
            var typeid = parseInt($form.find('.dvInvoiceType span.active').attr('data-id'));
            var typename = $('.bill-title' + shopId).html();
            if (typename == "不需要发票")
                typeid = 0;
            if (typeid > 0) {
                var title = $form.find('.dvInvoiceRise span.active').html();
                var code = "", s = "";
                if (title.indexOf("公司") > -1 && typeid != 3 && typeid != 0) {
                    title = $form.find('input[name="invoicename"]').val();
                    if ($.trim(title) == "") {
                        $.dialog.errorTips('发票公司名必填！');
                        isTrue = true;
                        return;
                    }
                    code = $form.find('input[name="invoicecode"]').val();
                    if ($.trim(code) == "") {
                        $.dialog.errorTips('发票税号必填！');
                        isTrue = true;
                        return;
                    }
                }
                para.InvoiceTitle = title;
                para.InvoiceCode = code;                
                var context = $form.find('.dvInvoiceContext span.active').html();
                if ($.trim(context) == "") {
                    $.dialog.errorTips('请选择发票类容！');
                    isTrue = true;
                    return;
                }                
                if (typeid == 2) {
                    var cellphone = $form.find('input[name="cellphone"]').val();
                    var email = $form.find('input[name="email"]').val();
                    if (!cellphone) {
                        $.dialog.errorTips("请输入收票人手机号");
                        isTrue = true;
                        return;
                    }
                    if (!email) {
                        $.dialog.errorTips("请输入收票人邮箱");
                        isTrue = true;
                        return;
                    }
                    para.CellPhone = cellphone;
                    para.Email = email;
                }

                if (typeid == 3) {
                    $form.find('.vatInvoice input').each(function () {
                        if ($.trim($(this).val()) == "") {
                            $.dialog.errorTips("增值税发票项均为必填");
                            isTrue = true;
                            return;
                        }
                    });
                    para.InvoiceTitle = $form.find('input[name="companyname"]').val();
                    para.InvoiceCode = $form.find('input[name="companyno"]').val();
                    para.RegisterAddress = $form.find('input[name="registeraddress"]').val();
                    para.RegisterPhone = $form.find('input[name="registerphone"]').val();
                    para.BankName = $form.find('input[name="bankname"]').val();
                    para.BankNo = $form.find('input[name="bankno"]').val();
                    para.RealName = $form.find('input[name="vatrealname"]').val();
                    para.CellPhone = $form.find('input[name="vatcellphone"]').val();
                    para.RegionID = regionid || $form.find('input[name="RegionID"]').val();
                    //para.InvoiceContext = $form.find(".dvInvoiceContextVat .invoice-item-selected span").html();
                    para.Address = $form.find('input[name="vataddress"]').val();
                }
            }
            para.InvoiceType = typeid;
            para.InvoiceContext = context;
            orderShop.PostOrderInvoice = para;
            orderShops.push(orderShop);
            window.localStorage.removeItem("invoiceInfo" + shopId);
        });
        if (isTrue) {
            return false;
        }
        model.orderShops = orderShops;
        //虚拟商品用户信息项
        model.virtualProductItems = JSON.parse(window.localStorage.getItem("virtualProductItemJson"));
        model.ProductType = productType;

        loading = showLoading(); 
        var total = parseFloat($("#total").val());
        $.post('/' + areaName + '/Order/IsAllDeductible', { integral: model.integral, total: total }, function (result) {
            if (result.data) {
                loading.close();
                $.dialog.confirm("您确定用积分抵扣全部金额吗?", function () {
                    submit(model);
                });
            }
            else {
                submit(model, loading);
            }
        });
    }
});

function submit(model, loading) {
    if (loading == null)
        loading = showLoading();
    var url = '/' + areaName + '/Order/SubmitOrder';
    if (isLimitTimeBuy == "True") {
        url = '/' + areaName + '/Order/SubmitLimitOrder';
    }
    $.post(url, model, function (result) {
        if (result.success) {
            localStorage.removeItem("virtualProductItemJson");
            if (result.data.orderIds != null && result.data != undefined) {
                orderIds = result.data.orderIds;//当前订单号
                //在货到付款，且只有一个店铺时
                if (model.isCashOnDelivery && model.orderShops.length == 1) {
                    loading.close();
                    if (result.data.realTotalIsZero) {
                        integralSubmit(result.data.orderIds.toString(), model.groupActionId > 0);
                    }
                    else {
                        $.dialog.succeedTips('提交成功！', function () {
                            location.href = '/' + areaName + '/Member/Orders';
                        }, 0.5);
                    }
                }
                else if (result.data.realTotalIsZero) {
                    loading && loading.close();
                    integralSubmit(result.data.orderIds.toString(), model.groupActionId > 0);
                }
                else {
                    loading && loading.close();
                    GetPayment(result.data.orderIds.toString());
                }
            }
            else if (result.data != null && result.data != undefined) {//限时购
                var requestcount = 0;
                ///检查订单状态并做处理
                function checkOrderState() {
                    $.getJSON('/OrderState/Check', { Id: result.data }, function (r) {
                        if (r.state == "Processed" && r.Total === 0) {
                            loading && loading.close();
                            integralSubmit(r.orderIds[0].toString(), model.groupActionId > 0);
                        }
                        else if (r.state == "Processed") {
                            loading && loading.close();
                            GetPayment(r.orderIds[0].toString());
                        }
                        else if (r.state == "Untreated") {
                            requestcount = requestcount + 1;
                            if (requestcount <= 10)
                                setTimeout(checkOrderState, 0);
                            else {
                                $.dialog.tips("服务器繁忙,请稍后去订单中心查询订单");
                                loading && loading.close();
                            }
                        }
                        else {
                            $.dialog.tips('订单提交失败,错误原因:' + r.message);
                            loading && loading.close();
                        }
                    });
                }
                checkOrderState();
            }
            else {
                loading && loading.close();
                $.dialog.alert(result.msg, function () {
                    window.location.href = window.location.href;
                });
            }
        } else {
            loading && loading.close();
            $.dialog.alert(result.msg, function () {
                window.location.href = window.location.href;
            });
        }
    });
}

$(document).on('click', '#paymentsChooser .close', function () {
    $('.cover,.custom-dialog').hide();
    $('#capitalstepone').remove();
    $('#payCapitalPwd').remove();
    if ($("#userCapitalSwitch").is(':checked')) {
        $('#userCapitalSwitch').click();
    }
    if (paymentShown) {//如果已经显示支付方式，则跳转到订单列表页面
        //location.href = '/' + areaName + '/Member/Orders';
        location.href = '/common/site/pay?area=mobile&platform=' + areaName.replace('m-', '') + '&controller=member&action=orders&neworderids=' + orderIds;
    }
});

//计算总价格
function CalcPrice() {
    var sum = 0;
    $('.goods-info .item .price').each(function () {
        var pr = $(this).data('price');
        if (pr == undefined || pr == null || pr == "")
            pr = 0;
        sum += parseFloat(pr);
        //sum += parseFloat($(this).data('price'));
    });
    var enabledIntegral = $('#userIntegralSwitch').is(':checked');
    if (enabledIntegral) {
        sum -= parseFloat($("#integralPerMoney").val());
        $("#integralPrice").html("￥-" + $("#integralPerMoney").val());
    }

    var invoice = CalInvoiceRate(parseFloat($("#integralPerMoney").val()));
    sum = sum + invoice;

    var enabledCapital = $("#userCapitalSwitch").is(':checked');
    if (enabledCapital) {
        var totalCapital = parseFloat($("#capitalAmount").val());
        var inputcapital = parseFloat($("#capital").val());
        var capital = totalCapital;
        if (sum <= 0) {
            capital = 0;
            if (inputcapital != capital) {
                $("#capital").val(capital.toFixed(2));
            }
            $("#capitalPrice").html("￥-" + capital.toFixed(2));
            $("#userCapitals").val(capital.toFixed(2));
        } else {
            if (!inputcapital || inputcapital < 0) {
                inputcapital = 0;
            }
            if (inputcapital > totalCapital) {
                inputcapital = totalCapital;
            }
            if (sum <= inputcapital) {
                capital = sum;
            } else {
                capital = inputcapital;
            }
            //重新计算余额
            if (isResetUseCapital && totalCapital > 0) {
                if (totalCapital < sum) {
                    capital = totalCapital;
                } else {
                    capital = sum;
                }
                isResetUseCapital = false;
            }
            sum -= capital;
            if (inputcapital != capital) {
                $("#capital").val(capital.toFixed(2));
            }
            $("#capitalPrice").html("￥-" + capital.toFixed(2));
            $("#userCapitals").val(capital.toFixed(2));
        }
    }
    if (sum <= 0) sum = 0;
    $('#allTotal').html('¥' + MoneyRound(sum)).attr('data-alltotal', MoneyRound(sum));
}

function getCount() {
    var result = [];
    $('.goods-info[shopid]').each(function () {
        var shopId = $(this).attr('shopid');
        $('.item[pid][count]', this).each(function () {
            var pid = $(this).attr('pid');
            var count = $(this).attr('count');
            var amount = count * parseFloat($(this).attr('price'));//总金额
            result.push({ shopId: shopId, productId: pid, count: count, amount: amount });
        });
    });

    return result;
}

function freeFreight(shopId) {
    var goodsInfo = $('.goods-info#' + shopId);
    var priceElement = goodsInfo.find('.item .price');
    var oldPrice = parseFloat(priceElement.data('oldprice'));
    var price = oldPrice - parseFloat(priceElement.data('freight'));
    priceElement.html('￥' + MoneyRound(price)).data('price', price);
    goodsInfo.find(".showfreight").html("免运费");
    CalcPrice();
}

//刷新运费
function refreshFreight(regionId) {
    //获取运费
    var data = getCount();
    $.post('/{0}/order/CalcFreight'.format(areaName), { parameters: data, addressId: regionId }, function (result) {
        if (result.success == true) {
            for (var i = 0; i < result.data.length; i++) {
                var item = result.data[i];
                var shopId = item.shopId;
                var freight = item.freight;

                var priceDiv = $('.goods-info#{0} .price'.format(shopId));
                var amount = parseFloat(priceDiv.data('price')) - parseFloat(priceDiv.data('freight'));
                var freeFreightAmount = parseFloat(priceDiv.data('freefreight'));
                if (freeFreightAmount <= 0 || amount < freeFreightAmount) {
                    $('.goods-info#{0} .showfreight'.format(shopId)).html('￥' + MoneyRound(freight));
                    priceDiv.data('price', amount + freight).data('freight', freight).html('￥' + MoneyRound(amount + freight));
                }
                if (priceDiv.is('[selftake]'))
                    freeFreight(shopId);
            }
            CalcPrice();
        } else
            $.dialog.errorTips(result.msg);
    });
}

//加载发票事件
var regionid,
				showCity = document.getElementsByName('showCity'),
				province = {}, cityPicker3, selectCityName = '', selectAreaName = '';
function InvoiceOperationInit() {
    //发票弹框动画
    $('.bill').click(function (e) {
        var shopId = $(this).parent().attr('shopid');
        e.stopPropagation();
        $('.cover').show();
        $('div[name="divInvoice' + shopId + '"]').show();
    });
    $('.couponCover').click(function (e) {
        e.stopPropagation();
        $('.cover').hide();
        $('.bill-Cart').hide();
    });
    $(".dvInvoiceType span.active").each(function () {
        var _this = $(this);
        var d = parseInt(_this.attr('data-id'));
        var s = _this.attr('data-shopid');
        var $fromInvoice = $('div[name="formInvoice' + s + '"]');
        switch (d) {
            case 0:
                break;
            case 1:
                $fromInvoice.find('.elInvoice').hide();
                $fromInvoice.find('.dvInvoiceTitle').show();
                $fromInvoice.find('.vatInvoice').hide();
                $fromInvoice.find('.invoiceDay').hide();
                break;
            case 2:
                $fromInvoice.find('.elInvoice').show();
                $fromInvoice.find('.dvInvoiceTitle').show();
                $fromInvoice.find('.vatInvoice').hide();
                $fromInvoice.find('.invoiceDay').hide();
                break;
            case 3:
                $fromInvoice.find('.elInvoice').hide();
                $fromInvoice.find('.dvInvoiceTitle').hide();
                $fromInvoice.find('.vatInvoice').show();
                $fromInvoice.find('.invoiceDay').show();
                break;
        }
    });
    $('.dvInvoiceType span').click(function () {
        var shopId = parseInt($(this).attr('data-shopid'));
        var $fromInvoice = $('div[name="formInvoice' + shopId + '"]');
        $fromInvoice.find('.dvInvoiceType span').removeClass("active");
        $(this).addClass("active");
        var type = parseInt($(this).attr('data-id'));
        switch (type) {
            case 0:
                break;
            case 1:
                $fromInvoice.find('.elInvoice').hide();
                $fromInvoice.find('.dvInvoiceTitle').show();
                $fromInvoice.find('.vatInvoice').hide();
                $fromInvoice.find('.invoiceDay').hide();
                break;
            case 2:
                $fromInvoice.find('.elInvoice').show();
                $fromInvoice.find('.dvInvoiceTitle').show();
                $fromInvoice.find('.vatInvoice').hide();
                $fromInvoice.find('.invoiceDay').hide();
                break;
            case 3:
                $fromInvoice.find('.elInvoice').hide();
                $fromInvoice.find('.dvInvoiceTitle').hide();                
                $fromInvoice.find('.vatInvoice').show();
                $fromInvoice.find('.invoiceDay').show();
                break;
        }
    });
    $('.dvInvoiceRise span').click(function () {
        $(this).parent().find('span').removeClass("active");
        $(this).addClass("active");
        var type = parseInt($(this).attr('data-id'));
        if(type == 0)
        {
            $(this).parent().parent().find('.dvInvoincCompany').hide();

        } else {
            $(this).parent().parent().find('.dvInvoincCompany').show();
        }
    });
    $('.dvInvoiceContext span').click(function () {
        $(this).parent().find('span').removeClass("active");
        $(this).addClass("active");
    });
    $('input[name="invoicename"]').focus(function () {
        $(this).parent().parent().find('.companylist').show();
    });
    $('input[name="invoicename"]').keyup(function () {
        var val = $(this).val();
        if (val == "")
            $(this).parent().parent().find('.companylist').show();
        else
            $(this).parent().parent().find('.companylist').hide();
    });
    $('ul.companylist li a').click(function () {
        //alert($(this).attr('data-id'));
        var parentDiv = $(this).parent().parent().parent().parent();
        parentDiv.find('input[name="invoicename"]').val($(this).text());
        parentDiv.find('input[name="invoicecode"]').val($(this).attr('data-code'));
        $(this).parent().parent().parent().find('.companylist').hide();

    });

    $('.noInvoice').click(function () {
        var shopId = parseInt($(this).attr('data-shopid'));
        var $form = $(this).parent().parent().parent().parent().find('div[name="formInvoice' + shopId + '"]');
        $form.find('.dvInvoiceType span').removeClass('active');
        $form.find('.dvInvoiceType span').eq(0).addClass('active');
        var s = '不需要发票';
        $('.bill-title' + shopId).html(s);
        CalcPrice();
        $('.cover').hide();
        $('.bill-Cart').hide();
    });

    $('.bill-submit').click(function () {
        var shopId = parseInt($(this).attr('data-shopid'));
        var $form = $(this).parent().find('div[name="formInvoice' + shopId + '"]');
        var typename = $form.find('.dvInvoiceType span.active').html();
        var typeid = parseInt($form.find('.dvInvoiceType span.active').attr('data-id'));
        var rate = parseFloat($form.find('.dvInvoiceType span.active').attr('data-rate'));
        var title = $form.find('.dvInvoiceRise span.active').html();
        var code = "", s = "";
        if (title.indexOf("公司") > -1 && typeid != 3 && typeid != 0) {
            title = $form.find('input[name="invoicename"]').val();
            if ($.trim(title) == "") {
                $.dialog.errorTips('公司名必填！');
                return;
            }
            code = $form.find('input[name="invoicecode"]').val();
            if ($.trim(code) == "") {
                $.dialog.errorTips('税号必填！');
                return;
            }
        }
        var context ="";
        if (typeid > 0) {
            context = $form.find('.dvInvoiceContext span.active').html();
            if ($.trim(context) == "") {
                $.dialog.errorTips('请选择发票类容！');
                return;
            }
        }
        s = '<span style="' + (rate <= 0 ? "display:none" : "") + '">(税率<span class="taxprice red" shopid="' + shopId + '">' + rate + '%</span>)</span> ' + typename + '(' + title + ')';
        var para = {};
        para.InvoiceType = typeid;
        var isSubmit = false;
        switch (typeid) {
            case 0:
                $.dialog.errorTips("请选择发票类型");
                return;
                //s = '不需要发票';
                //$('.bill-title' + shopId).html(s);
                //CalcPrice();
                //$('.cover').hide();
                //$('.bill-Cart').hide();
                break;
            case 1:
                para.Name = title;
                para.Code = code;
                para.InvoiceContext = context;
                isSubmit = true;
                break;
            case 2:
                var cellphone = $form.find('input[name="cellphone"]').val();
                var email = $form.find('input[name="email"]').val();
                if (!cellphone) {
                    $.dialog.errorTips("请输入收票人手机号");
                    return;
                }
                if (!email) {
                    $.dialog.errorTips("请输入收票人邮箱");
                    return;
                }
                para.Name = title;
                para.Code = code;
                para.InvoiceContext = context;
                para.CellPhone = cellphone;
                para.Email = email;
                isSubmit = true;
                break;
            case 3:
                para.Name = $form.find('input[name="companyname"]').val();
                para.Code = $form.find('input[name="companyno"]').val();
                para.RegisterAddress = $form.find('input[name="registeraddress"]').val();
                para.RegisterPhone = $form.find('input[name="registerphone"]').val();
                para.BankName = $form.find('input[name="bankname"]').val();
                para.BankNo = $form.find('input[name="bankno"]').val();
                para.RealName = $form.find('input[name="vatrealname"]').val();
                para.CellPhone = $form.find('input[name="vatcellphone"]').val();
                para.RegionID = regionid || $form.find('input[name="RegionID"]').val();
                para.InvoiceContext = context;
                para.Address = $form.find('input[name="vataddress"]').val();
                isSubmit = true;
                s = '<span style="' + (rate <= 0 ? "display:none" : "") + '">(税率<span class="taxprice red" shopid="' + shopId + '" style="' + (rate <= 0 ? "display:none" : "") + '">' + rate + '%</span>)</span> ' + typename + '(' + para.Name + ')';
                break;
        }

        if (isSubmit) {
            var loading = showLoading();
            $.post("/" + areaName + "/order/SaveInvoiceTitleNew", para, function (result) {
                loading.close();
                if (result.success) {
                    $('.bill-title' + shopId).html(s);
                    CalcPrice();
                    $('.cover').hide();
                    $('.bill-Cart').hide();
                    //写入发票信息缓存
                    window.localStorage.setItem("invoiceInfo" + shopId, JSON.stringify(para));
                }
                else {
                    $.dialog.tips(result.msg);
                }
            });
        }
    });

    $(".dvInvoincCompany .del-title").click(function () {
        var self = this;
        var id = $(self).attr("data-id");
        $.dialog.confirm("确定删除该发票抬头吗？", function () {
            var loading = showLoading();
            $.post("/" + areaName + "/BranchOrder/DeleteInvoiceTitle", { id: id }, function (result) {
                loading.close();
                if (result.success == true) {
                    var _p = $(self).parent();
                    _p.remove();
                    $.dialog.tips('删除成功！');
                }
                else {
                    $.dialog.tips('删除失败！');
                }
            });
        });
    });    
    loadRegion();
}
function loadRegion() {  
    
    regionid = Number($("#RegionID").val());//如果是修改收货地址
    var _temp, _proIndex = 0, _cityIndex = 0, _districtIndex = 0, _streetIndex = 0;
    var _proId = 0, _cityId = 0, _districtId = 0, _streetId = 0;
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
                layer: 4,
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
            province = data;
            $(showCity).click(function () {
                var focus = document.querySelector(':focus');
                if (focus)
                    focus.blur();
                cityPicker3.show(function (items) {
                    $(showCity).val((items[0].Name || '') + " " + (items[1].Name || '') + " " + (items[2].Name || '') + " " + (items[3].Name || '').replace("其它", ""));
                    if (items[2].Name) {
                        selectCityName = items[2].Name;
                        selectAreaName = items[1].name;
                    }
                    else {
                        selectCityName = items[1].Name;//当用户先选择了地区，则定位搜索范围为用户选择区域。优先区县
                        selectAreaName = selectCityName;
                    }
                    if (!items[1].Id) {
                        regionid = items[0].Id;
                    } else {
                        if (!items[2].Id) {
                            regionid = items[1].Id;
                        } else {
                            regionid = items[2].Id;
                        }
                    }
                    $("#RegionID").val(regionid);
                });
            });
            if (Number(_proId) > 0) {//当修改收货地址的时候才进行
                _temp = province.filter(function (a, index) {
                    if (a.Id == _proId) {
                        _proIndex = index;
                    }
                    return a.Id == _proId;
                    return;
                });
                cityPicker3.pickers[0].setSelectedIndex(_proIndex);
                var ret = [];
                $.ajax({
                    url: '/common/RegionAPI/GetSubRegion',
                    type: 'get', //GET
                    async: false,    //或false,是否异步
                    data: { parent: _temp[0].Id, bAddAll: true },
                    timeout: 10000,    //超时时间
                    dataType: 'json',    //返回的数据格式：json/xml/html/script/jsonp/text
                    success: function (data, textStatus, jqXHR) {
                        ret = data;
                    }
                });
                _temp = ret.filter(function (a, index) {
                    if (a.Id == _cityId) {
                        _cityIndex = index;
                    }
                    return a.Id == _cityId;
                    return;
                });
                cityPicker3.pickers[1].setSelectedIndex(_cityIndex);
                ret = [];
                $.ajax({
                    url: '/common/RegionAPI/GetSubRegion',
                    type: 'get', //GET
                    async: false,    //或false,是否异步
                    data: { parent: _temp[0].Id, bAddAll: true },
                    timeout: 10000,    //超时时间
                    dataType: 'json',    //返回的数据格式：json/xml/html/script/jsonp/text
                    success: function (data, textStatus, jqXHR) {
                        ret = data;
                    }
                });
                _temp = ret.filter(function (a, index) {
                    if (a.Id == _districtId) {
                        _districtIndex = index;
                    }
                    return a.Id == _districtId;
                    return;
                });
                cityPicker3.pickers[2].setSelectedIndex(_districtIndex);
                ret = [];
                $.ajax({
                    url: '/common/RegionAPI/GetSubRegion',
                    type: 'get', //GET
                    async: false,    //或false,是否异步
                    data: { parent: _temp[0].Id, bAddAll: true },
                    timeout: 10000,    //超时时间
                    dataType: 'json',    //返回的数据格式：json/xml/html/script/jsonp/text
                    success: function (data, textStatus, jqXHR) {
                        ret = data;
                    }
                });
                _temp = ret.filter(function (a, index) {
                    if (a.Id == _streetId) {
                        _streetIndex = index;
                    }
                    return a.Id == _streetId;
                    return;
                });
                cityPicker3.pickers[3].setSelectedIndex(_streetIndex)
            }
        }
    });
}

//计算税费
function CalInvoiceRate(orderTotalIntegral) {
    var invoiceRate = 0.00;
    $('.taxprice').each(function () {
        var _rate = parseFloat($(this).text());
        if (_rate > 0) {
            var shopid = $(this).attr('shopid');
            var d_box = $("#" + shopid);
            var total = parseFloat(d_box.find('.price').data('price'));//商家商品价格
            var freight = parseFloat(d_box.find('.price').data('freight'));//运费
            var deliveryType = $('input:radio[name="shop{0}.DeliveryType"]:checked'.format(shopid));
            if (deliveryType.val() == 1)
                freight = 0;
            total = total - freight;
            var enabledIntegral = $('#userIntegralSwitch').is(':checked');
            if (enabledIntegral) {
                if (orderTotalIntegral > 0) {
                    var productTotal = 0;//商品总价（商品价-优惠券-满额减）
                    $('.price').each(function () {
                        productTotal = parseFloat(productTotal) + (parseFloat($(this).attr('data-price')) - parseFloat($(this).attr('data-freight')));
                    });
                    var integralAmount = parseFloat(orderTotalIntegral * (total / productTotal)).toFixed(2);
                    total = total - integralAmount;
                }
            }            
            invoiceRate = MoneyRound(parseFloat(invoiceRate) + parseFloat((total * (_rate / 100))));
        }
    });

    if (invoiceRate > 0) {
        $('#divInvoiceAmount').show();
        $('#invoiceAmount').html("￥" + invoiceRate).attr('data-amount', invoiceRate);
    } else
        $('#divInvoiceAmount').hide();
    return invoiceRate;
}
