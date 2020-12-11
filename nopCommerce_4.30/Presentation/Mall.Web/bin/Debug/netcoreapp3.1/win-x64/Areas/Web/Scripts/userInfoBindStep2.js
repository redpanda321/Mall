var count = 120;
$('#btnAuthCode').click(function () {
    if (check() || !checkCheckCodeIsValid()) {
        return;
    }
    var destination = $("#destination").val();
    var id = $('#pluginId').val();
    $.post('SendCodeStep2', { pluginId: id, destination: destination, checkBind: true }, function (result) {
        if (result.success) {
            setTimeout(countDown1('timeDiv1', ''), 1000);
        }
        else {
            $.dialog.errorTips('发送验证码失败：' + result.msg);
        }
    });
});

$('#id_btn').click(function () {
    var destination = $("#destination").val();
    var id = $('#pluginId').val();
    var code = $('#code').val();
    if (check() || !checkCheckCodeIsValid()) {
        return;
    }
    $.post('/userInfo/CheckCodeStep2', { pluginId: id, code: code, destination: destination }, function (result) {
        if (result.success) {
            $.dialog.succeedTips('验证成功！', function () { window.location.href = '/Userinfo/rebindstep3?name=' + ($('#name').val()); });

        }
        else {
            $.dialog.errorTips(result.msg);
            reloadImg();//刷新图形验证码
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
function check() {
    var reg1 = /^[1-9]\d{10}$/;
    //  reg2 = /^\w+([-+.]\w+)*@@\w+([-.]\w+)*\.\w+([-.]\w+)*$/, 萧萧下的毒
    var reg2 = /^(\w)+(\.\w+)*@(\w)+((\.\w+)+)$/;
    str = $('#destination').val();
    var a = reg1.test(str),
        b = reg2.test(str);
    if (a || b) {
        $('#msg').hide();
        return false;
    } else {
        $('#msg').html('<div style="color:#e4393c">请填写正确的信息!</div>');
        return true;
    }
}


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