// JavaScript source code
$(function () {
    $('#Save').click(function () {
        var min = parseInt($('#WithDrawMinimum').val()) || 0;
        var max = parseInt($('#WithDrawMaximum').val()) || 0;
        if (min < 1 || max > 1000000) {
            $.dialog.alert("金额范围只能是(1-1000000)");
            return;
        }
        var alipayEnable = $('#Withdraw_AlipayEnable').is(":checked");
        var isOpenWithdraw = $('#IsOpenWithdraw').is(":checked");
        var loading = showLoading();
        $.post('./SaveWithDrawSetting', { minimum: $('#WithDrawMinimum').val(), maximum: $('#WithDrawMaximum').val(), alipayEnable: alipayEnable, isOpenWithdraw: isOpenWithdraw }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('保存成功');
            }
            else
                $.dialog.errorTips('保存失败！' + result.msg);
        });
    });
})