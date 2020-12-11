$(function () {
    bindSubmitBtn();
    bindCheckCode();
    initUsenameBox();
});

function bindSubmitBtn() {
    $('#loginsubmit').click(function () {
        submit();
    });

    document.onkeydown = function () {
        if (event.keyCode == 13) {
            submit();
        }
    }
}

function initUsenameBox() {
    var defaultUsername = $.cookie('Mall-DefaultUserName');
    if (defaultUsername) {
        $('#loginname').val(defaultUsername);
        $(".user-name").css("display","none")
        $('#password').focus();
    };
    if( $('#loginname').val()==""){
        $(".user-name").css("display","block")
        }
}

function submit() {
    var result = checkUsername() && checkPassword() && checkCheckCode();
    var IsOpenPC = $("#IsOpenPC").val();
    if (result) {
        var username = $('#loginname').val();
        var password = $('#password').val();
        var checkCode = $('#checkCodeBox').val();
        var keep = $('#autoLogin').attr('checked');
        keep = keep ? true : false;

        var loading = showLoading();
        $.post('/Login/Login', { username: username, password: password, checkCode: checkCode, keep: keep }, function (data) {
            loading.close();
            if (data.success) {//登录成功
                $.cookie('Mall-DefaultUserName', username, { path: "/", expires: 365 });
                if (IsOpenPC == 0) {
                    location.href = "/sellerAdmin";
                } else {
                    if (data.isMemberDisabled == true) { //特种情况，优先处理（产品需求）
                        $.dialog.tips('检测到会员身份被冻结，同时您又是卖家身份,即将跳转到卖家中心', function () {
                            location.href = "/sellerAdmin";
                        }, 2);
                        return;
                    }
                    var returnUrl = decodeURIComponent(QueryString('returnUrl')).replace('&amp;', '&');
                    if (returnUrl)
                        location.href = returnUrl;
                    else if (data.isChildSeller == true) {
                        location.href = "/sellerAdmin";
                    }
                    else {
                        location.href = '/'; //跳转至买家中心
                    }
                }
            }
            else {
                var isFirstShowCheckcode = false;
                refreshCheckCode();
                if (data.errorTimes > data.minTimesWithoutCheckCode) {//需要验证码
                    if ($('#checkCodeArea').css('display') == 'none') {
                        isFirstShowCheckcode = true;
                        $('#checkCode_error').html(data.msg).show();
                    }

                    $('#checkCodeArea').show();
                    $('#autoentry').css('margin-top', 0);
                }
                else {
                    $('#checkCodeArea').hide();
                    $('#autoentry').removeAttr('style');
                }
                if (!isFirstShowCheckcode) {
                    $('#loginpwd_error').html(data.msg).show();
                    $('#password').focus();
                }
                else
                    $('#checkCodeBox').focus();

            }
        });
    }
}

function checkCheckCode() {
    var result = false;
    if ($('#checkCodeArea').css('display') == 'none')
        result = true;
    else {
        var checkCode = $('#checkCodeBox').val();
        var errorLabel = $('#checkCode_error');
        if (checkCode && checkCode.length == 4) {
            $.ajax({
                type: "post",
                url: "/login/checkCode",
                data: { checkCode: checkCode },
                dataType: "json",
                async: false,
                success: function (data) {
                    if (data.success) {
                        result = true;
                        errorLabel.hide();
                    }
                    else {
                        $('#checkCodeBox').focus();
                        errorLabel.html('验证码错误').show();
                    }
                }
            });
        }
        else {
            $('#checkCodeBox').focus();
            if (!checkCode)
                errorLabel.html('请填写验证码').show();
            else
                errorLabel.html('验证码错误').show();
        }
    }
    return result;
}

function checkUsername() {
    var result = false;
    var username = $('#loginname').val();
    var loginError = $('#loginname_error');
    if (!username) {
        loginError.html('请输入用户名').show();
    }
    else {
        result = true;
        loginError.hide();
    }
    return result;
}

function checkPassword() {
    var result = false;
    var password = $('#password').val();
    var passwordError = $('#loginpwd_error');
    if (!password) {
        passwordError.html('请输入密码').show();
    }
    else {
        result = true;
        passwordError.hide();
    }
    return result;
};

function refreshCheckCode() {
    var path = $('#checkCodeImg').attr('src').split('?')[0];
    path += '?time=' + new Date().getTime();
    $('#checkCodeImg').attr('src', path);
    $('#checkCodeBox').val('');
}

function bindCheckCode() {
    $('#checkCodeImg,#checkCodeChange').click(function () {
        refreshCheckCode();
    });
}


function bindFocus() {
    $('#password').keydown(function () {
        $('#loginpwd_error').hide();
    });

    $('#loginname').keydown(function () {
        $('#loginpwd_error').hide();
    });

    $('#checkCodeBox').keydown(function () {
        $('#checkCode_error').hide();
    });

}

