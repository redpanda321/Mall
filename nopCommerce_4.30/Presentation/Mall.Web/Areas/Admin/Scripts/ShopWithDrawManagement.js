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

    $(".start_datetimes").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });
    $(".end_datetimes").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    });

    $('.start_datetimes').on('changeDate', function () {
        if ($(".end_datetimes").val()) {
            if ($(".start_datetimes").val() > $(".end_datetimes").val()) {
                $('.end_datetimes').val($(".start_datetimes").val());
            }
        }

        $('.end_datetimes').datetimepicker('setStartDate', $(".start_datetimes").val());
    });

    getPage(status);
    $("#dh_" + status).parent().attr("class", "active");

    //搜索
    $('#searchButton').click(function (e) {
        searchClose(e);
        var applyStartTime = $("#inputStartDate").val();
        var applyEndTime = $("#inputEndDate").val();
        var auditedStartTime = $("#inputStartDates").val();
        var auditedEndTime = $("#inputEndDates").val();
        var shopName = $("#shopName").val();

        $("#list").MallDatagrid('reload', { applyStartTime: applyStartTime, applyEndTime: applyEndTime, auditedStartTime: auditedStartTime, auditedEndTime: auditedEndTime, shopName: shopName });
    });
});


function getPage(status) {
    if (status == 1 || status == 4 || status == 5) {
        //提现列表,待处理
        $("#list").MallDatagrid({
            url: 'List',
            nowrap: false,
            rownumbers: true,
            NoDataMsg: '没有找到符合条件的数据',
            border: false,
            fit: true,
            fitColumns: true,
            pagination: true,
            idField: "Id",
            pageSize: 16,
            pagePosition: 'bottom',
            pageNumber: 1,
            queryParams: { Status: status },
            operationButtons: "#batchOperate",
            columns:
            [[
                {
                    checkbox: true, width: 39, formatter: function (value, row, index) {
                        var html = "<input type=\"checkbox\" ";
                        if (row['WithStatus'] != 1 && row['WithStatus'] != 4) {
                            html += ' disabled ';
                        }
                        html += ">";
                        return html;
                    }
                },
                { field: "ApplyTime", title: "申请时间", width: 140, sort: true },
                { field: "ShopName", title: "商家", width: 140 },
                { field: "CashAmount", title: "申请金额", width: 130,sort:true },
                { field: "CashType", title: "提现方式", width: 130 },
                { field: "Account", title: "账户", width: 130 },
                { field: "AccountName", title: "收款账户姓名", width: 140 },
                {
                    field: "Id", operation: true, title: "操作",
                    formatter: function (value, row, index) {
                        var id = row.Id.toString();
                        var WithStatus = parseFloat(row.WithStatus.toString());

                        var html = [""];
                        switch (WithStatus) {
                            case 1:
                                html.push("<span class=\"btn-a\">");
                                html.push("<a onclick='DoOperate(" + id + ",0,\"\",\"" + row.CashType + "\")'>审核</a>");
                                html.push("</span>");
                                break;
                            case 4:
                                html.push("<span class=\"btn-a\">");
                                html.push("<a onclick='DoOperate(" + id + ",0,\"\",\""+row.CashType+"\")'>重新付款</a>");
                                html.push("</span>");
                                break;
                            case 5:
                                html.push("<span class=\"btn-a\">");
                                html.push("<a onclick='CancelPay(" + id + ")'>取消</a>");
                                html.push("</span>");
                                break;
                        }

                        return html.join('');
                    }
                }
            ]]
        });
    } else if (status == 2) {
        //提现列表,拒绝
        $("#list").MallDatagrid({
            url: 'List',
            nowrap: false,
            rownumbers: true,
            NoDataMsg: '没有找到符合条件的数据',
            border: false,
            fit: true,
            fitColumns: true,
            pagination: true,
            idField: "id",
            pageSize: 16,
            pagePosition: 'bottom',
            pageNumber: 1,
            queryParams: { Status: 2 },
            columns:
            [[
                { field: "ApplyTime", title: "申请时间", width: 140, sort: true },
                { field: "DealTime", title: "拒绝时间", width: 140 },
                { field: "ShopName", title: "商家", width: 140 },
                { field: "CashAmount", title: "申请金额", width: 130, sort: true },
                { field: "CashType", title: "提现方式", width: 130 },
                { field: "Account", title: "账户", width: 130 },
                { field: "AccountName", title: "收款账户姓名", width: 140 },
                { field: "PlatRemark", title: "备注", width: 140 }
            ]]
        });
    } else if (status == 3) {
        $(".Deal").show();
        //提现列表,成功
        $("#list").MallDatagrid({
            url: 'List',
            nowrap: false,
            rownumbers: true,
            NoDataMsg: '没有找到符合条件的数据',
            border: false,
            fit: true,
            fitColumns: true,
            pagination: true,
            idField: "id",
            pageSize: 16,
            pagePosition: 'bottom',
            pageNumber: 1,
            queryParams: { Status: 3 },
            operationButtons: "#batchOperate",
            columns:
            [[
                { field: "DealTime", title: "审核时间", width: 140, sort: true },
                { field: "ApplyTime", title: "申请时间", width: 140, sort: true },
                { field: "ShopName", title: "商家", width: 140 },
                { field: "CashAmount", title: "申请金额", width: 130, sort: true },
                { field: "CashType", title: "提现方式", width: 130 },
                { field: "Account", title: "账户", width: 130 },
                { field: "AccountName", title: "收款账户姓名", width: 140 },
                { field: "AccountNo", title: "交易流水号", width: 140 },
                { field: "PlatRemark", title: "备注", width: 140 }
            ]]
        });
    }
}

