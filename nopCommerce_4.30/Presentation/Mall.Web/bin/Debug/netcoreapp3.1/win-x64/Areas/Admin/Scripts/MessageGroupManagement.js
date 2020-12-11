// JavaScript source code
$(function () {

    $(".start_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });
    $(".end_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });
    $('.start_datetime').on('changeDate', function () {
        if ($(".end_datetime").val()) {
            if ($(".start_datetime").val() > $(".end_datetime").val()) {
                $('.end_datetime').val($(".start_datetime").val());
            }
        }

        $('.end_datetime').datetimepicker('setStartDate', $(".start_datetime").val());
    });

    $('.start_datetime,.end_datetime').keydown(function (e) {
        e = e || window.event;
        var k = e.keyCode || e.which;
        if (k != 8 && k != 46) {
            return false;
        }
    });

    $("#list").MallDatagrid({
        url: "./GetSendRecords",
        singleSelect: true,
        pagination: true,
        NoDataMsg: '没有找到符合条件的数据',
        idField: "Id",
        pageSize: 15,
        pageNumber: 1,
        queryParams: {},
        columns:
        [[
            { field: "Id", hidden: true },
            { field: "MsgType", title: "类型", width: 200, },
            {
                field: "UseRate", title: "整体使用率", width: 200, formatter: function (value, row, index) {
                    if (row.CurrentCouponCount > 0 && row.MsgType == "优惠券") {
                        var rate = row.CurrentUseCouponCount / row.CurrentCouponCount;
                        rate = rate * 100;
                        return rate.toFixed(2) + "%";
                    }
                    else if (row.CurrentCouponCount == 0 && row.MsgType == "优惠券") {
                        return 0 + "%";
                    } else if (row.MsgType != "优惠券") {
                        return "";
                    }
                }
            },
            { field: "SendTime", title: "发送时间", width: 200 },
            { field: "SendToUser", title: "发送对象", width: 120 },
            { field: "SendState", title: "发送状态", width: 120 },
            {
                field: "operation", operation: true, title: "操作", width: 100,
                formatter: function (value, row, index) {
                    var html = ["<span class=\"btn-a\">"];
                    html.push("<a class=\"good-check\" onclick=\"OpenMessageGroup('" + index + "')\">查看内容</a>");
                    html.push("</span>");
                    return html.join("");
                }
            }
        ]]
    });
    $('#searchBtn').click(function (e) {
        searchClose(e);
        var startDate = $("#inputStartDate").val();
        var endDate = $("#inputEndDate").val();
        var messageType = $.trim($('#MessageType').val());
        var sendState = $.trim($('#SendState').val());
        $("#list").MallDatagrid('reload', {
            startDate: startDate, endDate: endDate,
            msgType: messageType, sendState: sendState
        });
    })
});

//弹窗
function OpenMessageGroup(rowIndex) {
    var data = $("#list").MallDatagrid('getRowByIndex', rowIndex);

    loading = showLoading('加载中');
    $.post('./GetSendRecordDetail', { id: data.Id }, function (result) {
        loading.close();
        //alert(JSON.stringify(result));
        if (result.success) {
            var strContent = "";
            result.model.ToUserLabel = (result.model.ToUserLabel != null && result.model.ToUserLabel != "") ? result.model.ToUserLabel : "全部";

            switch (result.model.MessageType) {
                case 0://微信
                    //alert("a");
                    strContent = GetContentWeiXin(result.model);
                    break;
                case 1://邮件
                    //alert("b");
                    strContent = GetContentEmail(result.model);
                    break;
                case 2://优惠券
                    //alert("c");
                    strContent = GetContentCoupon(result.model);
                    break;
                case 3://短信
                    //alert("d");
                    strContent = GetContentSMS(result.model);
                    break;
            }

            $.dialog({
                title: '查看内容',
                lock: true,
                id: 'goodCheck',
                width: '466px',
                content: strContent.join(''),
                padding: '0 40px',
                okVal: '确定',
                ok: function () {
                }
            });
        }
        else {
            $.dialog.alert(result.msg);
        }
    }, "json");

}

//微信
function GetContentWeiXin(data) {
    //alert(JSON.stringify(data));
    dlgcontent = ['<div class="formbox">',
        '<div class="mb">',
        '<label>群发对象：</label>',
        '<p>' + data.ToUserObject + '</p>',
        '</div>'];
    
    if (data.ToUserSex != null && data.ToUserSex.length > 0) {
        dlgcontent = dlgcontent.concat(['<div class="mb">',
            '<label>性别：</label>',
            '<p>' + data.ToUserSex + '</p>',
            ' </div>']);
    }

    if (data.ToUserRegion != null && data.ToUserRegion.length > 0) {
        dlgcontent = dlgcontent.concat(['<div class="mb">',
            '<label>地区：</label>',
            '<p>' + data.ToUserRegion + '</p>',
            ' </div>']);
    }

    var strContent = data.SendContent;
    if (data.SendWXMediaId != null && data.SendWXMediaId.length > 0) {
        strContent = "<a href=\"/Admin/Weixin/WXMsgTemplate?mediaid=" + data.SendWXMediaId + "\" target=\"_blank\">" + strContent + "</a>";
    }
    dlgcontent = dlgcontent.concat(['<div class="mb">',
        '<label>内容：</label>',
        '<p>' + strContent + '</p>',
        ' </div>']);

    dlgcontent = dlgcontent.concat(['</div>']);

    return dlgcontent;
}

//邮件
function GetContentEmail(data) {
    //alert(JSON.stringify(data));
    dlgcontent = ['<div class="formbox">',
        '<div class="mb">',
        '<label>群发对象：</label>',
        '<p>' + data.ToUserLabel.replace("标签：", "").replace("标签:", "") + '</p>',
        '</div>'];

    dlgcontent = dlgcontent.concat(['<div class="mb">',
        '<label>邮件标题：</label>',
        '<p>' + (data.SendEmailTitle == null ? "" : data.SendEmailTitle) + '</p>',
        ' </div>']);

    dlgcontent = dlgcontent.concat(['<div class="mb">',
        '<label>邮件正文：</label>',
        '<p>' + data.SendContent + '</p>',
        ' </div>']);

    dlgcontent = dlgcontent.concat(['</div>']);

    return dlgcontent;
}

//优惠券
function GetContentCoupon(data) {
    //alert(JSON.stringify(data));
    dlgcontent = ['<div class="formbox">',
        '<div class="mb">',
        '<label>群发对象：</label>',
        '<p>' + data.ToUserLabel.replace("标签：", "").replace("标签:", "") + '</p>',
        '</div>'];
    
    if (data.CouponList != null && data.CouponList.length > 0) {
        for (var i = 0; i < data.CouponList.length; i++) {
            var item = data.CouponList[i];
            var divclass = (i != 0 ? " mbtp" : "");
            dlgcontent = dlgcontent.concat(['<div class="mb' + divclass + '">',
                '<label>优惠券名称：</label>',
            '<p>' + item.CouponName + '</p>',
                ' </div>']);

            dlgcontent = dlgcontent.concat(['<div class="mb">',
                '<label>商家：</label>',
                '<p>' + item.ShopName + '</p>',
                ' </div>']);

            dlgcontent = dlgcontent.concat(['<div class="mb">',
                '<label>面额：</label>',
                '<p>' + item.Price + '</p>',
                ' </div>']);

            dlgcontent = dlgcontent.concat(['<div class="mb">',
                '<label>使用条件：</label>',
                '<p>' + (item.OrderAmount == 0 ? "不限制" : "满" + item.OrderAmount + "使用") + '</p>',
                ' </div>']);

            dlgcontent = dlgcontent.concat(['<div class="mb">',
                '<label>有效期：</label>',
                '<p>' + item.strEndTime + "~" + item.strEndTime + '</p>',
                ' </div>']);
        }
    } else {
        dlgcontent = dlgcontent.concat(['<div class="mb">',
            '<label></label>',
            '<p>没有优惠券记录</p>',
            ' </div>']);
    }

    dlgcontent = dlgcontent.concat(['</div>']);

    return dlgcontent;
}

//短信
function GetContentSMS(data) {
    //alert(JSON.stringify(data));
    dlgcontent = ['<div class="formbox">',
        '<div class="mb">',
        '<label>群发对象：</label>',
        '<p>' + data.ToUserLabel.replace("标签：", "").replace("标签:", "") + '</p>',
        '</div>'];

    dlgcontent = dlgcontent.concat(['<div class="mb">',
        '<label>短信正文：</label>',
        '<p>' + data.SendContent + '</p>',
        ' </div>']);
    

    dlgcontent = dlgcontent.concat(['</div>']);

    return dlgcontent;
}

