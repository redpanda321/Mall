$(function () {
    $('#btn').click(function () {
        var items = $('input[formItem]');
        var data = [];
        $.each(items, function (i, item) {
            data.push({ key: $(item).attr('name'), value: $(item).val() });
        });
        var dataString = JSON.stringify(data);
        var loading = showLoading();
        var appId, appSecret;
        
            appId = $('input[name="WeixinAppletId"]').val();
            appSecret = $('input[name="WeixinAppletSecret"]').val();
        
         
        $.post('save', { values: dataString, weixinAppletId: appId, WeixinAppletSecret: appSecret }, function (result) {
            if (result.success)
                $.dialog.succeedTips('保存成功！', function () { });
            else
                $.dialog.errorTips('保存失败！' + result.msg);
            loading.close();
        });
    });
});