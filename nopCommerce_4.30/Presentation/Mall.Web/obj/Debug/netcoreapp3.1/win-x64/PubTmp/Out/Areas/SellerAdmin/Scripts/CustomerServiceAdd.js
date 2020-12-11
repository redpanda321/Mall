// JavaScript source code
$(function () {
    $("#Name").focus();
    $('#Tool option[value=3]').remove();//直接通过JS隐藏美洽客服项[39728]
});
function bindSubmitClickEvent() {
    var form = $('form');
    $('#submit').click(function () {
        if (form.valid()) {
            var loading = showLoading();
            $.post('add', form.serialize(), function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.tips('保存成功', function () {
                        location.href = $("#MCurl").val();
                    });
                }
                else
                    $.dialog.errorTips('保存失败!' + result.msg);
            })
        }
    });
}


$(function () {
    bindSubmitClickEvent();


})
