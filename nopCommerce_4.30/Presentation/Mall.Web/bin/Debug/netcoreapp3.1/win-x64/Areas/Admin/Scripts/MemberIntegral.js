$(function () {
    query();
    $("#searchBtn").click(function () { query(); });
    AutoComplete();
})

function query() {
    $("#list").MallDatagrid({
        url: './list',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到任何的会员积分信息',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "id",
        pageSize: 10,
        pageNumber: 1,
        queryParams: { userName: $("#autoTextBox").val(), startDate: $("#inputStartDate").val(), endDate: $("#inputEndDate").val() },
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        operationButtons: "",
        columns:
        [[
            { field: "id", hidden: true },
            { field: "userId", hidden: true },
            { field: "userName", title: '会员名' },
            { field: "availableIntegrals", title: '可用积分', sort: true },
            { field: "memberGrade", title: '会员等级' },
            { field: "historyIntegrals", title: '历史积分', sort: true },
            {
                field: "createDate", title: '会员注册时间', sort: true,
                formatter: function (value, row, index) {
                    return value.substring(0,10);
                }
            },
            { field: "operation", operation: true, title: "操作",
                formatter: function (value, row, index) {
                    return "<span class=\"btn-a\"><a href='./Detail/" + row.memberId + "'>查看</a></span>";
                }
            }
        ]]
    });
}

function AutoComplete() {
    //autocomplete
    $('#autoTextBox').autocomplete({
        source: function (query, process) {
            var matchCount = this.options.items;//返回结果集最大数量
            $.post("./getMembers", { "keyWords": $('#autoTextBox').val() }, function (respData) {
                return process(respData);
            });
        },
        formatItem: function (item) {
            return item.value;
        },
        setValue: function (item) {
            return { 'data-value': item.value, 'real-value': item.key };
        }
    });
}