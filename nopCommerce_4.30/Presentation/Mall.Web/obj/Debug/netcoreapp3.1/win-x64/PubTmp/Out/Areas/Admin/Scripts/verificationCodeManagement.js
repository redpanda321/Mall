﻿$(function () {
    AutoComplete();
    $(".start_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    }).on("click", function () {
        $(".start_datetime").datetimepicker("setEndDate", $(".end_datetime").val());
    });
    $(".end_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    }).on("click", function () {
        $(".end_datetime").datetimepicker("setStartDate", $(".start_datetime").val());
    });
    $(".start_verification_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    }).on("click", function () {
        $(".start_verification_datetime").datetimepicker("setEndDate", $(".end_verification_datetime").val());
    });;
    $(".end_verification_datetime").datetimepicker({
        language: 'zh-CN',
        format: 'yyyy-mm-dd',
        autoclose: true,
        weekStart: 1,
        minView: 2
    }).on("click", function () {
        $(".end_verification_datetime").datetimepicker("setStartDate", $(".start_verification_datetime").val());
    });
});
$(function () {

    list();
    function list() {
        //订单表格
        $("#list").MallDatagrid({
            url: './list',
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
            queryParams: {},
            columns:
            [[
                {
                    field: "OrderId", title: '订单号', width: 120,
                    formatter: function (value) {
                        var html = '<a href="/Admin/Order/Detail/' + value + '" target="_blank">' + value + '</span>';
                        return html;
                    }
                },
                { field: "VerificationCode", title: "核销码", width: 100, align: "center" },
                { field: "StatusText", title: "状态", width: 100, align: "center" },
                { field: "PayDateText", title: "付款时间", width: 100, align: "center" },
                { field: "VerificationTimeText", title: "核销时间", width: 100, align: "center" },
                { field: "Name", title: "商家/门店", width: 100, align: "center" },
                { field: "VerificationUser", title: "核销人", width: 100, align: "center" }
            ]]
        });
    }

    $('#searchButton').click(function (e) {
        var OrderId = $("#OrderId").val();
        var Status = $("#Status").val();
        var PayDateStart = $.trim($('#PayDateStart').val());
        var PayDateEnd = $("#PayDateEnd").val();
        var VerificationCode = $.trim($('#VerificationCode').val());
        var ShopBranchName = $.trim($('#ShopBranchName').val());
        var value = $('#ShopBranchName').attr("real-value");
        var Type; var SearchId;
        if (typeof (value) != "undefined"&&value.length>0) {
            var _value = value.split('@');
            Type = _value[0];
            SearchId = _value[1];
        }
        var VerificationTimeStart = $("#VerificationTimeStart").val();
        var VerificationTimeEnd = $("#VerificationTimeEnd").val();

        $("#list").MallDatagrid('clearReload', { OrderId: OrderId, Status: Status, PayDateStart: PayDateStart, PayDateEnd: PayDateEnd, VerificationCode: VerificationCode, ShopBranchName: ShopBranchName, VerificationTimeStart: VerificationTimeStart, VerificationTimeEnd: VerificationTimeEnd, SearchId: SearchId, Type: Type });
        $('#ShopBranchName').attr("real-value", "");
    });
});
function AutoComplete() {
    //autocomplete
    $('#ShopBranchName').autocomplete({
        source: function (query, process) {
            var matchCount = this.options.items;//返回结果集最大数量
            $.post("/Admin/VerificationCode/GetShopAndShopBranch", { "keyWords": $('#ShopBranchName').val() }, function (respData) {
                return process(respData);
            });
        },
        formatItem: function (item) {
            return item.value;
        },
        setValue: function (item) {
            return { 'data-value': item.value, 'real-value': item.type + '@' + item.id };
        }
    });
}