function DoOperate(id, action, msg,cashType) {
    if (action == 1) {
        $.dialog({
            title: '查看原因',
            lock: true,
            id: 'showRemrk',
            content: ['<div class="dialog-form">',
                '<div class="form-group">',
                    '<p class="help-esp">备注</p>',
                    '<textarea id="auditMsgBox" class="form-control" cols="61" rows="2"  >' + msg + '</textarea>\
                 <p id="valid" style="visibility:hidden;color:red;line-height:18px;">请填写未通过理由</p><p id="validateLength" style="visibility:hidden;color:red;line-height:18px;padding:0;">备注在40字符以内</p> ',
                '</div>',
            '</div>'].join(''),
            padding: '0 40px',
            init: function () { $("#auditMsgBox").focus(); },
            button: [
            {
                name: '关闭',
                callback: function () { },
                focus: true
            }
            ]
        })
    }
    else {

        var weiTitle = "处理提现到微信钱包申请";
        var bankTitle = "处理提现到银行申请";

        var bankMsg = "商家提现的账户为银行账户，需要人工给商家转账，是否已确认完成转账？";
        var WeiMsg = "商家提现的账户为微信账户，确认后将自动转账到商家提现账户，是否确认转账？";
        var confrimMsg = bankMsg;
        var title = bankTitle;
        if (cashType == "微信")
        {
            confrimMsg = WeiMsg;
            title = weiTitle;
        } else if (cashType == "支付宝") {
            confrimMsg = "商家提现的账户为支付宝账户，系统会跳转支付宝完成支付流程,是否确认转账？";
            title = "处理提现到支付宝申请";
        }
        else
        {
            confrimMsg = bankMsg;
            title = bankTitle;
        }
      

        $.dialog({
            title: title,
            lock: true,
            id: 'goodCheck',
            content: ['<div class="dialog-form">',
                 '<div class="form-group">',
                   '<p class="help-esp">' + confrimMsg + '</p>',
                 '</div>',
                '<div class="form-group">',
                    '<p class="help-esp">备注</p>',
                    '<textarea id="auditMsgBox" class="form-control" cols="61" rows="2"  >' + msg + '</textarea>\
                 <p id="valid" style="visibility:hidden;color:red;line-height:18px;">请填写未通过理由</p><p id="validateLength" style="visibility:hidden;color:red;line-height:18px;padding:0;">备注在40字符以内</p> ',
                '</div>',
            '</div>'].join(''),
            padding: '0 40px',
            init: function () { $("#auditMsgBox").focus(); },
            button: [
            {
                name: '付款',
                callback: function () {
                    if ($("#auditMsgBox").val().length > 40) {
                        $('#validateLength').css('visibility', 'visible');
                        return false;
                    }
                    ConfirmPay(id, 3, $("#auditMsgBox").val());
                },
                focus: true
            },
            {
                name: '拒绝',
                callback: function () {
                    if (!$.trim($('#auditMsgBox').val())) {
                        $('#valid').css('visibility', 'visible');
                        return false;
                    }
                    else if ($("#auditMsgBox").val().length > 40) {
                        $('#validateLength').css('visibility', 'visible');
                        return false;
                    }
                    else {
                        $('#valid').css('visibility', 'hidden');
                        ConfirmPay(id, 2, $("#auditMsgBox").val());
                    }
                }
            }]
        });
    }
}
function BatchDoOperate() {
    var selectedRows = $("#list").MallDatagrid("getSelections");
    var selectids = new Array();

    for (var i = 0; i < selectedRows.length; i++) {
        selectids.push(selectedRows[i].Id);
    }
    if (selectedRows.length == 0) {
        $.dialog.errorTips("你没有选择任何选项！");
    }
    else {
        
        var confrimMsg = "商家提现的账户为微信账户、支付宝账户，确认后将自动转账到商家提现账户，是否确认转账？";
        $.dialog({
            title: '商家提现批量审核',
            lock: true,
            id: 'goodCheck',
            content: ['<div class="dialog-form">',
                '<div class="form-group">',
                '<p class="help-esp">' + confrimMsg + '</p>',
                '</div>',
                '<div class="form-group">',
                '<p class="help-esp">备注</p>',
                '<textarea id="auditMsgBox" class="form-control" cols="61" rows="2"  ></textarea>\
                 <p id="valid" style="visibility:hidden;color:red;line-height:18px;">请填写未通过理由</p><p id="validateLength" style="visibility:hidden;color:red;line-height:18px;padding:0;">备注在40字符以内</p> ',
                '</div>',
                '</div>'].join(''),
            padding: '0 40px',
            init: function () { $("#auditMsgBox").focus(); },
            button: [
                {
                    name: '付款',
                    callback: function () {
                        if ($("#auditMsgBox").val().length > 40) {
                            $('#validateLength').css('visibility', 'visible');
                            return false;
                        }
                        BatchConfirmPay(selectids.join(','), 3, $("#auditMsgBox").val());
                    },
                    focus: true
                },
                {
                    name: '拒绝',
                    callback: function () {
                        if (!$.trim($('#auditMsgBox').val())) {
                            $('#valid').css('visibility', 'visible');
                            return false;
                        }
                        else if ($("#auditMsgBox").val().length > 40) {
                            $('#validateLength').css('visibility', 'visible');
                            return false;
                        }
                        else {
                            $('#valid').css('visibility', 'hidden');
                            BatchConfirmPay(selectids.join(','), 2, $("#auditMsgBox").val());
                        }
                    }
                }]
        });
    }
}
function ConfirmPay(id, status, msg) {
    $.post('ConfirmPay', { id: id, status: status, remark: msg }, function (result) {
        if (result.success) {
            $.dialog.succeedTips(result.msg);
            $('#searchButton').trigger('click');
            if (result.status == 2 && result.jumpurl) {
                window.open(result.jumpurl);
            }
        }
        else {
            $.dialog.alert(result.msg);
        }
    });
}
//批量操作
function BatchConfirmPay(ids, status, msg) {
    $.post('./BatchConfirmApply', { ids: ids, status: status, remark: msg }, function (result) {
        if (result.success) {
            $.dialog.succeedTips(result.msg);
            $('#searchButton').trigger('click');
            if (result.status == 2 && result.data) {
                window.open(result.data);
            }
        }
        else {
            $.dialog.alert("操作失败:" + result.msg);
        }
    });
}
function CancelPay(Id) {
    $.dialog.confirm("确定取消本次付款，开始新的操作吗？", function () {
        var loading = showLoading();
        $.ajax({
            type: "POST",
            url: "CancelPay",
            data: { Id: Id },
            datatype: "json",
            success: function (data) {
                loading.close();
                if (data.success) {
                    $('#searchButton').trigger('click');
                } else {
                    $.dialog.errorTips(data.msg, _this);
                }
            },
            error: function () {
                loading.close();
            }
        });
    });
}

//导出
function ExportExecl() {
    var shopName = $("#shopName").val();
    var inputStartDate = $("#inputStartDate").val();
    var inputEndDate = $("#inputEndDate").val();    
    var inputStartDates = $("#inputStartDates").val();
    var inputEndDates = $("#inputEndDates").val();

    var href = "/Admin/ShopWithDraw/ExportToExcelShopWithDraw?Status={0}&ApplyStartTime={1}&ApplyEndTime={2}&AuditedStartTime={3}&AuditedEndTime={4}"
        .format(3, inputStartDate, inputEndDate, inputStartDates, inputEndDates);//3成功，1待处理

    $("#aExport").attr("href", href);
}