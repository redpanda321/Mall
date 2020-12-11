var shippingAddressIdChange = false;
var disabledSelfTake = false;
var selectShopBranch = {};
var productType = $("#productType").val();
$(function () {
    disabledSelfTake = $("#isNeedUpdate").val() == 1;
    if (disableAllSelfTake == true) {
        disableAllSelfTake();
    }
    bindRecieverEdit();
    bindSubmit();
    initAddress();
    bindAddressRadioClick();
    InvoiceInit();
    InvoiceOperationInit();

    $('#shippingAddressRegionId').change(function () {
        shippingAddressIdChange = true;

        disableAllSelfTake();

        var regionId = $('#shippingAddressRegionId').val();
        if ($.isNullOrEmpty(regionId))
            return;
        var loading = showLoading();
        $('label.deliverytype').each(function () {
            var btn = $(this);
            var shopId = btn.closest('.ordsumbox').data('shopid');
            var pids = btn.data('pids').toString().split(',');

            $.ajax({
                url: '/shop/ExistShopBranch?shopId={0}&regionId={1}&productIds={2}'.format(shopId, regionId, pids.join('&productIds=')),
                context: { element: btn },
                async: false,
                success: function (result) {
                    if (result == true)
                        enableSelfTake(this.element);
                }
            });
        });

        $.get('/order/IsCashOnDelivery?addreddId=' + regionId, function (result) {
            loading.close();
            $("#icod").val(result);//是否支持货到付款
            if (!result) {
                disableCashOnDelivery();
            } else {
                disableCashOnDelivery(false);
            }
        }).error(function () {
            $("#icod").val("false");//是否支持货到付款
            disableCashOnDelivery();
            loading.close();
        });

        refreshFreight(regionId);
    });

    $("input[name=pay-offline]").change(function () {
        if ($(this).val() == "1") {
            $(".payment-dialog").hide();
            $(".sticky-wrap .offline-icon").html("在线支付");

            disableAllSelfTake(false);
        }
        else {
            $(".payment-dialog").show();
            $(".id_alpha").addClass("alpha");
            $("#submit").attr("disabled", "");
        }
    })

    $(".pd-submit").click(function () {
        $(".payment-dialog").hide();
        $("input[name=pay-offline]:eq(0)").attr("checked", "checked");
        $(".id_alpha").hide();
        $("#submit").removeAttr("disabled");
    })

    $(".pd-commit").click(function () {
        $(".sticky-wrap .offline-icon").html("货到付款")
        $(".payment-dialog").hide();
        $(".id_alpha").hide();
        $("#submit").removeAttr("disabled");

        var selfShopId = 1;
        disableAllSelfTake(true, selfShopId);
        $('#orderdata_{0} label.deliverytype'.format(selfShopId)).each(function () {
            $(this).parent().find('.tip').addClass('hide');
        });
    })

    $(document).on('click', '.thickclose', function () {
        $('.thickbox,.thickdiv').hide();

        var shopbranchDialog = $(this).closest('.shopbranch.dialog');
        if (shopbranchDialog.length > 0) {
            var sourceElement = $(shopbranchDialog.get(0).sourceElement);
            var shopId = sourceElement.closest('.ordsumbox').data('shopid');
            if (selectShopBranch[shopId] == null) {
                var ckb = sourceElement.prev().children();
                ckb.get(0).checked = true;
                ckb.change();
            }
        }

        return false;
    });

    var freight = $('#modelFreight').val();
    var intRule = $('#MoneyPerIntegral').data('rule');

    $("#integral").bind("keyup", function () {
        ShowPaidPrice();
    });

    $("#capital").bind("change", function () {
        ShowPaidPrice();
    });

    $("#IsUsedIntegral").change(function () {
        if (this.checked) {
            $(this).siblings().show().end().parent().siblings().show();
            var _d = $('#integral');
            _d.val(_d.data("userintegral"));
            _d.trigger("keyup");
        } else {
            $(this).siblings().hide().end().parent().siblings().hide();
            $('#integral').val(0);
            $("#integral").trigger("keyup");
        }
    });

    $("#IsUsedCapital").change(function () {
        if (this.checked) {
            $(this).removeAttr("checked");
            validCapitalPay(this);
        } else {
            $(this).siblings().hide().end().parent().siblings().hide();
            $('#capital').val(0);
            $("#capital").trigger("change");
        }
    });

    $("#MoneyPerIntegral").html();

    var price = function () {
        var a = 0;
        $('.shopb').each(function (i, e) {
            $(e).children().each(function (l, k) {
                if ($(k).attr('selected')) {
                    var b = $(k).attr('data-p');
                    a += (b | 0);
                }
            });
        });
        return a;
    },
    total = $('#payPriceId').attr('data');

    //优惠切换绑定
    $('.shopb').bind('change', function (t) {
        var _t = $(this);
        var shopid = _t.data("shopid");
        ComputeOrder(shopid);
        //如果已选自提，则无运费
        var checkbox = $('input:radio[name="shop{0}.DeliveryType"]:checked'.format(shopid));
        var deliverytype = checkbox.val();
        if (deliverytype == 1) {
            freeFreight(shopid);
        }
    });

    //下面两个调用需同步执行
    //优惠初始
    //$('.shopb').each(function (i, e) {
    //    var _t = $(e);
    //    _t.find("option").eq(1).attr("selected", true);
    //});

    //初始计算
    $(".ordsumbox").each(function (i, e) {
        var _t = $(e);
        var shopid = _t.data("shopid")
        ComputeOrder(shopid);
    });

    $('label.deliverytype input').change(function () {
        if (this.checked) {
            $(this).parent().siblings('.selectStore').show();
        }
    });

    $('input.express').change(function (e) {
        if (this.checked) {
            $(this).parent().siblings('.selectStore').hide();
            var shopId = $(this).closest('.ordsumbox').data('shopid');
            $('.shopbranchname[shopid={0}]'.format(shopId)).html('').parent().hide();
            $('.shopbranchaddress[shopid={0}]'.format(shopId)).html('').parent().hide();
            selectShopBranch[shopId] = null;
            ComputeOrder(shopId);
            if (e && shopId == 1)
                $('.payment-selected input.CashOnDelivery[name="pay-offline"]').parent().show();
        }
    });

    $('.selectStore').click(function () {
        var regionId = $('#shippingAddressRegionId').val();

        if ($.isNullOrEmpty(regionId)) {
            $.dialog.tips('请选择或新建收货地址');
            return;
        }

        var btn = $(this).siblings('label.deliverytype');
        if (btn.is('[disabled]'))
            return;
        $('.thickdiv').show();
        var shopId = btn.closest('.ordsumbox').data('shopid');
        var dialog = $('.shopbranch.dialog#' + shopId).show();
        var dialogElement = dialog.get(0);
        if (dialogElement.isInit != true || shippingAddressIdChange == true) {
            shippingAddressIdChange = false;
            var skuIds = btn.attr('skuIds').split(',');
            var counts = btn.attr('counts').split(',');
            var width = dialog.width() / 2;
            var height = dialog.height() / 2;
            dialog.css({ top: '40%', left: '50%', marginLeft: '-' + width + 'px', marginTop: '-' + height + 'px', position: 'fixed' });
            var pid = dialog.data('pid');
            dialogElement.isInit = true;
            dialogElement.sourceElement = this;
            dialogElement.source = [];

            $.get('/shop/GetCitySubRegions?regionId=' + regionId, function (data) {

                if (data && data.regions && data.regions.length > 0) {
                    var str = '<option value="">所有区域</option>';
                    for (var i = 0; i < data.regions.length; i++) {
                        str += '<option value="' + data.regions[i].id + '" ' + (data.regions[i].id == data.selectId ? 'selected' : '') + '>' + data.regions[i].name + '</option>';
                    }
                    $('.districtSelect', dialog).html(str).change(function () {
                        if (!this.value) {
                            $('.list', dialog).MallDatagrid('reload', { regionId: regionId, getParent: true });
                        } else {
                            $('.list', dialog).MallDatagrid('reload', { regionId: this.value, getParent: false });
                        }
                    });
                }
            });

            $(".list", dialog).MallDatagrid({
                url: '/order/GetShopBranchs',
                NoDataMsg: '该地区没有可自提门店',
                pagination: false,
                idField: "id",
                queryParams: { shopId: shopId, regionId: regionId, getParent: false, skuIds: skuIds, counts: counts, shippingAddressId: $("#shippingAddressId").val() },
                columns:
				[[
					{
					    width: 57,
					    formatter: function (value, row, index) {
					        var html = '<input id="shopbranch{0}" name="shopBranchId" type="radio" value="{0}" onchange="onSelectShopBranch(this)"';
					        if (row.enabled == false) {
					            html += ' disabled ';
					        }
					        html += "/>";
					        dialogElement.source.push(row);
					        return html.format(row.id);
					    }
					},
					{
					    field: "shopBranchName",
					    formatter: function (value, row, index) {
					        var dis = "";
					        if (row.distance > 0) {
					            if (row.distance > 1000) {
					                dis = (row.distance / 1000).toFixed(2) + "KM";
					            } else {
					                dis = row.distance + "M";
					            }
					            dis = "<span class=\"sb_distance red ml10\">距离" + dis + "</span>";
					        }
					        return '<div style="margin-bottom:15px; font-size:15px; color:#000;">{0}{4} {5}</div><div style="margin-bottom:10px;">自提地址：{1}</div><div style="margin-bottom:30px">联系方式：{2}</div>'
                                .format(row.shopBranchName, row.addressDetail, row.contactPhone, row.id, row.enabled == false ? '<label style="color: #fff; background: #f8413d;padding: 0 10px; border-radius: 2px;margin-left:5px;">#该门店缺货</label>' : '', dis);
					    }
					}
				]]
            });
        } else {
            var checkRadio = $('input:radio[name=shopBranchId]:checked', dialog);
            if (checkRadio.length > 0) {
                var data = dialogElement.source.first(function (p) { return p.id == checkRadio.val(); });
                $('.shopbranchname[shopid={0}]'.format(shopId)).html(data.shopBranchName);
                $('.shopbranchaddress[shopid={0}]'.format(shopId)).html('{0} {1} {2}'.format(data.addressDetail, data.contactUser, data.contactPhone));
                freeFreight(shopId);
                selectShopBranch[shopId] = checkRadio.val();
                $('.shopbranchname[shopid={0}]'.format(shopId)).parent().show();
                $('.shopbranchaddress[shopid={0}]'.format(shopId)).parent().show();
            }
        }
    });

    if (productType == 1) {
        $("div[id^='VProductItemImages_']").each(function () {
            var _t = $(this).attr("id");
            $(this).MallUpload({
                imgFieldName: _t,
                imagesCount: 3,
                maxSize: 3,
                canDel: true,
                isMobile: false
            });
        });
        //$("#VProductItemImages1").MallUpload({
        //    imgFieldName: "VProductItemImages1",
        //    imagesCount: 3,
        //    maxSize: 3,
        //    canDel: true
        //});
        $(".j_date").datetimepicker({
            language: 'zh-CN',
            format: 'yyyy-mm-dd',
            autoclose: true,
            weekStart: 1,
            minView: 2
        });
        $(".j_time").datetimepicker({
            language: 'zh-CN',
            format: 'hh:ii',
            autoclose: true,
            minView: 0,
            startView: 1,   //日期时间选择器打开之后首先显示的视图,0分，1时，2月，默认是2；
            minuteStep: 1,
            weekStart: 1,
            //formatViewType:'time'
        });
        $(".j_date,.j_time").attr("readonly", "readonly");
    }
});

