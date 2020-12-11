; (function ($) {
    $(function () {
        var _regurl = "/register";
        try {
            if (regReturnUrl && regReturnUrl.length > 0) {
                _regurl += "?returnUrl=" + regReturnUrl;
            }
        } catch (ex) { _regurl = "/register"; }

        var html = '<div class="alpha hide" id="id_alphaBindMonbie"></div><div class="mimiBox hide" id="id_mimiBoxBindMonbie"><div class="mimiWrap"><div class="mimiTitle"><span>快速绑定手机号</span></div><div class="mimiCon"><h1 style="text-align: center;font-size: 16px;color: red;">为了帐号安全，购买时需要绑定手机号</h1><div class="mimiLog" style="padding: 15px 0 0 60px;"><span class="mimiLogTitleMesg">手机号</span><div class="mimiInput"><input class="mimiTextBindMonbie" type="text" value=""><i class="i-name"></i><label id="mobilenumber_error" class="mimiError hide">您输入的手机号格式不对，请核对后重新输入</label></div><span class="mimiLogTitleMesg" id="verification_title">图形验证码</span><div class="mimiInput" id="verification"><input class="mimiTextBindMonbie w60 fl" type="text" value="" id="verification_inputBindMonbie"><label class="fl" style="line-height: 26px;"><img id="verification_img_bindmobile" class="verification_img_bindmobile" src="/Register/GetCheckCode" alt="" style="cursor:pointer;width:88px;height:33px;display:block;"></label><label class="ftx23 fr" style="line-height: 26px;">&nbsp;<a class="verification_img" href="javascript:;">看不清？换一张</a></label><div class="clear"></div><label id="verification_mobile_error" class="mimiError hide">验证码不正确或验证码已过期</label></div><span class="mimiLogTitleMesg">手机验证码</span><div class="mimiInput"><input class="mimiTextBindMonbie w60 fl" step="width:80px;" type="text" value=""><label class="ftx23 fl" style="line-height: 26px;margin-left:10px;"><a class="verification_mobile" href="javascript:;">获取验证码</a></label><div class="clear"></div><label id="mobilecode_error" class="mimiError hide">手机验证码不正确</label></div><div class="miniTrust"></div><div class="mimiBtn"><input id="bindmobilesubmitframe" class="mimi_btn" type="button" tabindex="8" value="绑 定"></div></div></div><div class="mimiClose mimiBindMonbieClose" style="cursor:pointer;">×</div></div></div>'
        $('body').append(html);
        $('.verification_img_bindmobile,.verification_img').on('click', function () {
            $("#verification_img_bindmobile").attr('src', '/Register/GetCheckCode?' + (+new Date()));
        });
    });
    $.fn.bindmobile = function (selectData, callBack, targetUrl, dataUrl, verifyUrl) {
        var that = $('.mimiTextBindMonbie'),
            btnDom = $('#bindmobilesubmitframe'),
            btnGetSms = $('.verification_mobile'),
            uid = 0,
            response = null,
            delayTime = 120,
            delayFlag = true,
            trigger = function (uid, selectData, callBack, dataUrl, targetUrl) {
                if (!dataUrl || !targetUrl) {
                    callBack.call(null, targetUrl);
                }
                postAjax(selectData, callBack, dataUrl, targetUrl);
            },
            postAjax = function (data, fn, url, elem) {
                if (response) {
                    response.abort();
                };
                response = $.ajax({
                    type: 'post',
                    dataType: 'json',
                    url: url,
                    data: data,
                    success: function (d) {
                        fn.call(d, elem);
                    }
                });
            },
            countDown = function () {
                delayTime--;
                btnGetSms.attr("disabled", "disabled");
                btnGetSms.html(delayTime + '秒后重新获取');
                if (delayTime == 1) {
                    delayTime = 120;
                    btnGetSms.html("获取验证码");
                    $("#mobilecode_error").addClass("hide");
                    btnGetSms.removeClass().removeAttr("disabled");
                    delayFlag = true;
                } else {
                    delayFlag = false;
                    setTimeout(countDown, 1000);
                }
            },
            sendmCode = function (mobile) {//发送手机短信验证码
                if (btnGetSms.attr("disabled") || delayFlag == false) {
                    return;
                }

                btnGetSms.attr("disabled", "disabled");
                var checkCode = $('#verification_inputBindMonbie').val();
                checkCode = $.trim(checkCode);

                postAjax(null, function (elem) {
                    var data = this;
                    if (data.success == true) {
                        $("#mobilecode_error").hide();
                        btnGetSms.html("120秒后重新获取");
                        setTimeout(countDown, 1000);
                        btnGetSms.attr("disabled", "disabled");
                    }
                    else {
                        $("#verification_img_bindmobile").attr('src', '/Register/GetCheckCode?' + (+new Date()));
                        btnGetSms.html("获取短信验证码");
                        $("#mobilecode_error").addClass("hide");
                        btnGetSms.removeAttr("disabled");
                        $('#mobilecode_error').show().html(data.msg);
                    }
                }, '/Register/SendCode?pluginId=Mall.Plugin.Message.SMS&destination=' + mobile + '&imagecheckCode=' + checkCode, this);
            },
            checkMobile = function (mobile) {
                $('#mobilenumber_error').hide();
                if (btnGetSms.attr("disabled")) {
                    return;
                }
                var errorLabel = $('#mobilenumber_error');
                var reg = /^0*(1)\d{10}$/;
                if (!mobile) {
                    $('#mobilenumber_error').show().html('请输入手机号!');
                    return;
                }
                if (!reg.test(mobile)) {
                    $('#mobilenumber_error').show().html('手机号码格式有误，请输入正确的手机号!');
                    return;
                }
            },
            checkCode = function () {// 验证码验证                
                $('#verification_inputBindMonbie').keyup(function () {
                    checkimgCode($(this).val());
                });
            }, checkimgCode = function (ver) {
                var l,
                    ver;
                //ver = $(this).val();
                l = ver.length;
                if (l == 4) {
                    postAjax({ checkCode: ver }, function (elem) {
                        var data = this,
                            str;
                        if (data.result) {
                            uid = 1;
                            $('#verification_mobile_error').hide();
                        } else {
                            uid = 0;
                            str = $('#verification_inputBindMonbie').val();
                            if (str) {
                                $('#verification_mobile_error').show().html('验证码错误!');
                            } else {
                                $('#verification_mobile_error').show().html('验证码不能为空!');
                            }
                        }
                    }, '/Register/CheckCheckCode', this);
                    return false;
                }
            },
            checkTheMobile=function(mobile){
            	postAjax({ mobile: mobile }, function (elem) {
	                var data = this;
	                if (data.result == false) {
	                    $('#mobilenumber_error').hide();
	                    sendmCode(mobile);
	                }
	                else {
	                    $('#mobilenumber_error').html('手机号码 ' + mobile + ' 已经被占用').show();
	                }
	            }, '/Register/CheckMobile', this);
            };
            
        that.each(function (i, e) {
            $(e).bind('focus', function () {
                $(this).addClass('mimiBorder');
                $(this).removeClass('mimiBorder_error').siblings('.mimiError').hide();
            }).bind('blur', function () {
                $(this).removeClass('mimiBorder');
            });
        });
        btnGetSms.unbind('click').bind('click', function () {            
            var arr = [],l;
            that.each(function (i, e) {
                var elem = $(e);
                arr.push(elem.val());
            });            
            var mobile = arr[0];
            checkMobile(mobile);
            l=arr[1].length;
            if (l == 4) {
                postAjax({ checkCode: arr[1] }, function (elem) {
                    var data = this,
                        str;
                    if (data.result) {
                        uid = 1;
                        $('#verification_mobile_error').hide();
                        checkTheMobile(mobile);
                    } else {
                        uid = 0;
                        str = $('#verification_inputBindMonbie').val();
                        if (str) {
                            $('#verification_mobile_error').show().html('验证码错误!');
                        } else {
                            $('#verification_mobile_error').show().html('验证码不能为空!');
                        }
                    }
                }, '/Register/CheckCheckCode', this);
            }else{
            	$('#verification_mobile_error').show().html('验证码错误!');
            }
        });
        btnDom.unbind('click').bind('click', function () {
            var arr = [];
            arr.push("Mall.Plugin.Message.SMS");
            that.each(function (i, e) {
                var elem = $(e);
                arr.push(elem.val());
            });            
            checkMobile(arr[1]);
            checkCode();
            if (!uid) {
                ver = $("#verification_inputBindMonbie").val();
                if (ver.length > 0) {
                    $('#verification_mobile_error').show().html('验证码验证失败!');
                } else {
                    $('#verification_mobile_error').show().html('验证码不能为空!');
                }
                return;
            } else {
                $('#verification_mobile_error').hide();
            }
            if (arr[3] == null || arr[3].length <= 0) {
                $('#mobilecode_error').show().html("请输入手机验证码!");
                return false;
            }

            arr = 'pluginId=' + arr[0] + '&code=' + arr[3] + '&destination=' + arr[1];// 组成需要post的数据
            postAjax(arr, function (elemList) {
                var data = this,
                    list = elemList;
                if (data.success) {
                    uid = 1;
                } else {
                    uid = 0;
                    $('#mobilecode_error').show().html(data.msg);
                    $("#verification_img_bindmobile").attr('src', '/Register/GetCheckCode?' + (+new Date()));
                }                
                if (uid) {
                    $('#id_alphaBindMonbie,#id_mimiBoxBindMonbie').hide();
                    trigger(uid, selectData, callBack, dataUrl, targetUrl);
                }
                return false;
            }, verifyUrl, that);
        });
        $('.mimiBindMonbieClose').bind('click', function () {
            $('#id_alphaBindMonbie,#id_mimiBoxBindMonbie').hide();
        });
        $('#id_alphaBindMonbie,#id_mimiBoxBindMonbie').show();
    };
}(jQuery));
/*
$.fn.login({用户勾选的商品数据},function(url){
	//跳转方法函数
	var data=this;
	if(data.state){
		window.location.href=url; 
	}
},'跳转的url','数据url','登陆验证url');
*/