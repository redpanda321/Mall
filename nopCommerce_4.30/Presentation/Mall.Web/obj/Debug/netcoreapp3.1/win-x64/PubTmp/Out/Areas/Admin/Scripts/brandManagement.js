//$(function () {
//    $("#searchBtn").click(function () { query(); });
//    AutoComplete();
	
//})


function deleteBrand(id,name)
{
        var loading = showLoading();
        $.post("./GetProductNum", { brandName: name }, function (data) {
            if (data.success == true) {
                if (data.data.ProductNum > 0) {
                    loading.close();
                    $.dialog.confirm('此品牌下正在销售的商品有 ' + data.data.ProductNum + ' 件(<a target="_blank" href="/admin/product/management?brandName=' + name+'">点击查看</a>)， 删除后商品将不再有品牌属性，确认要删除吗？', function () {
                        var loading = showLoading();
                        $.post("./Delete", { id: id }, function (data) {
                            $.dialog.tips(data.msg);
                            query();
                            loading.close();
                        });
                    });
                }
                else {
                    $.post("./Delete", { id: id }, function (data) {
                        $.dialog.tips(data.msg);
                        query();
                        loading.close();
                    });
                }
            }
        });
        
}


function audit(id) {
    var loading = showLoading();
    $.post("./Audit", { id: id }, function (data) {
        $.dialog.tips(data.msg,function(){location.href = "./Management";});
        loading.close();
        
    });
}

function refuse(id) {

    $.dialog({
        title: '拒绝审批',
        lock: true,
        id: 'goodCheck',
        content: ['<div class="dialog-form">',
            '<div class="form-group">',
                '<textarea id="txtRemark" class="form-control" cols="40" rows="2" onkeyup="this.value = this.value.slice(0, 50)" ></textarea>\
                 <p id="valid" style="visibility:hidden;color:red">请填写拒绝备注</p> ',
            '</div>',
        '</div>'].join(''),
        padding: '10px',
        init: function () { $("#txtPayRemark").focus(); },
        button: [{
            name:"取消"
        },
        {
            name: '确定',
            callback: function () {
                var remark = $("#txtRemark").val();
                if (remark.length == 0) {
                    $("#valid").css({ "visibility": "visible" });
                    return false;
                }
                var loading = showLoading();
                $.post("./Refuse", { id: id, remark:remark }, function (data) {
                    $.dialog.tips(data.msg, function () { location.href = "./Management"; });
                    loading.close();
                });
            },
            focus: true
        }]
    });
}

function deleteApply(id) {
    $.dialog.confirm('您确定删除该申请吗？', function () {
        var loading = showLoading();
        $.post("./DeleteApply", { id: id }, function (data) {
            $.dialog.tips(data.msg); queryApply();
            loading.close();
        });
    });
}


function query() {
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
        pageNumber: 1,
        queryParams: { keyWords: $("#brandBox").val()},
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        columns:
        [[
            { field: "Id", hidden: true },
            { field: "BrandDesc", title: '名称' },
            {
                field: "BrandLogo", title: 'LOGO', align: 'center',
                formatter: function (value, row, index) {
                    var html = "";
                    html += '<img style="width="100" height="24" style="float:none!important" src="' + row.BrandLogo + '" />';
                    return html;
                }
            },
        {
            field: "operation", operation: true, title: "操作",
            formatter: function (value, row, index) {
                var id = row.ID.toString();
                var html = ["<span class=\"btn-a\">"];
                if (row.AuditStatus == 1)//仅未审核的品牌需要审核
                    html.push("<a class=\"good-check\" onclick=\"audit('" + id + "')\">审核</a>");
                html.push("<a href='./Edit/" + id + "'>编辑</a>");
                html.push("<a onclick=\"deleteBrand('" + id + "','" + row.BrandName + "');\">删除</a>");
                html.push("</span>");
                return html.join("");
            }
        }
        ]]
    });
}


function AutoComplete() {
    //autocomplete
    $('#brandBox').autocomplete({
        source: function (query, process) {
            var matchCount = this.options.items;//返回结果集最大数量
            $.post("./getBrands", { "keyWords": $('#brandBox').val(), AuditStatus: $("#Audits").parent().attr("class") == "active" ?1:2}, function (respData) {
                return process(respData);
            });
        },
        formatItem: function (item) {
            if (item.envalue != null)
            {
                return item.value + "(" + item.envalue + ")";
            }
            return item.value;  
        },
        setValue: function (item) {
            return { 'data-value': item.value, 'real-value': item.key };
        }
    });
}

function queryApply() {
    $("#applyList").MallDatagrid({
        url: './ApplyList',
        nowrap: false,
        rownumbers: true,
        NoDataMsg: '没有找到符合条件的数据',
        border: false,
        fit: true,
        fitColumns: true,
        pagination: true,
        idField: "Id",
        pageSize: 15,
        pageNumber: 1,
        queryParams: { keyWords: $("#brandBox").val() },
        toolbar: /*"#goods-datagrid-toolbar",*/'',
        columns:
        [[
            { field: "Id", hidden: true },
            { field: "ShopName", title: '申请经营方' },
            { field: "BrandName", title: '品牌名称' },
            { field: "ApplyTime", title: '申请时间' },
            //{
            //    field: "BrandLogo", title: 'LOGO', align: 'center',
            //    formatter: function (value, row, index) {
            //        var html = "";
            //        html += '<img style="width="100" height="24" src="' + row.BrandLogo + '?' + Date() + '" />';
            //        return html;
            //    }
            //},
            {
                field: "operation", operation: true, title: "操作",
                formatter: function (value, row, index) {
                    var id = row.Id.toString();
                    var html = ["<span class=\"btn-a\">"];
                    if (row.AuditStatus == 0)//仅未审核的品牌需要审核
                    {
                        //html.push("<a class=\"good-check\" onclick=\"audit('" + id + "')\">审核通过</a>");
                        //html.push("<a class=\"good-check\" onclick=\"refuse('" + id + "')\">拒绝通过</a>");
                        html.push("<a href='./Show?id=" + id + "'>审核</a>");
                    }

                    html.push("<a onclick=\"deleteApply('" + id + "');\">删除</a>");
                    html.push("</span>");
                    return html.join("");
                }
            }
        ]]
    });
};
$(function () {
    $("#uploadImg").MallUpload(
  {
      imgFieldName: "BrandLogo"
  });
});
$(function () {
    query();
    $("#searchBtn").click(function () {
        if ($("#brandBox").val().length > 20) {
            $.dialog.tips("搜索内容不要超过20个文字！");
            return false;
        }
        query();
    });
    AutoComplete();
});