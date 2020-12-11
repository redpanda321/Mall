// JavaScript source code
$(function () {
    $('#Save').click(function () {
        var min = parseInt($('#ShopWithDrawMinimum').val()) || 0;
        var max = parseInt($('#ShopWithDrawMaximum').val()) || 0;
        if (min < 1 || max > 1000000) {
            $.dialog.alert("金额范围只能是(1-1000000)");
            return;
        }
        var loading = showLoading();
        $.post('./SaveWithDrawSetting', { minimum: min, maximum: max }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('保存成功');
            }
            else
                $.dialog.errorTips('保存失败！' + result.msg);
        });
    });
})