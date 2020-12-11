// JavaScript source code
window.onload = function () {
    $("#pCategory option").each(function () {
        $(this).html($(this).text());
    });
}
$(function () {

    $("#subCate").click(function () {

        setTimeout(function () {
            if ($("span.field-validation-error span").length === 0) {
                showLoading();
                return true;
            }
        }, 200);

    });

    $("#Name").focus();
    var depthDOM;
    if ($("#depthHidden").val() != 2) {
        $("#CommisRate").val(100);
        $("#Depth").hide();
        $("#VirtualProduct").hide();
    }
    $("#upload-img").MallUpload({imageDescript:'建议尺寸: 50px * 50px'});

    $("#pCategory").change(function () {
        var _t = $(this);
        if (_t.val() > 0) {
            var loading = showLoading();
            $.ajax({
                type: 'GET',
                url: './GetCateDepth',
                cache: false,
                data: { 'id': $(this).val() },
                dataType: 'json',
                success: function (data) {
                    loading.close();
                    if (data.success && data.depth == 2) {
                        $("#CommisRate").val('');
                        $("#Depth").show();
                        $("#VirtualProduct").show();
                    } else {
                        $("#CommisRate").val(100);
                        $("#Depth").hide();
                        $("#VirtualProduct").hide();
                    }
                },
                error: function () {
                    loading.close();
                    $("#CommisRate").val(100);
                    $("#Depth").hide();
                    $("#VirtualProduct").hide();
                }
            });
        } else {
            $("#CommisRate").val(100);
            $("#Depth").hide();
            $("#VirtualProduct").hide();
        }
    });
    
    if ($("#vb-msg").val() != undefined && $("#vb-msg").val() != "") {
        $.dialog.errorTips($("#vb-msg").val());
    }
    $('#ckbIsSupportVirtualProduct').change(function () {
        var supportVirtualProduct = this.checked == true;
        $("#SupportVirtualProduct").val(supportVirtualProduct);
    });
});