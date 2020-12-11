var stimebox, etimebox, maxLevel;
var levelName1, levelName2, levelName3;
$(function () {
    stimebox = $("#StartTime");
    etimebox = $('#EndTime');
    maxLevel = $("#maxLevel").val();
    levelName1 = $("#levelName1").val();
    levelName2 = $("#levelName2").val();
    levelName3 = $("#levelName3").val();
    stimebox.datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        startView: 2,
        minView: 2
    });

    etimebox.datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        startView: 2,
        minView: 2
    });
    etimebox.datetimepicker('setStartDate', stimebox.val());
    stimebox.datetimepicker('setEndDate', etimebox.val());

    stimebox.on('changeDate', function () {
        if (etimebox.val()) {
            if (stimebox.val() > etimebox.val()) {
                etimebox.val(stimebox.val());
            }
        }

        etimebox.datetimepicker('setStartDate', stimebox.val());
    });
    etimebox.on('changeDate', function () {
        stimebox.datetimepicker('setEndDate', etimebox.val());
    });

    query();

    $("#searchBtn").click(function () { query(); });


    OperateEventBind();
});

function OperateEventBind() {
    $('#list').on('click', '.btn-remove', function () {
        var _t = $(this);
        var _p = _t.parents('.btn-a');
        var id = _p.data("memberid");
        $.dialog.confirm('清退后，销售员无法进入小店，也不再获得佣金，是否继续？', function () {
            loading = showLoading();
            $.post('/Admin/Distributor/RemoveDistributor', { id: id }, function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.tips('操作成功');
                    reloadQuery();
                }
                else {
                    $.dialog.alert('清退销售员失败!' + result.msg);
                }
            });
        });
    });

    $('#list').on('click', '.btn-recover', function () {
        var _t = $(this);
        var _p = _t.parents('.btn-a');
        var id = _p.data("memberid");
        $.dialog.confirm('确定要恢复此销售员的身份吗？', function () {
            loading = showLoading();
            $.post('/Admin/Distributor/RecoverDistributor', { id: id }, function (result) {
                loading.close();
                if (result.success) {
                    $.dialog.tips('恢复销售员完成');
                    reloadQuery();
                }
                else {
                    $.dialog.alert('恢复销售员失败!' + result.msg);
                }
            });
        });
    });

    $('#list').on('click', '.btn-audit', function () {
        var _t = $(this);
        var _p = _t.parents('.btn-a');
        var id = _p.data("memberid");
        AuditDialog(id);
    });
    $('#list').on('click', '.btn-vremark', function () {
        var _t = $(this);
        var r = _t.data("remark");
        $.dialog({
            title: '查看原因',
            width: 200,
            lock: true,
            id: 'distributorView',
            content: ['<div>' + r + '</div>'].join(''),
            padding: '20px',
            button: [
                {
                    name: '关闭',
                    focus: true
                }]
        });
    });
}

function AuditDialog(ids) {
    $.dialog({
        title: '销售员审核',
        lock: true,
        id: 'distributorCheck',
        content: ['<div class="dialog-form">',
            '<div class="form-group">',
            '<p class="help-esp">备注</p>',
            '<textarea class="form-control" type="text" name="txtRemark" id="txtRemark"/></textarea>',
            '<span class="field-validation-error" id="remarkCotentTip"></span> ',
            '</div>',
            '</div>'].join(''),
        padding: '0 40px',
        init: function () { $("#txtRemark").focus(); },
        button: [
            {
                name: '通过审核',
                callback: function () {
                    Audit(ids, $('#txtRemark').val(), true);
                },
                focus: true
            }, {
                name: '拒绝',
                callback: function () {
                    var remark = $("#txtRemark").val();
                    if (remark.length < 1) {
                        $("#remarkCotentTip").text("请输入拒绝的审核通过的理由");
                        return false;
                    }
                    if (remark.length > 200) {
                        $("#remarkCotentTip").text("备注内容在200个字符以内");
                        return false;
                    }
                    Audit(ids, $('#txtRemark').val(), false);
                },
            }]
    });
}

