var curApplyType = 1;
$(function () {
    curApplyType = $("#hidApplyType").val();
    LoadData(0, '', '');
    $('#searchButton').click(function () {
        var status = $('#applyType').val();
        LoadData(status, $('#inputWithNo').val(), $('#inputWithDrawUser').val())
    });
})
function DoOperate(id, action, msg) {
    if (action == 1) {
        $.dialog({
            title: '查看原因',
            lock: true,
            id: 'showRemrk',
            content: ['<div class="dialog-form">',
                '<div class="form-group">',
                    '<p class="help-esp">备注</p>',
                    '<textarea id="auditMsgBox" class="form-control" cols="40" rows="2"  >' + msg + '</textarea>\
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
        $.dialog({
            title: '审核付款',
            lock: true,
            id: 'goodCheck',
            content: ['<div class="dialog-form">',
                '<div class="form-group">',
                    '<p class="help-esp">备注</p>',
                    '<textarea id="auditMsgBox" class="form-control" cols="40" rows="2"  >' + msg + '</textarea>\
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
                        ConfirmPay(id, 4, $("#auditMsgBox").val());
                    }
                }
            }]
        });
    }
}
function ConfirmPay(id, status, msg) {
    $.post('./ConfirmApply', { id: id, comfirmStatus: status, remark: msg }, function (result) {
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

//批量操作
function BatchConfirmPay(ids, status, msg) {
    $.post('./BatchConfirmApply', { ids: ids, comfirmStatus: status, remark: msg }, function (result) {
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
        $.dialog({
            title: '审核付款',
            lock: true,
            id: 'goodCheck',
            content: ['<div class="dialog-form">',
                '<div class="form-group">',
                '<p class="help-esp">备注</p>',
                '<textarea id="auditMsgBox" class="form-control" cols="40" rows="2"  ></textarea>\
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
                            BatchConfirmPay(selectids.join(','), 4, $("#auditMsgBox").val());
                        }
                    }
                }]
        });
    }
}

function CancelPay(Id) {
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
}


function LoadData(status, withdrawno, username) {

    var dataColumn = [];
    dataColumn.push({
        checkbox: true, width: 39, formatter: function (value, row, index) {
            var html = "<input type=\"checkbox\" ";
            if (row['ApplyStatus'] != 1 && row['ApplyStatus'] != 2) {
                html += ' disabled ';
            }
            html += ">";
            return html;
        } });
    dataColumn.push({ field: "Id", title: '提现单号', width: 100 });
    dataColumn.push({
        field: "ApplyStatusDesc", title: '状态', width: 100, align: 'center'
    });
    dataColumn.push({
        field: "MemberName", title: '提现会员帐号', width: 100, align: 'center'
    });
    dataColumn.push({
        field: "NickName", title: "真实姓名", width: 100, align: "center"
    });
    dataColumn.push({
        field: "ApplyAmount", title: "提现金额", width: 100, align: "center"
    });
    dataColumn.push({
        field: "ApplyTime", title: "申请时间", width: 120, align: "center"
    });
    dataColumn.push({
        field: "ConfirmTime", title: "处理时间", width: 120, align: "center"
    });
    dataColumn.push({
        field: "PayTime", title: "付款时间", width: 100, align: "center"
    });
    dataColumn.push({
        field: "PayNo", title: "付款流水号", width: 100, align: "center"
    });
    dataColumn.push({
        field: "Operate", title: "操作", width: 140, align: "center",
        formatter: function (value, row, index) {
            var id = row['Id'].toString();
            var html = [""];
            switch (parseFloat(row['ApplyStatus'])) {
                case 1:
                    html.push("<span class=\"btn-a\">");
                    html.push("<a onclick='DoOperate(" + row["Id"] + ",0,\"\")'>审核</a>");
                    html.push("</span>");
                    break;
                case 4:
                    html.push("<span class=\"btn-a\">");
                    html.push("<a onclick='DoOperate(" + row["Id"] + ",1,\"" + row["Remark"] + "\")'>查看备注</a>");
                    html.push("</span>");
                    break;
                case 2:
                    html.push("<span class=\"btn-a\">");
                    html.push("<a onclick='DoOperate(" + row["Id"] + ",0,\"\")'>付款</a>");
                    html.push("</span>");
                    html.push("<span class=\"btn-a\">");
                    html.push("<a onclick='DoOperate(" + row["Id"] + ",1,\"" + row["Remark"] + "\")'>查看原因</a>");
                    html.push("</span>");
                    break;
                case 3:
                    html.push("<span class=\"btn-a\">");
                    html.push("<a onclick='DoOperate(" + row["Id"] + ",1,\"" + row["Remark"] + "\")'>查看原因</a>");
                    html.push("</span>");
                    break;
                case 5:
                    html.push("<span class=\"btn-a\">");
                    html.push("<a onclick='CancelPay(" + row["Id"] + ")'>取消</a>");
                    html.push("</span>");
                    break;
            }

            return html.join('');
        }
    });

    var url = 'ApplyWithDrawList';

    $("#list").empty();
    $("#list").MallDatagrid({
        url: url,
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 15,
        pagePosition: 'bottom',
        pageNumber: 1,
        queryParams: { capitalType: status, user: username, withdrawno: withdrawno, applyType: curApplyType },
        columns: [dataColumn],
        operationButtons: "#batchOperate"
    });
}