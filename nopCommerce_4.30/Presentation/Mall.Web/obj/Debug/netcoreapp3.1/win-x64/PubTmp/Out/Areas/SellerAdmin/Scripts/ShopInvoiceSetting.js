$(function () {
    $('input[type="checkbox"]').onoff();

    if ($('input[name="ProvideInvoice"]').attr('checked') == "checked")
    {
        //$('input.subpluginCheck').attr("checked", true);
        $('.invoiceconfig').show();
    }
    $('input[name="ProvideInvoice"]').change(function () {
        var _this = $(this),
            state = _this[0].checked;
        $('input.subpluginCheck').attr("checked", state);
        if (state) {
            $('.invoiceconfig').show();

        } else {
            $('.invoiceconfig').hide();
        }        
    });

    $('input.subpluginCheck').change(function () {
        var _this = $(this);
        var arr = [];
        $('input.subpluginCheck').each(function (i, e) {
            var state = e.checked
            if (state) {
                arr.push(state);
            }
        });
        if (arr.length <= 0) {
            $('input[name="ProvideInvoice"]').attr("checked", false);
            $('.invoiceconfig').hide();
        }
    });

    $('input[name="PlainInvoiceRate"]').blur(function () {
        validPlain();
    });
    $('input[name="VatInvoiceDay"]').blur(function () {
        validday();
    });
    $('input[name="VatInvoiceDay"]').keyup(function () {
        $(this).val($(this).val().replace(/[^\-?\d]/g, ''));
    });
    $('input[name="VatInvoiceRate"]').blur(function () {
        validvat();
    });
    var $form = $('#form-invoice');
    $form.on('submit', function () {
        var ProvideInvoice = $('input[name="ProvideInvoice"]').attr('checked');
        if (ProvideInvoice) {
            if (!validPlain() || !validday() || !validvat())
                return false;
        }
        var loading = showLoading();
        $form.ajaxSubmit({
            success: function (data) {
                loading.close();
                if (data) {
                    if (data.success == true) {
                        $.dialog.succeedTips('操作成功！');
                        setTimeout(function () {
                            window.location.reload(true);
                            //location.href = '/SellerAdmin/product/management';
                        }, 1500);
                    } else {
                        $.dialog.errorTips(data.message);
                    }
                } else {
                    $.dialog.errorTips('操作失败！');
                }
            },
            error: function (data) {
                loading.close();
                $.dialog.errorTips('操作失败！');
            }
        });

        return false;
        //$.post('./InvoiceSetting', { enable: state }, function (result) {
        //    loading.close();
        //    if (!result.success) {
        //        $.dialog.errorTips('操作失败!失败原因：' + result.msg);
        //    }
        //}, "json");
    });
});

//验证数据
function validPlain() {
    var PlainInvoiceRate = $('input[name="PlainInvoiceRate"]');    

    if(PlainInvoiceRate.val() == "")
    {
        PlainInvoiceRate.parent().addClass('has-error');
        $.dialog.errorTips('请输入普通税率');
        return false;
    } else {
        PlainInvoiceRate.parent().removeClass('has-error');
    }

    if (parseFloat(PlainInvoiceRate.val()) >= 100) {
        PlainInvoiceRate.parent().addClass('has-error');
        $.dialog.errorTips('请输入0-100之间数字');
        return false;
    } else {
        PlainInvoiceRate.parent().removeClass('has-error');
    }
    return true;
}
function validday() {
    var VatInvoiceDay = $('input[name="VatInvoiceDay"]');
    if (VatInvoiceDay.val() == "") {
        VatInvoiceDay.parent().addClass('has-error');
        $.dialog.errorTips('请输入大于等于零的整数');
        return false;
    } else if (parseInt(VatInvoiceDay.val()) < 0) {
        VatInvoiceDay.parent().addClass('has-error');
        $.dialog.errorTips('请输入大于等于零的整数');
        return false;
    }
    VatInvoiceDay.parent().removeClass('has-error');
    return true;
}
function validvat() {
    var VatInvoiceRate = $('input[name="VatInvoiceRate"]');
    if (VatInvoiceRate.val() == "") {
        VatInvoiceRate.parent().addClass('has-error');
        $.dialog.errorTips('请输入增值税税率');
        return false;
    } else {
        VatInvoiceRate.parent().removeClass('has-error');
    }

    if (parseFloat(VatInvoiceRate.val()) >= 100) {
        VatInvoiceRate.parent().addClass('has-error');
        $.dialog.errorTips('请输入0-100之间数字');
        return false;
    } else {
        VatInvoiceRate.parent().removeClass('has-error');
    }
    return true;
}

//截取小数点后两位
function number(obj) {
    obj.value = obj.value.replace(/[^\d.]/g, ""); //清除"数字"和"."以外的字符
    obj.value = obj.value.replace(/^\./g, ""); //验证第一个字符是数字
    obj.value = obj.value.replace(/\.{2,}/g, "."); //只保留第一个, 清除多余的
    obj.value = obj.value.replace(".", "$#$").replace(/\./g, "").replace("$#$", ".");
    obj.value = obj.value.replace(/^(\-)*(\d+)\.(\d\d).*$/, '$1$2.$3'); //只能输入两个小数
}