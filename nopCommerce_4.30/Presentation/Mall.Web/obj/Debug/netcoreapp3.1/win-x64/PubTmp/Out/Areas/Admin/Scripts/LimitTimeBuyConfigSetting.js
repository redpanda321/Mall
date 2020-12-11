// JavaScript source code
$(function () {
    $("#submit").click(function () {
        loading = showLoading();
        var isNormalPurchase = $("input[name=IsNormalPurchase]:checked").val() == "1";
        var isneedaudit = $("input[name=isneedaudit]:checked").val() == "1";
        $.post("./SetConfig", { Preheat: $("#Preheat").val(), IsNormalPurchase: isNormalPurchase, isneedaudit: isneedaudit }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.succeedTips("保存成功！");
            }
        })
    })
})