$(function () {
    $('.progress-').hide();
    var orderIds = $('#orderIds').val();
    $('input[name="requestUrl"]').change(function () {
        var _t = $(this);
        var _nextbtn = $('#nextBtn');
        var url = _t.val();
        if (_t.attr('urlType') != '-1') {
            _nextbtn.removeAttr('href');
            _nextbtn.removeAttr('target');
            url = "/Pay/?pmtid=" + _t.attr("id") + "&ids=" + orderIds;
            _nextbtn.attr('urlType', _t.attr('urlType'));
            _nextbtn.attr('href', url);
            _nextbtn.attr('target', "_blank");
        }
        else {
            _nextbtn.attr('urlType', _t.attr('urlType'));
            _nextbtn.removeAttr('href');
        }
    });

    $('#nextBtn').click(function () {
        var t = $("input[name='requestUrl']:checked").val();
        if (t == undefined) {
            $.dialog.tips('请选择支付方式！');
            return;
        }

        if ($(this).attr('href') != 'javascript:;' || $(this).attr('urlType') == "2") {
            $.dialog({
                title: '登录平台支付',
                lock: true,
                content: '<p>请您在新打开的支付平台页面进行支付，支付完成前请不要关闭该窗口</p>',
                padding: '30px 20px',
                button: [
                {
                    name: '已完成支付',
                    callback: function () {
                        location.href = '/userorder';
                    }
                },
                {
                    name: '支付遇到问题',
                    callback: function () { }
                }]
            });

            if ($(this).attr('urlType') == "2") {
                var url = $(this).attr('formdata');
                BuildPostForm('pay_form', url, '_blank').submit();
            }
        }
    });
    var urltype = $("input[name='requestUrl']:checked").attr("urltype");//返回该页时如果有选中的支付方式则触发一次change事件
    $("input:radio[urltype='" + urltype + "']").change();
    //默认选中第一个
    $(".jdradio").eq(0).trigger("click");
});

function BuildPostForm(fm, url, target) {
    var e = null, el = [];
    if (!fm || !url)
        return e;
    target = target || '_blank';
    e = document.getElementById(fm);
    if (!e) {
        e = document.createElement('Form');
        e.Id = fm;
        document.body.appendChild(e);
    }

    e.method = 'post';
    e.target = target;
    e.style.display = 'none';
    e.enctype = 'application/x-www-form-urlencoded';

    var idx = url.indexOf('?');
    var para = [], op = [];
    if (idx > 0) {
        para = url.substring(idx + 1, url.length).split('&');
        url = url.substr(0, idx);//截取URL
        var keypair = [];
        for (var p = 0 ; p < para.length; p++) {
            idx = para[p].indexOf('=');
            if (idx > 0) {
                el.push('<input type="hidden" name="' + para[p].substr(0, idx) + '" id="frm' + para[p].substr(0, idx) + '" value="' + para[p].substring(idx + 1, para[p].length) + '" />');
            }
        }
    }

    e.innerHTML = el.join('');
    e.action = url;
    return e;
};