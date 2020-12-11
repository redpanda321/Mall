var count = 120;
$('#btnAuthCode').click(function () {
    if (!checkCheckCodeIsValid())
        return false;

    var destination = $("#destination").text();
    var id = $('#pluginId').val();
    $('#btnAuthCode').attr("disabled", true);
    $.post('/UserInfo/SendCode', { pluginId: id, destination: destination }, function (result) {
        if (result.success) {
            setTimeout(countDown1('timeDiv1', ''), 1000);
        }
        else {
            $.dialog.errorTips('发送验证码失败：' + result.msg);
        }
    });
});
function countDown1() {
    $("#btnAuthCode").parent().parent().hide();
    $("#msg").show().html('验证码已发送还剩下<font color="#f60">' + count + '</font>秒');
    if (count == 1) {
        $("#msg").hide();
        $("#btnAuthCode").parent().parent().show().removeAttr("disabled");
        count = 120;
        return;
    } else {
        setTimeout(countDown1, 1000);
    }
    count--;
}
$('#id_btn').click(function () {
    if (!checkCheckCodeIsValid()) {
        return false;
    }
    var destination = $("#destination").text();
    var id = $('#pluginId').val();
    var code = $('#code').val();
    $.post('/UserInfo/CheckCode', { pluginId: id, code: code, destination: destination }, function (result) {
        if (result.success) {
            $.dialog.succeedTips('验证成功！', function () { location.href = "/UserInfo/ReBindStep2?pluginId=" + id + "&key=" + result.key; });

        }
        else {
            $.dialog.errorTips(result.msg);
            reloadImg();//刷新图形验证码
        }
    });
});

//图形验证验证
function checkCheckCodeIsValid() {
    var checkCode = $('#checkCode').val();
    //var errorLabel = $('#checkCode_error');
    checkCode = $.trim(checkCode);

    var result = false;
    if (checkCode) {
        $.ajax({
            type: "post",
            url: "/register/CheckCheckCode",
            data: { checkCode: checkCode },
            dataType: "json",
            async: false,
            success: function (data) {
                if (data.success) {
                    if (data.result) {
                        //errorLabel.hide();
                        result = true;
                    }
                    else {
                        //errorLabel.html('图形验证码错误').show();
                        $.dialog.errorTips("图形验证码错误");
                        reloadImg();//刷新图形验证码
                    }
                }
                else {
                    //$.dialog.errorTips("图形验证码校验出错", '', 1);
                    $.dialog.errorTips("图形验证码校验出错");
                }
            }
        });
    } else {
        //errorLabel.html('请输入验证码').show();
        $.dialog.errorTips("请输入图形验证码");
    }
    return result;
}

function reloadImg() {
    $("#checkCodeImg").attr("src", "/Register/GetCheckCode?_t=" + Math.round(Math.random() * 10000));
}