function Audit(ids, remark, isAudit) {
    ids = ids + "";
    loading = showLoading();
    var title = isAudit ? "审核" : "拒绝";
    var status = isAudit ? 2 : 3;
    $.post('/Admin/Distributor/AuditDistributor', { ids: ids, remark: remark, status: status }, function (result) {
        loading.close();
        if (result.success) {
            $.dialog.tips(title + '申请操作成功');
            reloadQuery();
        }
        else {
            $.dialog.alert(title + '销售员申请失败!' + result.msg);
        }
    });
}

function reloadQuery() {
    var pageNo = $("#list").MallDatagrid('options').pageNumber;
    query(pageNo);
}

function query(page) {
    var search = $("#from_search").inputToJson();
    page = page || 1;
    $("#list").MallDatagrid({
        url: '/Admin/Distributor/GetDistributorList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "MemberId",
        pageSize: 10,
        pageNumber: page,
        queryParams: search,
        operationButtons: "#operatebox",
        columns:
        [[
            { field: "MemberId", checkbox: true, width: 40, },
            { field: "MemberName", title: '会员名' },
            { field: "ShopName", title: '小店名称', width: 120 },
            { field: "GradeName", title: '销售员等级', width: 80 },
            { field: "SuperiorMemberName", title: '上一级销售员', width: 120 },
            {
                field: "SubNumber", title: '下级发展数', width: 100,
                formatter: function (value, row, index) {
                    var html = "";
                    html += levelName1 + '：' + row.SubNumber;
                    if (maxLevel > 1) {
                        html += '<br>' + levelName2 + '：' + row.SubNumber2;
                    }
                    if (maxLevel > 2) {
                        html += '<br>' + levelName3 + '：' + row.SubNumber3;
                    }
                    return html;
                }
            },
            { field: "SettlementAmount", title: '已结算佣金总额', width: 150, sort: true },
            { field: "ShowDistributionStatus", title: '状态', width: 80 },
            { field: "ShowApplyTime", title: '申请时间', width: 155, sort: true },
            {
                field: "s", title: "操作", width: 90, align: "center",
                formatter: function (value, row, index) {
                    var html = "";
                    html += '<span class="btn-a" data-memberid="' + row.MemberId + '" data-status="' + row.DistributionStatus + '">';
                    html += '<a href="/Admin/Distributor/Detail/' + row.MemberId + '">详情</a>';
                    if (row.DistributionStatus == 2) {
                        html += '<a class="btn-remove">清退</a>';
                    }
                    if (row.DistributionStatus == 4) {
                        html += '<a class="btn-recover">恢复</a>';
                    }
                    if (row.DistributionStatus == 1) {
                        html += '<a class="btn-audit">审核</a>';
                    }
                    if (row.DistributionStatus != 3) {
                        html += '<a class="btn-changesuper" href="javascript:ShowChangeSuperDlg(' + index + ')">调整上级</a>';
                    }
                    if (row.DistributionStatus == 3) {
                        html += '<a class="btn-vremark" data-remark="' + row.Remark + '">查看原因</a>';
                    }
                    html += '</span>';
                    return html;
                }
            }
        ]],
        onLoadSuccess: function () {
            initBatchBtnShow();
        }
    });
}

function ShowChangeSuperDlg(rowIndex) {
    var data = $("#list").MallDatagrid('getRowByIndex', rowIndex);
    $.dialog({
        title: '调整所属上级',
        lock: true,
        width: 550,
        padding: '0 5px',
        id: 'ChangeSuperDlg',
        content: $('#ChangeSuperDlgTmp')[0],
        init: function () {
            $("#spn_membername").html(data.MemberName);
            $("#spn_supername").html(data.SuperiorMemberName);
            $("#searchMemberName").val("");
            $('#ChangeSuperDlgTmp').show();
            QueryTopList(data.MemberId);
            $("#btnSearchTop").click(function () {
                return function (MemberId) {
                    QueryTopList(MemberId);
                }(data.MemberId);
            });
        }
    });
}