function validCapitalPay(target) {
    $.ajax({
        type: 'post',
        url: 'GetPayPwd',
        async: false,
        dataType: 'json',
        success: function (result) {
            if (!result.success) {
                $.dialog.confirm("请先设置支付密码", function () {
                    window.top.open('/UserCapital/SetPayPwd', '_blank');
                });
            } else {
                $.dialog({
                    title: '确认支付',
                    lock: true,
                    id: 'goodCheck',
                    width: '280px',
                    height: '100px',
                    content: ['<div class="dialog-form">',
                        '<div class="form-group">',
                           '<div class="item">\
                                 <span class="label" style="position:relative;top:-15px;">支付密码：</span>\
                                    <div class="">\
                                    <input type="password" value="" id="payPwd" onkeyup="getVal(this)" name="userVo.realName" maxlength="20" class="itxt fl">\
                            </div>\
                            </div>',
                        '</div>',
                    '</div>'].join(''),
                    padding: '10px',
                    init: function () { $("#payPwd").focus(); },
                    button: [
                    {
                        name: '确认',
                        callback: function () {
                            var pwd = $("#payPwd").val();
                            if (pwd.length == 0) {
                                $.dialog.alert("请输入支付密码");
                                return false;
                            }
                            $.post('ValidPayPwd', { pwd: pwd }, function (result) {
                                if (result.success) {
                                    $(target).attr("checked", "checked").siblings().show().end().parent().siblings().show();
                                    $('#capital').val($("#capital").data("usercapital"));
                                    $("#capital").trigger("change");
                                    $("#PayPwd").val(pwd);
                                }
                                else {
                                    $.dialog.alert(result.msg);
                                    return false;
                                }
                            });
                        },
                        focus: true
                    },
                    {
                        name: '取消',
                        callback: function () { }
                    }]
                });
            }
        }
    });
}

function focusInput() {
    $("#payPwd").focus();
}
function getVal(obj) {
    var txtLen = $(obj).val().length, html = "";
    for (var i = 0, len = txtLen; i < len; i++) {
        html = html + "<span></span>";
    }
    $("#pwdShow").html(html);
}
function getCount() {
    var result = [];
    $('.order-table[shopid]').each(function () {
        var shopId = $(this).attr('shopid');
        $('tr[pid][quntity]', this).each(function () {
            var pid = $(this).attr('pid');
            var count = $(this).attr('quntity');
            var amount = count * parseFloat($(this).attr('price'));//总金额
            result.push({ shopId: shopId, productId: pid, count: count, amount: amount });
        });
    });

    return result;
}

//刷新运费
function refreshFreight(regionId) {
    //获取运费
    var data = getCount();
    $.post('/order/CalcFreight', { parameters: data, addressId: regionId }, function (result) {
        if (result.success == true) {
            for (var i = 0; i < result.freights.length; i++) {
                var item = result.freights[i];
                var shopId = item.shopId;
                var freight = item.freight;

                var shopDiv = $('#orderdata_' + shopId);
                var amount = parseFloat($('.shopd', shopDiv).data('v'));
                var freeFreight = parseFloat($('.shopf', shopDiv).data('free'));
                if (freeFreight <= 0 || amount < freeFreight) {
                    $('.shopd', shopDiv).attr('data', amount + freight).html('￥' + MoneyRound(amount + freight));
                    shopDiv.find('.shopf').attr('data', freight).data('profrei', freight).html('￥' + MoneyRound(freight));
                }
            }
            ShowPaidPrice();
        } else
            $.dialog.errorTips(result.msg);
    });
}

//禁用货到付款
function disableCashOnDelivery(flag) {
    $('.payment-selected input[name="pay-offline"]')[0].checked = true;
    var lbl = $('.payment-selected input.CashOnDelivery[name="pay-offline"]').parent();
    if (flag != false) {
        lbl.hide();
    } else if ($('#orderdata_1').length > 0)//判断是否有平台店
        lbl.show();
}

//启用到店自提
function enableSelfTake(element) {
    if (disabledSelfTake) return;
    element.removeAttr('disabled');
    element.children().removeAttr('disabled');
    element.parent().find('.tip').addClass('hide');
    element.parent().find('.tip.have').removeClass('hide');
}

