// JavaScript source code
$(function () {
    var price = $("#range").data("price");
    var lastbuyprice = $("#range").data("lastbuyprice");
    var isexpired = $("#isExpired").val();
    
    $('#btnSave').click(function () {
        var month = $("#range").val();
        if (price == 0) {
            if (parseInt(month) > 6 || parseInt(month) < 0) {
                $.dialog.errorTips("当前以0元购买服务，只能购买1~6个月，且不能叠加购买!");
                return false;
            }
        }
        if (!isexpired && lastbuyprice == 0 && price == 0) {
            $.dialog.errorTips("上次以0元购买服务，未到期前，不能再次购买");
            return false;
        }
        if (month.length > 0 && month <= 12) {
            var totalPrice = month * price;
            $.dialog.confirm('您确定花费' + totalPrice.toFixed(2) + '元购买' + month + '个月限时购服务吗？', function () {
                $('#submit').click();
            });
        }
    })

    $("#range").focus();
    var a = v({
        form: 'form1',
        ajaxSubmit: true,
        beforeSubmit: function () {
            loadingobj = showLoading();
        },
        afterSubmit: function (data) {// 表单提交成功回调
            loadingobj.close();
            if (data.success) {
                $.dialog.succeedTips("提交成功！", function () {
                    window.location.reload();
                }, 0.3);
            } else {
                $.dialog.errorTips(data.msg, '', 3);
            }
        }
    });
    if (price == 0) {
        a.add(
            {
                target: 'range',
                empty: true,
                ruleType: 'uint&&(value>0)&&(value<=6)',// v.js规则验证
                error: '当前以0元购买服务，只能购买1~6个月，且不能叠加购买!'
            });
    }
    else {
        a.add(
            {
                target: 'range',
                empty: true,
                ruleType: 'uint&&(value>0)&&(value<=12)',// v.js规则验证
                error: '只能为数字，且只能是1到12之间的整数!'
            });
    }
});
