// JavaScript source code
$(function () {
    if (typeof ($("input[name='Main_Mch_ID']").val()) != "undefined") {
        if ($("input[name='Main_Mch_ID']").val().length > 0 && $("input[name='Main_AppId']").val().length > 0) {
            $("input[name='Main_Mch_ID']").attr("formitem", "");
            $("input[name='Main_AppId']").attr("formitem", "");
            $("#ckbServices").attr("checked", 'checked');
            $(".j_service").show();
        } else {
            $("#ckbServices").attr("checked", false);
            $("input[name='Main_Mch_ID']").removeAttr("formitem");
            $("input[name='Main_AppId']").removeAttr("formitem");
            $(".j_service").hide();
        }
    }
    $('#btn').click(function () {
        var items = $('input[formItem]');
        var data = [];
        $.each(items, function (i, item) {
            data.push({ key: $(item).attr('name'), value: $(item).val() });
        });
        var dataString = JSON.stringify(data);
        var id = $('#pluginId').val();
        var loading = showLoading();
        $.post('save', { pluginId: id, values: dataString }, function (result) {
            loading.close();
            if (result.success)
                $.dialog.tips('保存成功！', function () { location.href = "Management"; });
            else
                $.dialog.errorTips('保存失败！' + result.msg);
        });
    });
    $('#ckbServices').change(function () {
        $('#ckbServices').toggleClass('hidden');
        if (this.checked == true) {
            $(".j_service").show();
            //如果服务商模式开启
            $("input[name='Main_Mch_ID']").attr("formitem", "");
            $("input[name='Main_AppId']").attr("formitem", "");
        }
        else {
            $(".j_service").hide();
            $("input[name='Main_Mch_ID']").val('');
            $("input[name='Main_AppId']").val('');
            $("input[name='Main_Mch_ID']").removeAttr("formitem");
            $("input[name='Main_AppId']").removeAttr("formitem");
        }
    });
});