function QueryTopList(memberId) {
    //优惠券表格
    $("#DistributorSuperGrid").MallDatagrid({
        url: '/Admin/Distributor/GetCanSuperDistributorList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "MemberId",
        pageSize: 6,
        pageNumber: 1,
        queryParams: { memberId: memberId, memberName: $("#searchMemberName").val() },
        operationButtons: "#operatebox",
        columns:
        [[
            { field: "MemberName", title: '销售员名称' },
            { field: "GradeName", title: '销售员等级', width: 150 },
            {
                field: "s", title: "操作", width: 120, align: "center",
                formatter: function (value, row, index) {
                    var html = "";
                    html += '<span class="btn-a">';
                    html += '<a href="javascript:ChangeSuper(' + memberId + ',' + row.MemberId + ')">设为上级</a>';
                    html += '</span>';
                    return html;
                }
            }
        ]]
    });
}

function ChangeSuper(memberId, superId) {
    $.dialog.confirm('调整后，后续新产生的订单将按照新的上下级进行结算，已产生的订单不受影响。确定要调整所属上级吗？', function () {
        loading = showLoading();
        $.post('/Admin/Distributor/ChangeSuper', { id: memberId, superId: superId }, function (result) {
            loading.close();
            if (result.success) {
                $.dialog.tips('调整上级完成');
                reloadQuery();
            }
            else {
                $.dialog.alert('调整上级失败!' + result.msg);
            }
        });
        $.dialog({ id: 'ChangeSuperDlg' }).close();
    });
}

function initBatchBtnShow() {

    $('#batOperateSel').on("change", function () {
        var type = $(this).val();
        $(this).val('0');
        if (type > 0) {
            var selecteds = $("#list").MallDatagrid('getSelections');
            var ids = [];
            var states = [];
            $.each(selecteds, function () {
                ids.push(this.MemberId);
                states.push(this.DistributionStatus);
            });
            if (ids.length == 0) {
                $.dialog.alert('请至少选择一个销售员！');
                return;
            }
            type = parseInt(type);
            switch (type) {
                case 1:
                    var isok = true;
                    states.forEach(function (v) {
                        if (v != 1) {
                            isok = false;
                        }
                    });
                    if (!isok) {
                        $.dialog.alert('请正确勾选待审核的销售员！');
                        return;
                    }
                    AuditDialog(ids.join(',').toString());
                    break;
                case 2:
                    var isok = true;
                    states.forEach(function (v) {
                        if (v != 2) {
                            isok = false;
                        }
                    });
                    if (!isok) {
                        $.dialog.alert('请正确勾选已审核的销售员！');
                        return;
                    }
                    $.dialog.confirm('清退后，销售员无法进入小店，也不再获得佣金，是否继续？', function () {
                        loading = showLoading();
                        $.post('/Admin/Distributor/RemoveDistributorList', { ids: ids.join(',').toString() }, function (result) {
                            loading.close();
                            if (result.success) {
                                $.dialog.tips('操作成功');
                                reloadQuery();
                            }
                            else {
                                $.dialog.alert('批量清退销售员失败!' + result.msg);
                            }
                        });
                    });
                    break;
                case 3:
                    var isok = true;
                    states.forEach(function (v) {
                        if (v != 4) {
                            isok = false;
                        }
                    });
                    if (!isok) {
                        $.dialog.alert('请正确勾选已清退的销售员！');
                        return;
                    }
                    $.dialog.confirm('确定要恢复此销售员的身份吗？', function () {
                        loading = showLoading();
                        $.post('/Admin/Distributor/RecoverDistributorList', { ids: ids.join(',').toString() }, function (result) {
                            loading.close();
                            if (result.success) {
                                $.dialog.tips('操作成功');
                                reloadQuery();
                            }
                            else {
                                $.dialog.alert('批量恢复销售员失败!' + result.msg);
                            }
                        });
                    });
                    break;
            }
        }
    });
}

//导出
function ExportExecl() {
    var memberName = $("#MemberName").val();
    var shopName = $("#ShopName").val();
    var gradeId = $("#GradeId").val();
    var superiorMemberName = $("#SuperiorMemberName").val();
    var startTime = $("#StartTime").val();
    var endTime = $("#EndTime").val();
    var status = $("#Status").val();

    var href = "/Admin/Distributor/ExportToExcelDistributor?memberName={0}&shopName={1}&gradeId={2}&superiorMemberName={3}&startTime={4}&endTime={5}&status={6}"
        .format(memberName, shopName, gradeId, superiorMemberName, startTime, endTime, status);
    
    $("#aExport").attr("href", href);
}