//禁用所有门店到店自提
function disableAllSelfTake(flag, shopId) {
    var filter = '';
    if (shopId)
        filter = '#orderdata_{0} '.format(shopId);

    $(filter + '.express[name$=".DeliveryType"]').each(function () {
        this.checked = true;
        selectShopBranch[shopId] = null;
        $(this).change();
    });
    $(filter + 'label.deliverytype').each(function () {
        var element = $(this);
        if (flag != false) {
            element.attr('disabled', 'disabled');
            element.children().attr('disabled', 'disabled');
            element.parent().find('.tip').removeClass('hide');
            element.parent().find('.tip.have').addClass('hide');
        } else {
            enableSelfTake(element);
        }
    });
}

//计算税费
function CalInvoiceRate(orderTotalIntegral) {
    var invoiceRate = 0.00;
    $('.invoicerate').each(function (i, e) {
        var _rate = parseFloat($(this).text());
        if (_rate > 0) {
            var shopid = $(this).attr('shopid');
            var d_box = $("#orderdata_" + shopid);
            var shopcamount = parseFloatOrZero(d_box.find(".shopc").attr('data'));
            var total = parseFloatOrZero(d_box.find('.shopd').data('v')) - shopcamount;//商家商品价格
            if (orderTotalIntegral > 0) {
                var productTotal = 0;//商品总价（商品价-满额减）
                $('.shopd').each(function () {
                    productTotal = parseFloatOrZero(productTotal) + parseFloatOrZero($(this).attr('data-v'));
                });
                var cTotal = 0;
                $('.shopc').each(function () {
                    cTotal = parseFloatOrZero(cTotal) + parseFloatOrZero($(this).attr('data'));
                });
                productTotal = productTotal - cTotal;
                var integralAmount = parseFloat(orderTotalIntegral * (total / productTotal)).toFixed(2);
                total = total - integralAmount;
            }
            invoiceRate = MoneyRound(parseFloat(invoiceRate) + parseFloatOrZero((total * (_rate / 100))));
        }
    });

    if (invoiceRate > 0) {
        $('#divTotalInvoice').show();
        $('#totalInvoice').html("￥" + invoiceRate);
    } else
        $('#divTotalInvoice').hide();
    return invoiceRate;
}

function freeFreight(shopId) {
    var d_box = $("#orderdata_" + shopId);
    var d_freight = d_box.find(".shopf");
    var freight = d_freight.data('profrei');
    d_freight.attr('data', 0).html('免运费');
    var shopd = d_box.find('.shopd');

    var d_dissel = d_box.find(".shopb");
    var shopprice = parseFloat(shopd.attr('data')) - MoneyRound(parseFloat(freight));
    var ordSubDisPrice = 0;
    if (d_dissel.length > 0) {
        var d_selopt = d_dissel.find("option:selected");
        ordSubDisPrice = parseFloatOrZero(d_selopt.data("p"));
        if (ordSubDisPrice > shopprice) {
            ordSubDisPrice = shopprice;
        }
    }
    shopd.html('￥' + MoneyRound(shopprice - ordSubDisPrice));
    //shopd.html('￥' + (parseFloat(shopd.attr('data')) - MoneyRound(parseFloat(freight))));
    ShowPaidPrice();
}

function onSelectShopBranch(input) {
    input = $(input);
    var shopId = input.closest('.shopbranch.dialog').hide().attr('id');
    var id = input.val();
    $('.thickdiv').hide();
    selectShopBranch[shopId] = id;
    var data = input.closest('.shopbranch.dialog').get(0).source.first(function (p) { return p.id == id; });
    $('.shopbranchname[shopid={0}]'.format(shopId)).html(data.shopBranchName);
    $('.shopbranchaddress[shopid={0}]'.format(shopId)).html('{0} {1} {2}'.format(data.addressDetail, data.contactUser, data.contactPhone));
    ComputeOrder(shopId);
    if (shopId == 1)//如果是自营店
        disableCashOnDelivery();
    //选择门店到店自提时，免运费
    freeFreight(shopId);
    $('.shopbranchname[shopid={0}]'.format(shopId)).parent().show();
    $('.shopbranchaddress[shopid={0}]'.format(shopId)).parent().show();
}

