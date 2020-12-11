// JavaScript source code
$(function () {
    loadGrid();

    $("#btnSearch").click(function () {
        $("#list").MallDatagrid('reload', { type: $("#searchType").val(), state: $("#searchState").val(), name: $("#searchName").val() })
    })
});

function loadGrid() {
    var search = $("#from_search").inputToJson();
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
        pageSize: 20,
        pageNumber: 1,
        queryParams: search,
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        columns:
        [[
            { field: "name", title: "活动名称" },
            { field: "typeStr", title: "红包类型" },
            { field: "totalPrice", title: "总面额" ,sort:true},
            { field: "receiveCount", title: "领取人数", sort: true },
            {
                field: "startTime", title: "开始日期", sort: true, formatter: function(value) {
                    return value.substring(0,10);
                } },
            {
                field: "endTime", title: "结束时间", sort: true, formatter:function(value) {
                    return value.substring(0, 10);
                } },
            {
                field: "isInvalid", title: "状态", formatter: function (value, row, index) {
                    return row.stateStr;
                }
            },
            {
                field: "operation", operation: true, title: "操作", formatter: function (value, row, index) {
                    var html = "";
                    html += '<span class="btn-a"><a href="/Admin/Bonus/Detail/' + row.id + '">详情</a></span>';

                    var str = row.endTime.substring(0,10) + ' 23:59:59';
                    str = str.replace(/-/g, "/");
                    var enddate = new Date(str);
                    if (new Date() > enddate) {
                        return html;
                    }
                    if (!row.isInvalid || row.startTime > new Date()) {
                        html += '<span class="btn-a"><a href="/Admin/Bonus/Edit/' + row.id + '">编辑</a></span>';
                    }
                    if (!row.isInvalid) {
                        if (row.type == 1) {
                            html += '<span class="btn-a"><a href="/Admin/Bonus/Apportion/' + row.id + '">发放</a></span>';
                        }
                        html += '<span class="btn-a"><a onclick="invalid(' + row.id + ' , ' + row.isInvalid + ');">失效</a></span>';
                    }

                    return html;
                }
            }
        ]]
    });
}

function invalid(id, isInvalid) {
    if (isInvalid) {
        $.dialog.tips('此活动已失效!');
        return;
    }

    $.dialog.confirm('您确定要失效此活动？', function () {
        var loading = showLoading();
        $.post("/Admin/Bonus/Invalid", { id: id }, function () {
            $.dialog.tips('已成功失效此活动');
            var pageNo = $("#list").MallDatagrid('options').pageNumber;
            $("#list").MallDatagrid('reload', { pageNumber: pageNo });
            loading.close();
        })
    })
}