function InvoiceInit() {
    $("div[name^='invoceMsg']").hide();
    $("input[name^='isInvoce']").click(function () {
        var id = $(this).attr("class");
        var shopid = $(this).attr("date-shopid");
        if (id == "isInvoce1") {
            $("div[name^='invoceMsg" + shopid + "'").hide();
            $("div[name^='invoceMsg" + shopid + "'").find('.invoicerate').removeClass('invoicerate').addClass('invoiceratehide');
        }
        else if (id == "isInvoce2") {
            var title = $("div[name^='dvInvoice'] .invoice-tit-list .invoice-item-selected input").val();
            var context = $(".dvInvoiceContext .invoice-item-selected span").html();
            $("#invoiceTitle").html(title);
            $("#invoiceContext").html(context);
            $("div[name^='invoceMsg" + shopid + "'").show();
            $("div[name^='invoceMsg" + shopid + "'").find('.invoiceratehide').removeClass('invoiceratehide').addClass('invoicerate');
        }
        ShowPaidPrice();
        //CalInvoiceRate();
    })

    //$("#dvInvoice .dvInvoiceContext div:eq(0)").addClass("invoice-item-selected");
    $(".dvInvoiceContext div").click(function () {
        $(this).parent().find('div').removeClass("invoice-item-selected");
        $(this).addClass("invoice-item-selected");
    });

    $(".dvInvoiceContextVat div").click(function () {
        $(this).parent().find('div').removeClass("invoice-item-selected");
        $(this).addClass("invoice-item-selected");
    });

    //$(".dvInvoiceType div:eq(0)").addClass("invoice-item-selected");

    $(".dvInvoiceType div.invoice-item-selected").each(function () {
        var _this = $(this);
        var d = _this.attr('data-id');
        var s = _this.attr('data-shopid');
        if (d == 3) {
            $('.invoicetype1-' + s).hide();
            $('.invoicetype2-' + s).show();
            $("#area-selector" + s).RegionSelector({
                selectClass: "sele",
                valueHidden: "#RegionID" + s
            });
        } else {
            $('.invoicetype1-' + s).show();
            $('.invoicetype2-' + s).hide();
            if (d == 2)
                $('.Electronic' + s).show();
            else {
                $('.Electronic' + s).hide();
            }
        }
    });
    
    $(".dvInvoiceType div").click(function () {
        $(this).parent().find('div').removeClass("invoice-item-selected");
        $(this).addClass("invoice-item-selected");

        var shopid = $(this).attr('data-shopid');
        var typeid = $(this).attr('data-id');
        if (typeid == 3) {
            $('.invoicetype1-' + shopid).hide();
            $('.invoicetype2-' + shopid).show();
            $("#area-selector" + shopid).RegionSelector({
                selectClass: "sele",
                valueHidden: "#RegionID" + shopid
            });
        } else {
            $('.invoicetype1-' + shopid).show();
            $('.invoicetype2-' + shopid).hide();
            if (typeid == 2)
                $('.Electronic' + shopid).show();
            else {
                $('.Electronic' + shopid).hide();
            }
        }
    })

    $("div[name^='dvInvoice'] .invoice-tit-list div.invoice-item").click(function () {
        if (!$(this).hasClass("editting")) {
            //$("#dvInvoice .invoice-tit-list div").removeClass("invoice-item-selected");
            $(this).parent().find('div.invoice-item').removeClass("invoice-item-selected");
            $(this).addClass("invoice-item-selected");
        }
    });

    $(".btnAddInvoice").click(function () {
        var _t = $(this);
        _t.hide();
        var html = '<div class="invoice-item">';
        html += '公司：<input type="text" value="" class="invoicename" >';
        html += '税号：<input type="text" value="" class="invoicecode" >';
        html += '<div class="item-btns">';
        html += '<a href="javascript:void(0);" class="ml10 update-tit">保存</a>';
        html += '<a href="javascript:void(0);" class="ml10 set-tit hide">编辑</a>';
        html += '<a href="javascript:void(0);" class="ml10 del-tit hide">删除</a>';
        html += '</div>';
        html += '</div>';
        var shopId = _t.attr('data-shopid');
        var $form = $('.formInvoice' + shopId);
        $form.find('.invoice-tit-list .invoice-item').removeClass("invoice-item-selected");
        $form.find('.invoice-tit-list').prepend(html);
        $form.find('.invoice-tit-list .invoice-item input:first').focus();

        InvoiceOperationInit();
    });

    $(".btnOk").click(function () {
        var shopId = $(this).attr('data-shopid');
        var $form = $('.formInvoice' + shopId);
        var type = $form.find('.dvInvoiceType div.invoice-item-selected span').html();
        var rate = $form.find('.dvInvoiceType div.invoice-item-selected').attr('data-rate');
        var typeid = parseInt($form.find('.dvInvoiceType div.invoice-item-selected').attr('data-id'));
        var title = $form.find('.invoice-item-selected .invoicename').val();
        var code = $form.find(".invoice-tit-list .invoice-item-selected .invoicecode").val();
        var context = $form.find(".dvInvoiceContext .invoice-item-selected span").html();
        if (typeid == 3) {
            title = $form.find('input[name="companyname"]').val();
            context = $form.find(".dvInvoiceContextVat .invoice-item-selected span").html();
        }
        if (title == null || context == null) {
            $.dialog.tips("请选择发票信息,信息内容不能为空");
            return;
        }
        else if (title.length > 0 && context.length > 0 && $.trim(title) != "") {
            var para = {};
            para.InvoiceType = typeid;
            var isSubmit = false;
            switch (typeid) {
                case 1:
                    if ($.trim(title) == "") {
                        $.dialog.tips('公司名称不能为空！');
                        return;
                    }
                    if (title.indexOf("公司") > -1 && $.trim(code) == "") {
                        $.dialog.tips('税号不能为空！');
                        return;
                    }                    
                    para.Name = title;
                    para.Code = code;
                    para.InvoiceContext = context;
                    isSubmit = true;
                    break;
                case 2:
                    var cellphone = $form.find('input[name="cellphone"]').val();
                    var email = $form.find('input[name="email"]').val();
                    if (!cellphone) {
                        $.dialog.tips("请输入收票人手机号");
                        return;
                    }
                    if (!email) {
                        $.dialog.tips("请输入收票人邮箱");
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
                    para.RegionID = $form.find('input[name="RegionID"]').val();
                    para.InvoiceContext = $form.find(".dvInvoiceContextVat .invoice-item-selected span").html();
                    para.Address = $form.find('input[name="vataddress"]').val();
                    isSubmit = true;
                    break;
            }
            if (isSubmit) {
                var loading = showLoading();
                $.post("/order/SaveInvoiceTitleNew", para, function (result) {
                    loading.close();
                    if (result.success) {
                        var $invoceMsg = $("div[name^='invoceMsg" + shopId + "'");
                        var $invoiceInfo = $("div[name^='invoiceInfo" + shopId + "'");
                        $invoceMsg.find(".invoiceType").html(type + '<span style="' + (rate <= 0 ? "display:none" : "") + '">(税率<em class="red invoicerate" shopid="' + shopId + '">' + rate + '%</em>)</span>');
                        $invoceMsg.find(".invoiceTitle").parent().find(".code").remove();
                        $invoceMsg.find(".invoiceTitle").html(title);
                        $invoceMsg.find(".invoiceContext").html(context);
                        if (typeid == 3) {
                            $invoceMsg.find(".invoiceTitle").html(para.Name);
                            $invoceMsg.find(".invoiceContext").html(para.InvoiceContext);
                        }
                        $invoiceInfo.find('.btnInvoiceEdit').show();
                        $('.thickbox,.thickdiv').hide();

                        ShowPaidPrice();
                        //CalInvoiceRate();
                        //if (code.length > 0 && $.trim(code.val()) != "") {
                        //    $(".invoiceTitle" + shopId).after("<span class=\"mr15 code\">税号：</span><span id=\"invoiceCode\" class=\"mr15 code\">" + code.val() + "</span>");
                        //}
                    }
                    else {
                        $.dialog.tips(result.msg);
                    }
                });
            }
        }
        else {
            $.dialog.tips("请选择发票信息,信息内容不能为空");
        }

    })

    $("div[name^='dvInvoice'] .invoice-tit-list").on("click", ".set-tit", function (e) {
        var self = this;
        $(self).addClass("hide").prev().removeClass("hide").parents(".invoice-item").removeClass("invoice-item-selected").addClass("editting")
            .find("input").removeAttr("disabled").eq(0).focus();
        return false;
    });
}

function InvoiceOperationInit() {
    $("div[name^='dvInvoice'] .invoice-tit-list .del-tit").click(function () {
        var self = this;
        var id = $(self).attr("key");
        $.dialog.confirm("确定删除该发票抬头吗？", function () {
            var loading = showLoading();
            $.post("./DeleteInvoiceTitle", { id: id }, function (result) {
                loading.close();
                if (result == true) {
                    $(self).parents(".invoice-item").remove();
                    $(".invoice-tit-list .invoice-item:eq(0)").addClass("invoice-item-selected");
                    $.dialog.tips('删除成功！');
                }
                else {
                    $.dialog.tips('删除失败！');
                }
            })
        });
    });

    $("div[name^='dvInvoice'] .invoice-tit-list .update-tit").click(function () {
        var self = this;
        var name = $(this).parents(".invoice-item").find(".invoicename").val();
        var code = $(this).parents(".invoice-item").find(".invoicecode").val();
        if ($.trim(name) == "") {
            $.dialog.tips('公司名称不能为空！');
            return;
        }
        if ($.trim(code) == "") {
            $.dialog.tips('税号不能为空！');
            return;
        }
        var loading = showLoading();
        var id = $(self).attr("key");
        $.post("./SaveInvoiceTitle", { name: name, code: code, id: id }, function (result) {
            loading.close();
            if (result != undefined && result != null && result > 0) {
                $(self).parents(".invoice-item").find(".del-tit").removeClass("hide").attr("key", result);
                $(self).parents(".invoice-item").find(".set-tit").removeClass("hide").attr("key", result);
                $(self).addClass("hide").attr("key", result);
                $(self).parents(".invoice-item").find("input").attr("disabled", true);
                $(self).parent().removeClass("invoice-item-selected");
                $(self).parents(".invoice-item").removeClass("editting").addClass("invoice-item-selected");

                $("div[name^='dvInvoice'] .invoice-tit-list div.invoice-item").click(function () {
                    if (!$(this).hasClass("editting")) {
                        $(self).parent().removeClass("invoice-item-selected")
                        $(this).addClass("invoice-item-selected");
                    }
                })
                $.dialog.tips('保存成功！');
                $(".btnAddInvoice").show();
            }
            else {
                if (result == -1) {
                    $.dialog.tips('发票抬头不可为空！');
                } else {
                    $.dialog.tips('保存失败！');
                }
            }
        });
        return false;
    });

    $("div[name^='dvInvoice'] .invoice-tit-list div.invoice-item").click(function () {
        if (!$(this).hasClass("editting")) {
            $(this).parent().find('div.invoice-item').removeClass("invoice-item-selected");
            $(this).addClass("invoice-item-selected");
        }
    });
}


function initAddress() {
    if (!$('#selectedAddress').html()) {
        $('#editReciever').click();
    }
}

var shippingAddress = [];

function bindRecieverEdit() {
    $('#editReciever').click(function () {
        $.post('/order/GetUserShippingAddresses', {}, function (addresses) {
            var html = '';
            var currentSelectedId = parseInt($('#shippingAddressId').val());
            $.each(addresses, function (i, address) {
                shippingAddress[address.id] = address;
                var addressDetail = address.addressDetail || '';
                html += '<div class="item" name="li-' + address.id + '">\
                          <label>\
                             <input type="radio" class="hookbox pull-left" name="address" ' + (address.id == currentSelectedId ? 'checked="checked"' : '') + ' value="' + address.id + '" latlng="' + address.latitude + ',' + address.longitude + '" />\
                             <span class="item-text pull-left"><b>' + address.shipTo + '</b>&nbsp; ' + address.fullRegionName + ' &nbsp; ' + address.address + ' &nbsp; ' + addressDetail + ' &nbsp; ' + address.phone + ' </span>&nbsp\
                          </label>\
                          <span class="item-action">\
                              <a href="javascript:;" onclick="showEditArea(\'' + address.id + '\',0)">编辑</a> &nbsp;\
                              <a href="javascript:;" onclick="deleteAddress(\'' + address.id + '\')">删除</a>&nbsp;\
                              '+ (address.NeedUpdate ? "<a href=\"javascript:void(0);\" onclick=\"showEditArea(\'" + address.id + "\',1)\">升级地址</a> &nbsp;" : "") + '\
                          </span>\
                      </div>';
            });
            $('#consignee-list').html(html).show();
            $('#step-1').addClass('step-current');
            $('#addressListArea').show();

            $('input[name="address"]').change(function () {
                var shippingAddressId = $(this).val();
                $("#latAndLng").val($(this).attr("latlng"));
                $('#shippingAddressId').val(shippingAddressId).change();
            });
        });
    });
}


function bindAddressRadioClick() {
    $('#consignee-list').on('click', 'input[type="radio"]', function () {
        $('#addressEditArea').hide();

    });
}


function deleteAddress(id) {
    $.dialog.confirm('您确定要删除该收货地址吗？', function () {
        var loading = showLoading();
        $.post('/UserAddress/DeleteShippingAddress', { id: id }, function (result) {
            loading.close();
            if (result.success) {
                var current = $('div[name="li-' + id + '"]');
                if ($('input[type="radio"][name="address"]:checked').val() == id) {
                    $('input[type="radio"][name="address"]').first().click();
                    $('#selectedAddress').html('');
                    $("#latAndLng").val("0,0");
                    $('#shippingAddressId').val('').change();
                }
                current.remove();
            }
            else
                $.dialog.errorTips(result.msg);
        });
    });
}


function saveAddress(id, regionId, shipTo, address, addressdetail, phone, callBack) {
    id = isNaN(parseInt(id)) ? '' : parseInt(id);

    var url = '';
    if (id)
        url = '/UserAddress/EditShippingAddress';
    else
        url = '/UserAddress/AddShippingAddress';

    var data = {};
    if (id)
        data.id = id;
    data.regionId = regionId;
    data.shipTo = shipTo;
    data.address = address;
    data.phone = phone;
    data.Latitude = $("#Latitude").val();
    data.Longitude = $("#Longitude").val();
    data.AddressDetail = addressdetail;

    var loading = showLoading();
    $.post(url, data, function (result) {
        loading.close();
        if (result.success)
            callBack(result);
        else
            $.dialog.errorTips('保存失败!' + result.msg);
    });
}

var isSubmitLoading = false;
function bindSubmit() {
    $('#submit').click(function () {
        if (!isSubmitLoading) {
            isSubmitLoading = true;
            var loading = showLoading();
            var fn = function () {
                var arr = [];
                $('.shopb').each(function (i, e) {
                    $(e).children().each(function (l, k) {
                        if ($(k).attr('selected')) {
                            var b = $(k).val();
                            var s = b + "_" + $(k).attr("data-type");
                            arr.push(s);
                        }
                    });
                });
                return arr.join(',');
            };

            //var collIds = $("#collIds").val();
            var collIds = [];
            var couponIds = fn();

            var cartItemIds = QueryString('cartItemIds');
            var recieveAddressId = $('#shippingAddressId').val();
            var latAndLng = $("#latAndLng").val();

            var orderShops = [], hasError = false;
            $('.order-table[shopid]').each(function () {
                if (hasError == true)
                    return false;

                var shopId = $(this).attr('shopid');
                var orderShop = {};
                orderShop.shopId = shopId;
                orderShop.orderSkus = [];
                $('tbody tr', this).each(function () {
                    orderShop.orderSkus.push({ skuId: $(this).attr('skuid'), count: $(this).attr('quntity') });
                    if ($(this).attr('collpid') > 0) {
                        collIds.push($(this).attr('collpid'));
                    }
                });
                var checkbox = $('input:radio[name="shop{0}.DeliveryType"]:checked'.format(shopId));
                orderShop.deliveryType = checkbox.val();
                orderShop.shopBrandId = selectShopBranch[shopId];
                if (productType == 0) {
                    if (orderShop.shopBrandId == null && checkbox.hasClass('express') == false) {
                        $.dialog.tips('请选择自提门店');
                        hasError = true;
                        return false;
                    }
                }

                orderShop.remark = $('.orderRemarks#remark_' + shopId).val();
                if (orderShop.remark.length > 200) {
                    $.dialog.tips('留言信息至多200个汉字！');
                    hasError = true;
                    return false;
                }

                //发票
                var para = {};
                var invoiceType = parseInt($("input[name='isInvoce" + shopId + "']:checked").val());
                if ($("input[name='isInvoce" + shopId + "']").parents(".step").is(":hidden")) {//商家不提供发票，则默认不需要发票
                    invoiceType = 0;
                }

                if (invoiceType > 0) {
                    var $form = $('.formInvoice' + shopId);
                    var typeid = parseInt($form.find('.dvInvoiceType div.invoice-item-selected').attr('data-id'));
                    var title = $form.find('.invoice-item-selected .invoicename').val();
                    var code = $form.find(".invoice-tit-list .invoice-item-selected .invoicecode").val();
                    var context = $form.find(".dvInvoiceContext .invoice-item-selected span").html();
                    invoiceType = typeid;

                    if (typeid != 3) {
                        if (title == null || title == '') {
                            $.dialog.tips('请选择发票抬头');
                            hasError = true;
                            return false;
                        }
                        if (title != '个人') {
                            if (code == null || code == '') {
                                $.dialog.tips('请选择发票税号');
                                loading.close();
                                hasError = true;
                                return false;
                            }
                        }
                    }
                    para.InvoiceTitle = title;
                    para.InvoiceCode = code;
                    if (typeid == 3) {
                        context = $form.find(".dvInvoiceContextVat .invoice-item-selected span").html();
                    }
                    if (context == null || context == '') {
                        $.dialog.tips('请选择发票内容');
                        loading.close();
                        hasError = true;
                        return false;
                    }
                    para.InvoiceContext = context;

                    if (typeid == 2) {
                        var cellphone = $form.find('input[name="cellphone"]').val();
                        var email = $form.find('input[name="email"]').val();
                        if (!cellphone) {
                            $.dialog.errorTips("请输入收票人手机号");
                            hasError = true;
                            return;
                        }
                        if (!email) {
                            $.dialog.errorTips("请输入收票人邮箱");
                            hasError = true;
                            return;
                        }
                        para.CellPhone = cellphone;
                        para.Email = email;
                    }

                    if (typeid == 3) {
                        $form.find('.vatInvoice input').each(function () {
                            if ($.trim($(this).val()) == "") {
                                $.dialog.errorTips("增值税发票项均为必填");
                                hasError = true;
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
                        para.RegionID = $form.find('input[name="RegionID"]').val();
                        para.InvoiceContext = $form.find(".dvInvoiceContextVat .invoice-item-selected span").html();
                        para.Address = $form.find('input[name="vataddress"]').val();
                    }
                }
                para.InvoiceType = invoiceType;                
                orderShop.PostOrderInvoice = para;
                //orderShop.InvoiceType = invoiceType;
                orderShops.push(orderShop);
            });

            if (hasError == true) {
                loading.close();
                isSubmitLoading = false;
                return false;
            }

            var integral = parseInt($("#integral").val());
            integral = isNaN(integral) ? 0 : integral;
            var capital = parseFloatOrZero($("#capital").val());
            var paypwd = $("#PayPwd").val();

            //是否货到付款
            var isCashOnDelivery = false;
            if (($("#icod").val() == "True" || $("#icod").val() == "true") && $("input[name=pay-offline]:checked").val() == "2") {
                isCashOnDelivery = true;
            }

            recieveAddressId = parseInt(recieveAddressId);
            recieveAddressId = isNaN(recieveAddressId) ? null : recieveAddressId;

            $('input:radio[name="sex"]').is(":checked")

            //var invoiceType = $("input[name='isInvoce']:checked").val();
            //if ($("input[name='isInvoce']").parents(".step").is(":hidden")) {//商家不提供发票，则默认不需要发票
            //    invoiceType = "0";
            //}
            //if ($("input[name='invoiceType']").is(":checked"))
            //    invoiceType = $("input[name='invoiceType']:checked").val();

            //var invoiceTitle = $("#invoiceTitle").html();
            //if (invoiceTitle == null || invoiceTitle == '') {
            //    invoiceTitle = "";
            //    //$.dialog.tips( '请选择发票抬头' );
            //    //return false;
            //}
            //var objInvoiceCode = $("#invoiceCode");
            //var invoiceCode = "";
            //if (objInvoiceCode.length > 0) {
            //    invoiceCode = objInvoiceCode.html();
            //    //$.dialog.tips( '请选择发票抬头' );
            //    //return false;
            //}
            //var invoiceContext = $("#invoiceContext").html();
            //if (invoiceContext == null || invoiceContext == '') {
            //    invoiceContext = "";
            //    //$.dialog.tips( '请选择发票内容' );
            //    //return false;
            //}

            //if (invoiceType == "2") {
            //    if (invoiceTitle == null || invoiceTitle == '') {
            //        $.dialog.tips('请选择发票抬头');
            //        loading.close();
            //        isSubmitLoading = false;
            //        return false;
            //    }
            //    if (invoiceTitle != '个人') {
            //        if (invoiceCode == null || invoiceCode == '') {
            //            $.dialog.tips('请选择发票税号');
            //            loading.close();
            //            isSubmitLoading = false;
            //            return false;
            //        }
            //    }
            //    if (invoiceContext == null || invoiceContext == '') {
            //        $.dialog.tips('请选择发票内容');
            //        loading.close();
            //        isSubmitLoading = false;
            //        return false;
            //    }
            //}

            if ($("#IsUsedCapital").prop("checked")) {
                if (capital <= 0) {
                    $.dialog.tips('请输入预存款支付金额');
                    $("#capital").focus();
                    loading.close();
                    isSubmitLoading = false;
                    return false;
                }
            }

            var pass = true;
            var virtualProductItems = [];
            $("div.virtual").find("input[type=text]").removeClass("form-error");
            $("div.virtual").each(function () {
                var obj = new Object();
                obj.VirtualProductItemName = $(this).find(".j_name").html();
                obj.VirtualProductItemType = $(this).attr("ptype");
                obj.Content = $.trim($(this).find("input[type=text]").val());
                if (obj.VirtualProductItemType == 6) {
                    var itemid = $(this).data("itemid");
                    obj.Content = $(this).find("#VProductItemImages_" + itemid).MallUpload('getImgSrc').join(',');
                }
                if ($(this).hasClass("j_required")) {
                    if (obj.Content.length == 0) {
                        pass = false;
                        var name = $(this).find(".j_name").html();
                        if (obj.VirtualProductItemType == 6) {
                            $.dialog.errorTips("请上传图片");
                        } else {
                            $.dialog.errorTips("请输入" + name);
                        }
                        $("html,body").animate({ scrollTop: $("#checkout").offset().top }, 500);
                        $(this).find("input[type=text]").addClass("form-error");
                        loading.close();
                        isSubmitLoading = false;
                        return false;
                    }
                }
                if (obj.VirtualProductItemType == 4) {
                    var format = /^(([1][1-5])|([2][1-3])|([3][1-7])|([4][1-6])|([5][0-4])|([6][1-5])|([7][1])|([8][1-2]))\d{4}(([1][9]\d{2})|([2]\d{3}))(([0][1-9])|([1][0-2]))(([0][1-9])|([1-2][0-9])|([3][0-1]))\d{3}[0-9xX]$/;
                    //身份证码规则校验
                    if (obj.Content.length > 0 && !format.test(obj.Content)) {
                        pass = false;
                        $.dialog.errorTips("身份证输入的字符不符合规则！");
                        $(this).find("input[type=text]").focus();
                        loading.close();
                        isSubmitLoading = false;
                        return;
                    }
                }
                virtualProductItems.push(obj);
            });
            if (!pass) {
                return false;
            }
            if (productType == 0 && !recieveAddressId) {
                $.dialog.tips('请选择或新建收货地址');
                loading.close();
                isSubmitLoading = false;
            } else {

                $.post('/order/SubmitOrder', {
                    integral: integral, capital: capital, paypwd: paypwd, couponIds: couponIds, collpIds: collIds.join(','), cartItemIds: cartItemIds,
                    recieveAddressId: recieveAddressId,
                    isCashOnDelivery: isCashOnDelivery, orderShops: orderShops, latAndLng: latAndLng, virtualProductItems: virtualProductItems, productType: productType
                },
					function (result) {
					    if (isCashOnDelivery && $("#onlyshop1").val() == "True") {
					        loading.close();
					        //isSubmitLoading = false;

					        location.replace('/UserCenter?url=/UserOrder&tar=UserOrder');
					    }
					    else if (result.success) {//订单提交成功
					        if (!result.redirect) {
					            loading.close();
					            $.dialog.succeedTips("订单支付成功！", function () {
					                location.replace('/UserOrder');
					            });
					            return;
					        }
					        if (result.orderIds != null && result != undefined) {
					            location.replace('/order/pay?orderIds=' + result.orderIds.toString());
					            loading.close();
					            //isSubmitLoading = false;
					        }
					        else {
					            ///请求次数
					            var requestcount = 0;
					            ///检查订单状态并做处理
					            function checkOrderState() {
					                $.getJSON('/OrderState/Check', { Id: result.Id }, function (r) {
					                    if (r.state == "Processed") {
					                        location.href = '/order/pay?orderIds=' + r.orderIds.toString();
					                    }
					                    else if (r.state == "Untreated") {
					                        requestcount = requestcount + 1;
					                        if (requestcount <= 10)
					                            setTimeout(checkOrderState, 0);
					                        else {
					                            $.dialog.tips("服务器繁忙,请稍后去订单中心查询订单");
					                            loading.close();
					                            isSubmitLoading = false;
					                        }

					                    }
					                    else {
					                        $.dialog.tips('订单提交失败,错误原因:' + r.message);
					                        loading.close();
					                        isSubmitLoading = false;
					                    }
					                });
					            }
					            checkOrderState();
					        }
					    }
					    else {
					        loading.close();
					        isSubmitLoading = false;
					        $.dialog.errorTips(result.msg);
					    }
					});
            }
        }
    });
}

function showEditArea(id, isneedupdate) {
    $('input[name="address"][value="' + id + '"]').click();

    var address = shippingAddress[id];
    var addressdetail = address == null ? "" : address.addressDetail;
    var shipTo = address == null ? '' : address.shipTo;
    var addressName = address == null ? '' : address.address;
    var phone = address == null ? '' : address.phone;
    if (address) {
        var arr = (address.fullRegionName).split(' ');
    }
    //arr[2] = arr[2] || '';
    var fullRegionName = address == null ? '<i></i><em></em>' : '<i>' + arr[0] + ' </i><em>' + arr[1] + ' </em>' + arr[2] + '';

    $('input[name="shipTo"]').val(shipTo);
    if (isneedupdate != 1) $('input[type="text"][name="address"]').val(addressName);
    $('input[type="text"][name="addressdetail"]').val(addressdetail);
    $('input[name="phone"]').val(phone);
    $('span[name="regionFullName"]').html('');
    $('span[name="regionFullName"]').html(fullRegionName);
    if (id > 0) {//只有编辑的时候要将经纬度回显
        $("#Latitude").val(address.latitude);
        $("#Longitude").val(address.longitude);
    }
    $('#addressEditArea').show();
    if (id === 0) {
        $('#consignee_province').val(0);
        $('#consignee_city').val(0);
        $('#consignee_county').val(0);
        $('#regionSelector select').find('option:first').prop('selected', true);
        $('#NewAddressId').attr('level', 0);
        return;
    }
    else {
        $('#NewAddressId').attr('level', 4);
    }
    var regionPath = address.fullRegionIdPath.split(',');
    $('#consignee_province').val(regionPath[0]);
    $('#consignee_province').trigger('change');
    $('#consignee_city').val(regionPath[1]);
    $('#consignee_city').trigger('change');
    $("#NewAddressId").val(regionPath[regionPath.length - 1]);
    $("#regionSelector").RegionSelector({
        valueHidden: "#NewAddressId"
    });
    $("#regionSelector select").change(function () {
        $("#consigneeAddress").val("");//每次选择地址后都要将详细地址清空，防止经纬度和地区不匹配
    });
    if (regionPath.length == 3) {
        $('#consignee_county').val(regionPath[2]);
        $('#consignee_county').trigger('change');
    }
}

//发票弹出框
function invoiceReceipt(shopid) {
    var left, top, width, height;
    left = $(window).width() / 2;
    top = $(window).height() / 2;
    var dialog = $('#invoiceDialog' + shopid);
    dialog.show().removeClass('hide');
    width = dialog.width() / 2;
    height = dialog.height() / 2;
    dialog.css({ top: '50%', left: '50%', marginLeft: '-' + width + 'px', marginTop: '-' + height + 'px', position: 'fixed' }).show();
}
//转换0
function parseFloatOrZero(n) {
    result = 0;
    try {
        if (n.length < 1) n = 0;
        if (isNaN(n)) n = 0;
        if (n != 0) {
            result = parseFloat(n);
        } else {
            result = 0;
        }
    } catch (ex) {
        result = 0;
    }
    return result;
}

//显示实付金额
//By DZY[150706]
function ShowPaidPrice() {
    var d_mpi = $('#MoneyPerIntegral');
    var d_integral = $("#integral");
    var d_capital = $("#capital");
    //价格初始
    var orderTotalPrice = parseFloatOrZero($("#warePriceId").attr("v"));  //订单总价
    var orderPaidPrice = 0;
    var orderTotalDisPrice = 0;       //订单优惠总价
    var orderTotalFreight = 0;        //订单运费总价
    var orderTotalIntegral = 0;       //订单积分抵扣
    //var orderTotalCapital = 0;        //订单使用预存款
    //运费总价
    $(".shopf").each(function (l, k) {
        var _t = $(k);
        orderTotalFreight += parseFloatOrZero(_t.attr("data"));
    });
    //总优惠
    $(".shopc").each(function (l, k) {
        var _t = $(k);
        orderTotalDisPrice += parseFloatOrZero(_t.attr("data"));
    });
    orderTotalPrice = MoneyRound(orderTotalPrice);
    //积分抵扣
    var intRule = parseFloatOrZero(d_integral.data('rule'));   //积分规则
    var intMaxRate = parseFloatOrZero(d_integral.data('maxuserate'));   //积分最高可用比例
    if (intRule > 0) {
        var useIntegral = parseFloatOrZero(d_integral.val());
        if (useIntegral < 0) {
            useIntegral = 0;
            d_integral.val(useIntegral);
        }
        var canuseint = parseFloatOrZero(d_integral.data("userintegral"));
        if (canuseint < useIntegral) useIntegral = canuseint;
        orderTotalIntegral = useIntegral / intRule;
        orderTotalIntegral = Math.floor(orderTotalIntegral * Math.pow(10, 2)) / Math.pow(10, 2);
        var maxDedPrice = MoneyRound((orderTotalPrice - orderTotalDisPrice) * intMaxRate / 100);
        if (maxDedPrice - orderTotalIntegral < 0) {
            //为零修正
            orderTotalIntegral = maxDedPrice;
            useIntegral = Math.ceil(orderTotalIntegral * intRule);
            //无法抵价
            if (useIntegral > 0) {
                orderTotalIntegral = MoneyRound(useIntegral / intRule);
            } else {
                orderTotalIntegral = 0;
                d_integral.val(0);
            }
            if (orderTotalIntegral > maxDedPrice) {
                orderTotalIntegral = maxDedPrice;
            }
        }
        //数据补充
        if (useIntegral != 0) {
            d_integral.val(useIntegral);
        } else {
            if (d_integral.val().length > 1) {
                d_integral.val(0);
            }
        }
    } else {
        d_integral.val(0);
    }

    //计算税费
    var invoiceAmount = parseFloatOrZero(CalInvoiceRate(orderTotalIntegral));

    //计算实付(预存款前)
    orderPaidPrice = (orderTotalPrice + orderTotalFreight + invoiceAmount - orderTotalDisPrice - orderTotalIntegral);

    //预存款
    var useCapital = parseFloatOrZero(d_capital.val());
    if (useCapital < 0) {
        useCapital = orderPaidPrice;
    }
    var canusecapital = parseFloatOrZero(d_capital.data("usercapital"));
    if (canusecapital < useCapital) {
        useCapital = canusecapital;
    }
    if (orderPaidPrice < useCapital) {
        useCapital = orderPaidPrice;
    }
    d_capital.val(MoneyRound(useCapital));
    //orderTotalCapital = useCapital;
    //重新计算实付(预存款之后)
    orderPaidPrice = orderPaidPrice - useCapital;

    //显示
    var d_ordprice = $("#payPriceId");
    var d_ordFreight = $("#totalFreight");
    var d_ordDisPrice = $("#id_c");
    var d_ordMPI = $("#MoneyPerIntegral");
    var d_orduseInt = $("#integralPrice");
    var d_orduseCapital = $("#capitalPrice");
    d_ordprice.attr("v", MoneyRound(orderPaidPrice)).html("￥" + MoneyRound(orderPaidPrice));
    d_ordFreight.html("￥" + MoneyRound(orderTotalFreight));
    d_ordFreight.attr('data-freight', MoneyRound(orderTotalFreight));
    d_ordDisPrice.html("￥-" + MoneyRound(orderTotalDisPrice));
    if (!isNaN(orderTotalIntegral)) {
        orderTotalIntegral = parseFloat(orderTotalIntegral);
    }
    d_orduseInt.html("￥-" + MoneyRound(orderTotalIntegral));
    d_orduseCapital.html("￥-" + MoneyRound(useCapital));
    d_ordDisPrice.parent().hide();
    if (orderTotalDisPrice > 0) {
        d_ordDisPrice.parent().show();
    }
    //可获得积分
    var mpirule = parseFloatOrZero(d_ordMPI.data("rule"));
    if ((orderPaidPrice - orderTotalFreight + useCapital > 0) && mpirule > 0) {
        d_ordMPI.text(Math.floor((orderPaidPrice - orderTotalFreight + useCapital) / mpirule));
    }
    else {
        d_ordMPI.text(0);
    }
    
    d_ordprice.attr("v", MoneyRound(orderPaidPrice)).html("￥" + MoneyRound(orderPaidPrice));
}

//计算订单结果
//By DZY[150707]
function ComputeOrder(shopid) {
    var d_box = $("#orderdata_" + shopid);
    var d_dissel = d_box.find(".shopb");
    var d_freight = d_box.find(".shopf");
    var d_proPrice = d_box.find(".shopd");
    var d_showdis = d_box.find(".shopc");
    var ordSumProPrice = 0, ordSubDisPrice = 0; ordSubFreight = 0, shopFreeFrei = 0;
    ordSumProPrice = parseFloatOrZero(d_proPrice.data("v"));
    shopFreeFrei = parseFloatOrZero(d_freight.data("free"));
    ordSubFreight = parseFloatOrZero(d_freight.data("profrei"));
    if (d_dissel.length > 0) {
        var d_selopt = d_dissel.find("option:selected");
        ordSubDisPrice = parseFloatOrZero(d_selopt.data("p"));
        if (ordSubDisPrice > ordSumProPrice) {
            ordSubDisPrice = ordSumProPrice;
        }
    }
    //计算满额免
    var isFullFreeFrei = false;
    if (shopFreeFrei > 0) {
        if (ordSumProPrice - ordSubDisPrice >= shopFreeFrei) {
            ordSubFreight = 0;
            isFullFreeFrei = true;
        }
    }

    //计算实付
    var ordPaidPrice = ordSumProPrice - ordSubDisPrice + ordSubFreight;

    //显示单条
    d_proPrice.html("￥" + MoneyRound(ordPaidPrice));
    d_freight.html("￥" + ordSubFreight).attr("data", ordSubFreight);
    if (isFullFreeFrei) {
        d_freight.html("免运费");
    }
    d_showdis.html("￥-" + ordSubDisPrice).attr("data", ordSubDisPrice);
    //d_showdis.hide();
    if (parseFloat(ordSubDisPrice) == 0) {
        d_showdis.html("");
    }
    //显示计算结果
    ShowPaidPrice();
}

function selectAddress(indexId, latlng) {

}

//绑定地址
$(function () {
    $('#saveConsigneeTitleDiv').click(function () {
        var indexId = $('input[type="radio"][name="address"]:checked').val();
        var latlng = $('input[type="radio"][name="address"]:checked').attr("latlng");
        if (!latlng || latlng.length <= 3) latlng = $("#Latitude").val() + "," + $("#Longitude").val();
        if ($('#addressEditArea').css('display') == 'none') {
            var selectedAddress = shippingAddress[indexId];
            var newSelectedText = '<span>' + selectedAddress.shipTo + '</span> ' + '<br />' + selectedAddress.fullRegionName + ' &nbsp; &nbsp;' + selectedAddress.address + ' &nbsp; &nbsp;' + (selectedAddress.addressDetail ? selectedAddress.addressDetail : "") + '</br>' + selectedAddress.phone;
            var isopenstore = $('#isOpenStore').val();
            if (isopenstore == 'True' && (!latlng || latlng.length <= 3)) {
                $.dialog.confirm('未获取到坐标地址，需更新编辑地址，否则无法使用上门自提功能，确定要继续使用此地址？', function () {
                    disabledSelfTake = true;
                    $("#latAndLng").val(latlng);
                    $('#shippingAddressId').val(indexId).change();
                    $('input[type="hidden"][name="ShippingAddressId"]').val(indexId);
                    $('#addressEditArea').hide();
                    $('#selectedAddress').html(newSelectedText);
                    $('#addressListArea').hide();
                    $('#consignee-list').hide();
                    $('#step-1').removeClass('step-current');
                    $('#shippingAddressRegionId').val(selectedAddress.regionId).change();
                });
            } else {
                disabledSelfTake = false;
                $("#latAndLng").val(latlng);
                $('#shippingAddressId').val(indexId).change();
                $('input[type="hidden"][name="ShippingAddressId"]').val(indexId);
                $('#addressEditArea').hide();
                $('#selectedAddress').html(newSelectedText);
                $('#addressListArea').hide();
                $('#consignee-list').hide();
                $('#step-1').removeClass('step-current');
                $('#shippingAddressRegionId').val(selectedAddress.regionId).change();
            }

            //CalcFreight(indexId);
        }
        else {

            var shipTo = $('input[name="shipTo"]').val();
            var regTel = /([\d]{11}$)|(^0[\d]{2,3}-?[\d]{7,8}$)/;
            var phone = $('input[name="phone"]').val();
            var address = $('input[type="text"][name="address"]').val();
            var regionId = $('#consignee_county').val();
            var addressdetail = $('input[type="text"][name="addressdetail"]').val();

            if (!shipTo) {
                $.dialog.tips('请填写收货人');
                return false;
            }
            else if ($.trim(shipTo).length == 0) {
                $.dialog.tips('请填写收货人');
                return false;
            }
            else if (!phone) {
                $.dialog.tips('请填写电话');
                return false;
            }
            else if (!regTel.test(phone)) {
                $.dialog.tips('请填写正确的电话');
                return false;
            }
            else //RegionBind.js
                if ($("#NewAddressId").val() <= 0 || $("#NewAddressId").attr("isfinal") != "true") {
                    $.dialog.tips('请填选择所在地区');
                    return false;
                } else if (!address) {
                    $.dialog.tips('请填写详细地址');
                    return false;
                }
                else if ($.trim(address).length == 0) {
                    $.dialog.tips('请填写详细地址');
                    return false;
                }
                else {
                    regionId = $("#NewAddressId").val()
                    var isopenstore = $('#isOpenStore').val();
                    if (isopenstore == 'True' && (!latlng || latlng.length <= 3)) {
                        $.dialog.confirm('未获取到坐标地址，需更新编辑地址，否则无法使用上门自提功能，确定要继续使用此地址？', function () {
                            disabledSelfTake = true;
                            saveAddress(indexId, regionId, shipTo, address, addressdetail, phone, function (result) {
                                var newSelectedText = shipTo + ' &nbsp;&nbsp;&nbsp; ' + phone + ' &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<br />' + $('#areaName .selected-address').text() + ' &nbsp; &nbsp;' + address + '&nbsp;' + addressdetail + '&nbsp;';
                                indexId = isNaN(parseInt(indexId)) ? '' : parseInt(indexId);
                                if (indexId == 0) {
                                    indexId = result.id;
                                    shippingAddress[indexId] = {};
                                }

                                $("#latAndLng").val($("#Latitude").val() + "," + $("#Longitude").val());
                                $('#shippingAddressId').val(indexId).change();
                                $('input[type="hidden"][name="ShippingAddressId"]').val(indexId);
                                $('#addressEditArea').hide();
                                $('#selectedAddress').html(newSelectedText);
                                $('#addressListArea').hide();
                                $('#consignee-list').hide();
                                $('#step-1').removeClass('step-current');
                                $('#shippingAddressRegionId').val(regionId).change();
                                $("#Latitude").val(''); $("#Longitude").val('');//保存成功后将经纬度清空
                                //CalcFreight(indexId);
                            });
                        });
                    } else {
                        disabledSelfTake = false;
                        saveAddress(indexId, regionId, shipTo, address, addressdetail, phone, function (result) {
                            var newSelectedText = shipTo + ' &nbsp;&nbsp;&nbsp; ' + phone + ' &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<br />' + $('#areaName .selected-address').text() + ' &nbsp; &nbsp;' + address + '&nbsp;' + addressdetail + '&nbsp;';
                            indexId = isNaN(parseInt(indexId)) ? '' : parseInt(indexId);
                            if (indexId == 0) {
                                indexId = result.id;
                                shippingAddress[indexId] = {};
                            }

                            $("#latAndLng").val($("#Latitude").val() + "," + $("#Longitude").val());
                            $('#shippingAddressId').val(indexId).change();
                            $('input[type="hidden"][name="ShippingAddressId"]').val(indexId);
                            $('#addressEditArea').hide();
                            $('#selectedAddress').html(newSelectedText);
                            $('#addressListArea').hide();
                            $('#consignee-list').hide();
                            $('#step-1').removeClass('step-current');
                            $('#shippingAddressRegionId').val(regionId).change();
                            $("#Latitude").val(''); $("#Longitude").val('');//保存成功后将经纬度清空
                            //CalcFreight(indexId);
                        });
                    }
                }
        }
        //if (!latlng || latlng.length <= 3) latlng = $("#Latitude").val() + "," + $("#Longitude").val();
        //if (!latlng || latlng.length <= 3) {
        //    $.dialog.confirm('未获取到坐标地址，需更新编辑地址，否则无法使用上门自提功能，确定要继续使用此地址？', function () {
        //        disabledSelfTake = true;
        //        return selectAddress(indexId, latlng);
        //    });
        //} else {
        //    disabledSelfTake = false;
        //    return selectAddress(indexId, latlng);
        //}
    });

    // 重新选择select修改选择地址
    $("#regionSelector").on('change', 'select', function () {
        var selects = $("#regionSelector select"),
            str = '';
        for (var i = 0; i < selects.length; i++) {
            var emStr = selects[i].options[selects[i].options.selectedIndex].text;
            if (emStr == '请选择') {
                str += '';
            } else {
                str += '<em>' + emStr + ' </em> ';
            }
        }
        $('#areaName span').html(str);
        $("#consigneeAddress").val("");//每次选择地址后都要将详细地址清空，防止经纬度和地区不匹配
    });
    if ($('#productType').val() == "0") {
        $("#regionSelector").RegionSelector({
            valueHidden: "#NewAddressId"
        });
    }
});